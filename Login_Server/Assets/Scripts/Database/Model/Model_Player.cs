using MongoDB.Driver;

public class Model_Player
{
    public int ActiveConnectionID { set; get; }
    public string Token { get; set; }
    public string Username { get; set; }
    public byte Activity { get; set; }

    public short nWorkers;
    public MongoDBRef[] Workers { get; set; } 
}
