using System.Collections.Generic;

[System.Serializable]
public class Message_Player
{
    //player's username
    internal string username { get; set; }
    internal string discriminator { get; set; }

    //player's workers
    internal List<Message_Worker> workers { get; set; }

    internal Message_Player(string username, string discriminator, List<Message_Worker> workers)
    {
        this.username = username;
        this.discriminator = discriminator;
        this.workers = workers;
    }
}
