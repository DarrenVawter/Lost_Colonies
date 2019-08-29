using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using UnityEngine;

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

    private MongoCollection<Model_Account> accountCollection;
    private MongoCollection<Model_Player> playerCollection;
    private MongoCollection<Model_Worker> workerCollection;
    private MongoCollection<Model_Ship> shipCollection;
    
    #region NetworkBehavior
    public void Init()
    {
        client = new MongoClient(MONGO_URI);
        server = client.GetServer();
        mdb = server.GetDatabase(DB_NAME);

        //init collections(tables) here
        accountCollection = mdb.GetCollection<Model_Account>("accounts");
        playerCollection = mdb.GetCollection<Model_Player>("players");
        workerCollection = mdb.GetCollection<Model_Worker>("workers");
        shipCollection = mdb.GetCollection<Model_Ship>("ships");

        Debug.Log("Initialized " + DB_NAME);

        //TODO REMOVE!
        /*
        for (int i = 0; i < 20; i++)
        {
            Model_Ship ship = new Model_Ship();
            ship.shipName = "ship" + i;
            ship.isBusy = false;
            ship.isDocked = false;
            ship.sector = 1;
            ship.velX = 0;
            ship.velY = 0;
            ship.posX = i + 5;
            ship.posY = 5;
            shipCollection.Insert(ship);
        }        
        */

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
        Model_Account account = new Model_Account();

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
        account.Sector = SectorCode.RedSector;

        account.CreatedOn = DateTime.Now;
        account.LastLogin = account.CreatedOn;
        //*********************************************************************************

        //*********************************Player creation*********************************
        Model_Player player = new Model_Player();

        player.Username = account.Username;
        player.Discriminator = account.Discriminator;
        player.CreatedOn = account.CreatedOn;
        player.LastLogin = account.CreatedOn;
        player.Sector = account.Sector;
        player.Activity = PlayerActivity.Offline;

        //assign default worker for new player
        //TODO: allow this worker to be customized
        //TODO: set this worker's owner
        player.Workers = new List<MongoDBRef>();
        player.Workers.Add(SpawnDefaultWorker("DEFAULTTESTNAME" + UnityEngine.Random.Range(0, 10000)));
        //*********************************************************************************
        
        //now that all data is validated and set:
            //insert account
        accountCollection.Insert(account);
            //insert player
        playerCollection.Insert(player);
        
        return true;
    }

    public MongoDBRef SpawnDefaultWorker(string workerName)
    {
        Model_Worker defaultWorker = new Model_Worker();
        defaultWorker.isInCombat = false;
        defaultWorker.activity = WorkerActivity.Idle;
        
        //TODO: change default spawn location
        defaultWorker.location = GETNEXTAVAILABLESHIPDBREFERENCETESTTTT();
        defaultWorker.locationName = shipCollection.FindOne(Query<Model_Ship>.EQ(s => s._id, defaultWorker.location.Id.AsObjectId)).shipName;
        
        //TODO: generate name for worker
        defaultWorker.workerName = workerName;

        //stats
        //**
        //TODO populate
        //**

        //appearance
        //**
        //TODO populate
        //**

        workerCollection.Insert(defaultWorker);
        return new MongoDBRef("workers",workerCollection.FindOne(Query<Model_Worker>.EQ(w => w.workerName, workerName))._id);
    }

    private MongoDBRef GETNEXTAVAILABLESHIPDBREFERENCETESTTTT()
    {
        Model_Ship ship;
        string shipName = "ship0";
        var query = Query<Model_Ship>.EQ(s => s.shipName, shipName);

        for (int i = 1; i < 50; i++)
        {
            ship = FetchShipByName(shipName);

            if(ship != null)
            {
                return new MongoDBRef(shipCollection.Name, ship._id);
            }

            shipName = "ship" + i;
            query = Query<Model_Ship>.EQ(s => s.shipName, shipName);
        }

        Debug.Log("Alpha version is limited to 50 ships... reset db to try again.");
        return null;
    }

    public Model_Account LoginAccount(string usernameOrEmail, string hashedPassword, int connectionID, string token)
    {
        Model_Account account = null;
        IMongoQuery query = null;

        if (Utility.IsEmail(usernameOrEmail))
        {
            query = Query.And(
                Query<Model_Account>.EQ(u => u.Email, usernameOrEmail),
                Query<Model_Account>.EQ(u => u.ShaPassword, hashedPassword)
            );

            //TODO: veryify only one is found
            account = accountCollection.FindOne(query);
        }
        else if (Utility.IsUsername(usernameOrEmail))//TODO include discriminator
        {
            query = Query.And(
                Query<Model_Account>.EQ(u => u.Username, usernameOrEmail),
                Query<Model_Account>.EQ(u => u.ShaPassword, hashedPassword)
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

            accountCollection.Update(query, Update<Model_Account>.Replace(account));

            //update player entry
            Model_Player player = FetchPlayerByUsernameAndDiscriminator(account.Username + "#" + account.Discriminator);
            player.Token = token;
            player.Activity = PlayerActivity.Idle;
            player.LastLogin = DateTime.Now;

            query = Query.And(
                Query<Model_Player>.EQ(p => p.Username, player.Username),
                Query<Model_Player>.EQ(p => p.Discriminator, player.Discriminator)
            );

            playerCollection.Update(query, Update<Model_Player>.Replace(player));
        }
        else
        {
            Debug.Log("[Login Server]: Invalid login attempted.");
        }

        return account;

    }
    
    #endregion

    #region Fetch
    public Model_Account FetchAccountByToken(string token)
    {
        var query = Query<Model_Account>.EQ(u => u.Token, token);
        return accountCollection.FindOne(query);
    }
    public Model_Account FetchAccountByEmail(string email)
    {
        var query = Query<Model_Account>.EQ(u => u.Email, email);
        return accountCollection.FindOne(query);
    }
    public Model_Account FetchAccountByUsernameAndDiscriminator(string usernameAndDiscriminator)
    {
        string[] userDiscrim = usernameAndDiscriminator.Split('#');
        var query = Query.And(
                Query<Model_Account>.EQ(u => u.Username, userDiscrim[0]),
                Query<Model_Account>.EQ(u => u.Discriminator, userDiscrim[1])
            );
        return accountCollection.FindOne(query);
    }

    public Model_Player FetchPlayerByToken(string token)
    {
        var query = Query<Model_Player>.EQ(p => p.Token, token);
        return playerCollection.FindOne(query);
    }
    public Model_Player FetchPlayerByUsernameAndDiscriminator(string usernameAndDiscriminator)
    {
        string[] userDiscrim = usernameAndDiscriminator.Split('#');
        var query = Query.And(
                Query<Model_Player>.EQ(p => p.Username, userDiscrim[0]),
                Query<Model_Player>.EQ(p => p.Discriminator, userDiscrim[1])
            );
        return playerCollection.FindOne(query);
    }

    public Model_Ship FetchShipByName(string shipName)
    {
        var query = Query<Model_Ship>.EQ(s => s.shipName, shipName);
        return shipCollection.FindOne(query);
    }

    public Model_Worker FetchWorkerByName(string workerName)
    {
        var query = Query<Model_Worker>.EQ(w => w.workerName, workerName);
        return workerCollection.FindOne(query);
    }
    public Model_Worker FetchWorkerByID(ObjectId _id)
    {
        var query = Query<Model_Worker>.EQ(w => w._id, _id);
        return workerCollection.FindOne(query);
    }
    public List<Model_Worker> FetchWorkersByPlayer(Model_Player player)
    {
        List<Model_Worker> workers = new List<Model_Worker>();
        player.Workers.ForEach(delegate (MongoDBRef workerRef)
        {
            workers.Add(FetchWorkerByID(workerRef.Id.AsObjectId));
        });
        return workers;
    }

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
    /****************************************************
     * public void Update(){
     *     var query = query...
     *     var  document = collection.FindOne(query); 
     *     
     *     document.field = updatedValue;
     *     
     *     collection.Update(query, Update<Model>.Replace(document));     
     * }
     */

    /*
     public void updateTest()
    {
        var query = Query<Model_Account>.EQ(u => u.Username, "aaaa");
        var user = testCollection.FindOne(query);

        user.Discriminator = "0500";

        testCollection.Update(query, Update<Model_Account>.Replace(user));
    }*/
    #endregion

    #region Delete
    //this is just here to show remove syntax
    public void RemoveAccountByEmail(string email)
    {
        ObjectId id = FetchAccountByEmail(email)._id;
        accountCollection.Remove(Query<Model_Account>.EQ(u => u._id, id));
    }
    #endregion


}
