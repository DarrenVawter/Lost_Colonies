using System;
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
    private const string DB_IP = "104.199.121.240";
    private const string DB_PORT = "27017";

    private const string MONGO_URI = "mongodb://" + USERNAME + ":" + PASSWORD + "@" + DB_IP + ":" + DB_PORT + "/" + DB_NAME;

    private MongoClient client;
    private MongoServer server;
    private MongoDB.Driver.MongoDatabase mdb;

    private MongoCollection<Model_Account> accountCollection;
    private MongoCollection<Model_Worker> workerCollection;
    private MongoCollection<Model_Ship> shipCollection;

    //game variables
    private byte MAX_WORKERS = 7;

    public void Init()
    {
        client = new MongoClient(MONGO_URI);
        server = client.GetServer();
        mdb = server.GetDatabase(DB_NAME);

        //init collections(tables) here
        accountCollection = mdb.GetCollection<Model_Account>("accounts");
        workerCollection = mdb.GetCollection<Model_Worker>("workers");
        shipCollection = mdb.GetCollection<Model_Ship>("ships");

        Debug.Log("[Login Server]: Initialized " + DB_NAME);

        //TODO REMOVE!
        /*
        for (int i = 0; i < 20; i++)
        {
            Model_Ship ship = new Model_Ship();
            ship.name = "ship" + i;
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

    #region Insert
    public bool CreateAccount(string username, string hashedPassword, string email)
    {

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
        while(FetchAccountByUsernameAndDiscriminator(username, discriminatorInt.ToString().PadLeft(4,'0')) != null)
        {
            discriminatorInt++;
        }
        account.Discriminator = discriminatorInt.ToString().PadLeft(4, '0');

        //TODO: assign sector dynamically
        account.Sector = 1;

        //assign default worker for new account
        //TODO: allow this worker to be customized
        //TODO: set this worker's owner
        account.Workers = new MongoDBRef[MAX_WORKERS];
        account.Workers[0] = SpawnDefaultWorker();
        for(int i = 1; i < MAX_WORKERS; i++)
        {
            account.Workers[i] = null;
        }
        
        account.CreatedOn = DateTime.Now;
        account.LastLogin = DateTime.Now;

        //insert account with validated data
        accountCollection.Insert(account);
        return true;
    }
    public MongoDBRef SpawnDefaultWorker()
    {
        Model_Worker defaultWorker = new Model_Worker();
        defaultWorker.isInCombat = false;
        defaultWorker.activity = WorkerActivity.Idle;
        //TODO: change default spawn location
        defaultWorker.location = GETNEXTAVAILABLESHIPDBREFERENCETESTTTT();
        defaultWorker.locationName = "ship0";//TODO change me
        //TODO: generate name for worker
        defaultWorker.name = "DEFAULTTESTNAME";

        //stats
        //**
        //TODO populate
        //**

        //appearance
        //**
        //TODO populate
        //**

        workerCollection.Insert(defaultWorker);
        return null;// FetchByID(workerCollection, defaultWorker._id);
    }
    private MongoDBRef GETNEXTAVAILABLESHIPDBREFERENCETESTTTT()
    {
        Model_Ship ship;
        string name = "ship0";
        var query = Query<Model_Ship>.EQ(s => s.name, name);

        for (int i = 1; i < 50; i++)
        {
            ship = shipCollection.FindOne(query);

            if(ship != null)
            {
                return new MongoDBRef(shipCollection.Name, ship._id);
            }

            name = "ship" + i;
            query = Query<Model_Ship>.EQ(s => s.name, name);
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
        else if (Utility.IsUsername(usernameOrEmail))
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
            account.ActiveConnectionID = connectionID;
            account.Token = token;
            account.Status = 1;
            account.LastLogin = System.DateTime.Now;

            accountCollection.Update(query, Update<Model_Account>.Replace(account));

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
    public Model_Account FetchAccountByUsernameAndDiscriminator(string username, string discriminator)
    {
        var query = Query.And(
                Query<Model_Account>.EQ(u => u.Username, username),
                Query<Model_Account>.EQ(u => u.Discriminator, discriminator)
            );
        return accountCollection.FindOne(query);
    }
    /*      FETCH ALL******************************
     * public List<Model> FetchAll(){
     *      List<Model> listName = new List<Model>();
     *      foreach(var m in collection.Find(query)){
     *          listName.add(m); //LLAPI (8/9 @ 54:00)
     *      }
     *      return listName;
     * }
     */
    public Model_Ship FetchShipByName(string name)
    {
        var query = Query<Model_Ship>.EQ(s => s.name, name);
        return shipCollection.FindOne(query);
    }
    public Model_Worker FetchWorkerByName(string name)
    {
        var query = Query<Model_Worker>.EQ(w => w.name, name);
        return workerCollection.FindOne(query);
    }
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
