[System.Serializable]
public class Message_This_Worker
{
    //location info
    public string locationName;

    //worker's name
    public string workerName;

    //activity info
    public byte activity;

    //stats
    //**
    //TODO populate
    //**

    //appearance
    //**
    //TODO populate
    //**

    public Message_This_Worker(string locationName, string workerName, byte activity)
    {
        this.locationName = locationName;
        this.workerName = workerName;
        this.activity = activity;
    }

}
