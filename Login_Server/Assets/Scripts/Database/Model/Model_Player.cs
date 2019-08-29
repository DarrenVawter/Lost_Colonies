using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;

public static class PlayerActivity
{
    public const byte Offline = 0;
    public const byte Idle = 1;
    public const byte Navigating = 2;
    public const byte CombatNavigating = 3;
    //TODO: other activities
}

public class Model_Player
{
    //id in db
    public ObjectId _id { set; get; }

    public string Token { get; set; }
    public string Username { get; set; }
    public string Discriminator { get; set; }

    public BsonDateTime CreatedOn { get; set; }
    public BsonDateTime LastLogin { get; set; }

    public byte Sector { get; set; }
    public byte Activity { get; set; }

    public List<MongoDBRef> Workers { get; set; } 
}
