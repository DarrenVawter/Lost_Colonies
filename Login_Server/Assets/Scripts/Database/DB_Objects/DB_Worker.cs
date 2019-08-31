using MongoDB.Bson;
using MongoDB.Driver;

public class DB_Worker
{    
    //id in db
    public ObjectId _id;

    //owner of this worker
    public MongoDBRef owner;

    //location info
    public MongoDBRef location;

    //worker's name
    public string workerName;

    //activity info
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
