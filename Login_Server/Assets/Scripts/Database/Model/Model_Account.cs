using MongoDB.Bson;
using MongoDB.Driver;

public static class Activity
{
    public const byte Offline = 0;
    public const byte Idle = 1;
    public const byte Navigating = 2;
    public const byte CombatNavigating = 3;
    //TODO: other activities
}

public class Model_Account
{
    //id in db
    public ObjectId _id;

    public int ActiveConnectionID { set; get; }
    public string Username { get; set; }
    public string Discriminator { get; set; }
    public string Email { get; set; }
    public string ShaPassword { get; set; }
    public string Token { get; set; }

    public byte Sector { get; set; }
    public byte Status { get; set; }

    public BsonDateTime CreatedOn { get; set; }
    public BsonDateTime LastLogin { get; set; }

    public MongoDBRef[] Workers { get; set; }

}
