using UnityEngine;
using UnityEngine.SceneManagement;

public static class WorkerActivity
{
    public const byte Idle = 0;
    public const byte Captain = 1;
    public const byte Tactician = 2;
}

public class Worker : MonoBehaviour
{
    //owner of worker
    private string owner;
    
    //location of worker
    private string location;
    private Location activeLocationData;

    //worker's name
    private string name;

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
    public bool isInit { get; set; }
    
    public Worker(string owner, string location, string name, bool isInCombat, byte activity)
    {
        this.owner = owner;
        this.location = location;
        this.name = name;
        this.isInCombat = isInCombat;
        this.activity = activity;

        isActive = false;
        isInit = false;
    }

    private void init()
    {
        //reset init marker
        isInit = false;

        //get worker data (in case it has changed)
        Client.Instance.SendWorkerDataRequest(name);
        Worker workerData = Client.Instance.GetWorkerData(name);
        while (workerData == null)
        {
            workerData = Client.Instance.GetWorkerData(name);
        }

        owner = workerData.owner;
        location = workerData.location;
        name = workerData.name;
        isInCombat = workerData.isInCombat;
        activity = workerData.activity;

        //reset init marker
        isInit = true;
    }

    internal void SetActive()
    {
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

        //set as active
        isActive = true;
    }

}
