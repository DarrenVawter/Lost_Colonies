[System.Serializable]
public class Net_OnWorkerDataRequest : NetSectorMessage
{
    public string owner { get; }
    public string name { get; }
    public string location { get; }
    public byte sector { get; }
    public bool isInCombat { get; }
    public byte activity { get; }

    public Net_OnWorkerDataRequest(string owner, string name, string location, byte sector, bool isInCombat, byte activity)
    {
        OP = NetSectorOP.OnWorkerDataRequest;

        this.owner = owner;
        this.name = name;
        this.location = location;
        this.sector = sector;
        this.isInCombat = isInCombat;
        this.activity = activity;
    }
}