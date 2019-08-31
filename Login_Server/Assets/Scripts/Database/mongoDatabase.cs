using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using UnityEngine;

public static class COLLECTIONS
{
    public const string ACCOUNTS = "accounts";
    public const string PLAYERS = "players";
    public const string WORKERS = "workers";
    public const string SHIPS = "ships";
}

public class MongoDatabase
{

    //TODO: abstract DB connection so different servers can connect as different users

    //connection&db variables
    private const string USERNAME = "loginServer";
    private const string PASSWORD = "3Amxqxx6n8EbltzG";
    private const string DB_NAME = "LostColoniesDB";
    private const string DB_IP = "34.83.164.162";
    private const string DB_PORT = "27017";

    private const string MONGO_URI = "mongodb://" + USERNAME + ":" + PASSWORD + "@" + DB_IP + ":" + DB_PORT + "/" + DB_NAME;

    private MongoClient client;
    private MongoServer server;
    private MongoDB.Driver.MongoDatabase mdb;

    private MongoCollection<DB_Account> accountCollection;
    private MongoCollection<DB_Player> playerCollection;
    private MongoCollection<DB_Worker> workerCollection;
    private MongoCollection<DB_Ship> shipCollection;
    
    #region NetworkBehavior
    public void Init()
    {
        client = new MongoClient(MONGO_URI);
        server = client.GetServer();
        mdb = server.GetDatabase(DB_NAME);

        //init collections(tables) here
        accountCollection = mdb.GetCollection<DB_Account>(COLLECTIONS.ACCOUNTS);
        playerCollection = mdb.GetCollection<DB_Player>(COLLECTIONS.PLAYERS);
        workerCollection = mdb.GetCollection<DB_Worker>(COLLECTIONS.WORKERS);
        shipCollection = mdb.GetCollection<DB_Ship>(COLLECTIONS.SHIPS);

        Debug.Log("Initialized " + DB_NAME);
        
        //TODO remove this
        //setting ship0 to NONE, so player can request sail
        DB_Ship ship = FetchShipByName("ship0");
        ship.activity = SHIP_ACTIVITY.NONE;
        updateShip(ship);
        ship = FetchShipByName("ship1");
        ship.activity = SHIP_ACTIVITY.NONE;
        updateShip(ship); ship = FetchShipByName("ship2");
        ship.activity = SHIP_ACTIVITY.NONE;
        updateShip(ship);

        /*
        for (int i = 0; i < 50; i++)
        {
            DB_Ship ship2 = new DB_Ship();
            ship2.shipName = "ship" + i;
            ship2.sector = SECTOR_CODE.REDSECTOR;
            ship2.velX = 0;
            ship2.velY = 0;
            ship2.posX = i + 5;
            ship2.posY = 5;
            ship2.frame = 0;
            ship2.color1 = 1;
            ship2.color2 = 2;
            ship2.color3 = 3;
            ship2.activity = SHIP_ACTIVITY.NONE;
            ship2.workersOnBoard = new List<MongoDBRef>();
            shipCollection.Insert(ship2);
        } */
        

    }
    public void Shutdown()
    {
        client = null;
        server.Shutdown();
        mdb = null;
    }
    #endregion

    #region Insert
    public bool CreateAccount(string username, string hashedPassword, string email)
    {
        //*********************************Account creation*********************************
        DB_Account account = new DB_Account();

        //validate email
        //TODO: send verification e-mail
        if (!Utility.IsEmail(email))
        {
            Debug.Log(string.Format("[Login Server]: Bad e-mail: ({0})",email));
            return false;
        }
        if (FetchAccountByEmail(email) != null)
        {
            Debug.Log(string.Format("[Login Server]: E-mail taken: ({0})", email));
            return false;
        }
        account.Email = email;

        //validate username
        if (!Utility.IsUsername(username))
        {
            Debug.Log(string.Format("[Login Server]: Bad username: ({0})", username));
            return false;
        }
        account.Username = username;

        //TODO check password validity (don't delete me future Darren...)
        account.ShaPassword = hashedPassword;

        //get discriminator
        int discriminatorInt = 0;
        while(FetchAccountByUsernameAndDiscriminator(username+"#"+discriminatorInt.ToString().PadLeft(4,'0')) != null)
        {
            discriminatorInt++;
        }
        account.Discriminator = discriminatorInt.ToString().PadLeft(4, '0');

        //default to offline status
        account.Status = LoginStatus.Offline;

        //TODO: dynamically assign default sector
        account.Sector = SECTOR_CODE.REDSECTOR;

        account.CreatedOn = DateTime.Now;
        account.LastLogin = account.CreatedOn;
        //*********************************************************************************

        //*********************************Player creation*********************************
        DB_Player player = new DB_Player();

        player.Username = account.Username;
        player.Discriminator = account.Discriminator;
        player.CreatedOn = account.CreatedOn;
        player.LastLogin = account.CreatedOn;

        //TODO: change up this worker generation so that we just call a createWorker(DB_Worker worker) method
        //assign default worker for new player
        //TODO: allow this worker to be customized (appearance/name)
        player.Workers = new List<MongoDBRef>();
        player.Workers.Add(SpawnDefaultWorker("DEFAULT WORKER NAME" + UnityEngine.Random.Range(0, 10000)));
        player.ActiveWorkerIndex = 0;
        //*********************************************************************************

        //now that all data is validated and set:
            //insert account
        accountCollection.Insert(account);
            //insert player
        playerCollection.Insert(player);

        //set default worker's owner ref
            //get player from db (for db ref)
        player = FetchPlayerByUsernameAndDiscriminator(account.Username + "#" + account.Discriminator);
            //get and update worker
        DB_Worker worker = FetchWorkerByID(player.Workers[player.ActiveWorkerIndex].Id.AsObjectId);
        worker.owner = new MongoDBRef(COLLECTIONS.PLAYERS, player._id);
        updateWorker(worker);

        //TODO: remove this TESTTTTTTTTTTTTTTTTTTTTTTTTTTTTT
        //set default ship's owner ref
            //get and update ship
        DB_Ship ship = FetchShipByID(worker.location.Id.AsObjectId);
        ship.Owner = new MongoDBRef(COLLECTIONS.PLAYERS, player._id);
        updateShip(ship);

        return true;
    }

