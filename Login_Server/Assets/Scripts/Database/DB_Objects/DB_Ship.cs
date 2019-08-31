using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;

public class DB_Ship
{
    //id in db
    public ObjectId _id;

    //unique name of this ship
    public string shipName { get; set; }

    //owner of this ship
    public MongoDBRef Owner { get; set; }

    //what is the ship currently doing
    public byte activity { get; set; }

    //server #
    public byte sector { get; set; }

    //position info
    public float velX { get; set; }
    public float velY { get; set; }
    public float posX { get; set; }
    public float posY { get; set; }

    //who is on board this ship?
    public List<MongoDBRef> workersOnBoard { get; set; }    
    
    //appearance
        //exterior
    public byte frame { get; set; }
    public byte color1 { get; set; }
    public byte color2 { get; set; }
    public byte color3 { get; set; }
        //interior
    //**
    //TODO populate
    //**

    //stats
    //**
    //TODO populate
    //**

    //what colony & nation does this ship belong to?
    //**
    //TODO populate
    //**
}
