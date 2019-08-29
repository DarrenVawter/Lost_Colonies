public static class WorkerActivity
{
    public const byte Idle = 0;
    public const byte Captain = 1;
    public const byte Tactician = 2;
}

[System.Serializable]
public class Message_Worker
{
    //owner of worker
    internal string owner { get; private set; }

    //location of worker
    internal string locationName { get; private set; }
    //    private Game_Location activeLocationData;
    internal byte sector { get; private set; }

    //worker's name
    internal string workerName { get; private set; }

    //activity info
    internal bool isInCombat { get; private set; }
    internal byte activity { get; private set; }

    //stats
    //**
    //TODO populate
    //**

    //appearance
    //**
    //TODO populate
    //**
    internal Message_Worker()
    {
        owner = null;
        locationName = null;
        sector = SectorCode.None;
        workerName = null;
        isInCombat = false;
        activity = WorkerActivity.Idle;
    }

    internal Message_Worker(string owner, string locationName, byte sector, string workerName, bool isInCombat, byte activity)
    {
        this.owner = owner;
        this.locationName = locationName;
        this.sector = sector;
        this.workerName = workerName;
        this.isInCombat = isInCombat;
        this.activity = activity;
    }

}