    public MongoDBRef SpawnDefaultWorker(string workerName)
    {
        DB_Worker defaultWorker = new DB_Worker();
        defaultWorker.activity = WORKER_ACTIVITY.IDLE;
        defaultWorker.workerName = workerName;

        //TODO: change default spawn location
        defaultWorker.location = GETNEXTAVAILABLESHIPDBREFERENCETESTTTT();
        
        //stats
        //**
        //TODO populate
        //**

        //appearance
        //**
        //TODO populate
        //**

        workerCollection.Insert(defaultWorker);

        //Add player reference to default spawn location
            //get location
            //TODO: make default spawn location a colony, not a ship (?)
        DB_Ship ship = FetchShipByID(defaultWorker.location.Id.AsObjectId);
            //get worker from db (for db ref)
        defaultWorker = FetchWorkerByName(workerName);
            //update location
        ship.workersOnBoard.Add(new MongoDBRef(COLLECTIONS.WORKERS, defaultWorker._id));
        updateShip(ship);

        return new MongoDBRef("workers",workerCollection.FindOne(Query<DB_Worker>.EQ(w => w.workerName, workerName))._id);
    }

    private MongoDBRef GETNEXTAVAILABLESHIPDBREFERENCETESTTTT()
    {
        DB_Ship ship;
        string shipName = "ship0";
        var query = Query<DB_Ship>.EQ(s => s.shipName, shipName);

        for (int i = 1; i < 50; i++)
        {
            ship = FetchShipByName(shipName);

            if(ship != null && ship.Owner == null && ship.workersOnBoard.Count == 0)
            {
                return new MongoDBRef(shipCollection.Name, ship._id);
            }

            shipName = "ship" + i;
            query = Query<DB_Ship>.EQ(s => s.shipName, shipName);
        }

        Debug.Log("Alpha version is limited to 50 ships... reset db to try again.");
        return null;
    }

    public DB_Account LoginAccount(int connectionID, int recHostID, string usernameOrEmail, string hashedPassword, string token)
    {
        DB_Account account = null;
        IMongoQuery query = null;

        if (Utility.IsEmail(usernameOrEmail))
        {
            query = Query.And(
                Query<DB_Account>.EQ(u => u.Email, usernameOrEmail),
                Query<DB_Account>.EQ(u => u.ShaPassword, hashedPassword)
            );

            //TODO: veryify only one is found
            account = accountCollection.FindOne(query);
        }
        else if (Utility.IsUsername(usernameOrEmail))//TODO include discriminator
        {
            query = Query.And(
                Query<DB_Account>.EQ(u => u.Username, usernameOrEmail),
                Query<DB_Account>.EQ(u => u.ShaPassword, hashedPassword)
            );

            //TODO: veryify only one is found
            account = accountCollection.FindOne(query);
        }

        //check if valid login credentials
        if(account != null)
        {
            //successful login

            //update account entry
            account.ActiveConnectionID = connectionID;
            account.Token = token;
            account.Status = 1;
            account.LastLogin = DateTime.Now;

            accountCollection.Update(query, Update<DB_Account>.Replace(account));

        }
        else
        {
            Debug.Log("[Login Server]: Invalid login attempted.");
        }

        return account;

    }
    #endregion

    #region Fetch

