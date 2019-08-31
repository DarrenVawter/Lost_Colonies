using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;

public class DB_Player
{
    //id in db
    public ObjectId _id { set; get; }
    
    public int ActiveConnectionID { set; get; }
    public int ActiveHostID { set; get; }

    public string Token { get; set; }
    public string Username { get; set; }
    public string Discriminator { get; set; }

    public BsonDateTime CreatedOn { get; set; }
    public BsonDateTime LastLogin { get; set; }

    public List<MongoDBRef> Workers { get; set; }
    public byte ActiveWorkerIndex { get; set; }
}
