using MongoDB.Bson;
using MongoDB.Driver;

public static class WorkerActivity
{
    public const byte Idle = 0;
    public const byte Captain = 1;
    public const byte Tactician = 2;
}

public class Model_Worker
{
    //id in db
    public ObjectId _id;

    //owner of this worker
    public MongoDBRef owner;
    public string ownerName;

    //location info
    public MongoDBRef location;
    public string locationName;

    //worker's name
    public string name;

    //activity info
    public bool isInCombat;
    public byte activity;

    //stats
    //**
    //TODO populate
    //**

    //appearance
    //**
    //TODO populate
    //**
}
