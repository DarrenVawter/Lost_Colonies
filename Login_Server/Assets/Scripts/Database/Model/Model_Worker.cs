using MongoDB.Bson;
using MongoDB.Driver;

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
    public byte sector;

    //worker's name
    public string workerName;

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