    #region Account
    public DB_Account FetchAccountByToken(string token)
    {
        var query = Query<DB_Account>.EQ(u => u.Token, token);
        return accountCollection.FindOne(query);
    }
    public DB_Account FetchAccountByEmail(string email)
    {
        var query = Query<DB_Account>.EQ(u => u.Email, email);
        return accountCollection.FindOne(query);
    }
    public DB_Account FetchAccountByUsernameAndDiscriminator(string usernameAndDiscriminator)
    {
        string[] userDiscrim = usernameAndDiscriminator.Split('#');
        var query = Query.And(
                Query<DB_Account>.EQ(u => u.Username, userDiscrim[0]),
                Query<DB_Account>.EQ(u => u.Discriminator, userDiscrim[1])
            );
        return accountCollection.FindOne(query);
    }
    #endregion

    #region Player
    public DB_Player FetchPlayerByToken(string token)
    {
        var query = Query<DB_Player>.EQ(p => p.Token, token);
        return playerCollection.FindOne(query);
    }

    public DB_Player FetchPlayerByID(ObjectId _id)
    {
        var query = Query<DB_Player>.EQ(p => p._id, _id);
        return playerCollection.FindOne(query);
    }

    public DB_Player FetchPlayerByUsernameAndDiscriminator(string usernameAndDiscriminator)
    {
        string[] userDiscrim = usernameAndDiscriminator.Split('#');
        var query = Query.And(
                Query<DB_Player>.EQ(p => p.Username, userDiscrim[0]),
                Query<DB_Player>.EQ(p => p.Discriminator, userDiscrim[1])
            );
        return playerCollection.FindOne(query);
    }
    #endregion

    #region Worker
    public DB_Worker FetchWorkerByName(string workerName)
    {
        var query = Query<DB_Worker>.EQ(w => w.workerName, workerName);
        return workerCollection.FindOne(query);
    }

    public DB_Worker FetchWorkerByID(ObjectId _id)
    {
        var query = Query<DB_Worker>.EQ(w => w._id, _id);
        return workerCollection.FindOne(query);
    }

    public List<DB_Worker> FetchWorkersByPlayer(DB_Player player)
    {
        List<DB_Worker> workers = new List<DB_Worker>();
        player.Workers.ForEach(delegate (MongoDBRef workerRef)
        {
            workers.Add(FetchWorkerByID(workerRef.Id.AsObjectId));
        });
        return workers;
    }

    public List<DB_Worker> FetchWorkersByShip(DB_Ship ship)
    {
        List<DB_Worker> workers = new List<DB_Worker>();
        ship.workersOnBoard.ForEach(delegate (MongoDBRef workerRef)
        {
            workers.Add(FetchWorkerByID(workerRef.Id.AsObjectId));
        });
        return workers;
    }
    #endregion

    #region Ship
    public DB_Ship FetchShipByName(string shipName)
    {
        var query = Query<DB_Ship>.EQ(s => s.shipName, shipName);
        return shipCollection.FindOne(query);
    }
    public DB_Ship FetchShipByID(ObjectId _id)
    {
        var query = Query<DB_Ship>.EQ(s => s._id, _id);
        return shipCollection.FindOne(query);
    }
    #endregion

    /*      FETCH ALL******************************
     * public List<Model> FetchAll(){
     *      List<Model> listName = new List<Model>();
     *      foreach(var m in collection.Find(query)){
     *          listName.Add(m); //LLAPI (8/9 @ 54:00)
     *      }
     *      return listName;
     * }
     */

    #endregion

    #region Update
    public void updateAccount(DB_Account accountUpdate)
    {
        IMongoQuery query = Query<DB_Account>.EQ(a => a._id, accountUpdate._id);
        accountCollection.Update(query, Update<DB_Account>.Replace(accountUpdate));
    }

    public void updatePlayer(DB_Player playerUpdate)
    {
        IMongoQuery query = Query<DB_Player>.EQ(p => p._id, playerUpdate._id);
        playerCollection.Update(query, Update<DB_Player>.Replace(playerUpdate));
    }

    public void updateWorker(DB_Worker workerUpdate)
    {
        IMongoQuery query = Query<DB_Worker>.EQ(w => w._id, workerUpdate._id);
        workerCollection.Update(query, Update<DB_Worker>.Replace(workerUpdate));
    }

    public void updateShip(DB_Ship shipUpdate)
    {
        IMongoQuery query = Query<DB_Ship>.EQ(s => s._id, shipUpdate._id);
        shipCollection.Update(query, Update<DB_Ship>.Replace(shipUpdate));
    }
    #endregion

    #region Delete
    //this is just here to show remove syntax
    public void RemoveAccountByEmail(string email)
    {
        ObjectId id = FetchAccountByEmail(email)._id;
        accountCollection.Remove(Query<DB_Account>.EQ(u => u._id, id));
    }
    #endregion


}
