public static class WorkerActivity
{
    public const byte Idle = 0;
    public const byte Captain = 1;
    public const byte Tactician = 2;
}

[System.Serializable]
public class Game_Worker
{
    //owner of worker
    private string owner;
    
    //location of worker
    private string location;
//    private Game_Location activeLocationData;
    private byte sector;

    //worker's name
    private string workerName;

    //activity info
    private bool isInCombat;
    private byte activity;

    //stats
    //**
    //TODO populate
    //**

    //appearance
    //**
    //TODO populate
    //**

    //is the worker the active one?
    private bool isActive = false;

    //is the worker initialized?
    private bool isInit { get; set; }
    
    internal Game_Worker(string owner, string location, byte sector, string workerName, bool isInCombat, byte activity)
    {
        this.owner = owner;
        this.location = location;
        this.sector = sector;
        this.workerName = workerName;
        this.isInCombat = isInCombat;
        this.activity = activity;

        isActive = false;
        isInit = false;
    }

    internal void init(Game_Worker workerData)
    {
        //reset init mark
        isInit = false;

        owner = workerData.owner;
        location = workerData.location;
        workerName = workerData.workerName;
        isInCombat = workerData.isInCombat;
        activity = workerData.activity;
        isActive = false;

        //set init marker
        isInit = true;
    }

    internal void SetActive()
    {
        /*

        //verify initialization
        if (!isInit)
        {
            init();
        }          

        //get location (in case it has changed)
        Client.Instance.SendLocationDataRequest(location);
        activeLocationData = Client.Instance.GetLocationData(location);
        while (activeLocationData == null)
        {
            activeLocationData = Client.Instance.GetLocationData(location);
        }

        //update worker's rendered location
        transform.position = new Vector3(activeLocationData.gridX, activeLocationData.gridY);

        //move camera to this worker and change scene as appropriate
        if (activeLocationData.isShip)
        {
            if (SceneManager.GetActiveScene().name != "Sector_Hub")
            {
                //change to sector hub
                //TODO: or change to ship scene?
            }
            Camera.main.GetComponent<SH_CameraHandler>().recenter(gameObject);
        }
        else
        {
            if (SceneManager.GetActiveScene().name != "Colony")
            {
                //change to Colony scene
            }
        }

        //TODO:
        //deactive previous active worker if !null

        //set activity to idle since any current activity is halted when switching to this worker
        //activity = WorkerActivity.Idle;
        //TODO: remove this temporary line, re-enable the line above
        activity = WorkerActivity.Captain;

        //set player's active worker var
        Game_Player.Instance.activeWorker = this;

        //set as active
        isActive = true;

        */
    }
    
}
