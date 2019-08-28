[System.Serializable]
public class Net_OnWorkerDataRequest : NetSectorMessage
{
    public string owner { get; set; }
    public string name { get; set; }
    public string location { get; set; }
    public bool isInCombat { get; set; }
    public byte activity { get; set; }

    public Net_OnWorkerDataRequest(string owner, string name, string location, bool isInCombat, byte activity)
    {
        OP = NetSectorOP.OnWorkerDataRequest;

        this.owner = owner;
        this.name = name;
        this.location = location;
        this.isInCombat = isInCombat;
        this.activity = activity;
    }
}