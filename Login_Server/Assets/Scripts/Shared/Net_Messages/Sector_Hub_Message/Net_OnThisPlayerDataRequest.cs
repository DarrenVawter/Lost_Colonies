using System.Collections.Generic;

[System.Serializable]
public class Net_OnThisPlayerDataRequest : NetSectorMessage
{
    public string username { get; }
    public string discriminator { get; }
    public List<Game_Worker> workers { get; }

    public Net_OnThisPlayerDataRequest(string username, string discriminator, List<Game_Worker> workers)
    {
        OP = NetSectorOP.OnThisPlayerDataRequest;

        this.username = username;
        this.discriminator = discriminator;
        this.workers = workers;
    }
}
