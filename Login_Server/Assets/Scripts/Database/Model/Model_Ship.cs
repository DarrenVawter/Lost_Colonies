using MongoDB.Bson;
using MongoDB.Driver;

public class Model_Ship
{
    //id in db
    public ObjectId _id;

    //owner of this worker
    public MongoDBRef Owner { get; set; }
    
    //unique name of this ship
    public string shipName;

    //is occupied 
    //(mining asteroid, in combat)
    public bool isBusy;
    //in port
    public bool isDocked;

    //server #
    public short sector;

    //location info
    public float velX;
    public float velY;
    public float posX;
    public float posY;


    //stats
    //**
    //TODO populate
    //**

    //appearance
    //**
    //TODO populate
    //**
}
