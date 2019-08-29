using UnityEngine;
using UnityEngine.SceneManagement;

public class Game_Worker : MonoBehaviour
{
    //not included in message_worker
    internal Message_Location activeLocationData;
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    
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

    //is the worker the active one?
    internal bool isActive { get; private set; }

    //is the worker initialized?
    internal bool isInit { get; private set; }

    internal Game_Worker()
    {
        owner = null;
        locationName = null;
        sector = SectorCode.None;
        workerName = null;
        isInCombat = false;
        activity = WorkerActivity.Idle;
        isActive = false;
        isInit = false;
    }

    internal void init(Message_Worker workerData)
    {
        //reset init marker
        isInit = false;

        owner = workerData.owner;
        locationName = workerData.locationName;
        workerName = workerData.workerName;
        isInCombat = workerData.isInCombat;
        activity = workerData.activity;
        isActive = false;

        //set init marker
        isInit = true;
    }

    internal void SetActive()
    {
        //verify initialization
        if (!isInit)
        {
            Debug.Log(string.Format("Worker is not initialized ({0}).",workerName));
            return;
        }
        
        //get location (in case it has changed)
        Network_Client.Instance.SendLocationDataRequest(locationName);
        activeLocationData = Network_Client.Instance.GetLocationData(locationName);

        breaking the code right here on purpose;
        //
        //**
        //TODO: this probably shouldn't be in a while loop
        //**
        while (activeLocationData == null)
        {
            activeLocationData = Network_Client.Instance.GetLocationData(locationName);
        }
        */

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
        activity = WorkerActivity.Idle;
        //TODO: actually make worker go idle (stop current activity)

        //set player's active worker var
        Game_Player.Instance.activeWorker = this;

        //set as active
        isActive = true;
        
    }
}
