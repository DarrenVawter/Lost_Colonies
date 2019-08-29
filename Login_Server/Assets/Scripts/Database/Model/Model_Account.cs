using MongoDB.Bson;
using MongoDB.Driver;

public static class LoginStatus
{
    public const byte Offline = 0;
    public const byte StandAlone = 1;
    public const byte WebGL = 2;
    //TODO: other statuses
}

public class Model_Account
{
    //id in db
    public ObjectId _id { set; get; }

    public int ActiveConnectionID { set; get; }
    public string Token { get; set; }
    public string Username { get; set; }
    public string Discriminator { get; set; }
    public string Email { get; set; }
    public string ShaPassword { get; set; }

    public BsonDateTime CreatedOn { get; set; }
    public BsonDateTime LastLogin { get; set; }

    public byte Status { get; set; }
    public byte Sector { get; set; }

}
