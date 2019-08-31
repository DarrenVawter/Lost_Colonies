using UnityEngine;

public class Game_Worker : MonoBehaviour
{    
    //owner of worker
    internal string owner { get; private set; }

    //worker's name
    internal string workerName { get; private set; }

    //location of worker
    internal string locationName { get; private set; }

    //activity info
    internal byte activity { get; private set; }

    //stats
    //**
    //TODO populate
    //**

    //appearance
    //**
    //TODO populate
    //**
    
    //is the worker initialized?
    internal bool isInit { get; private set; }

    internal Game_Worker()
    {
        owner = null;
        locationName = null;
        workerName = null;
        activity = WORKER_ACTIVITY.IDLE;
        isInit = false;
    }

    internal void init(Message_This_Worker workerData)
    {
        //reset init marker
        isInit = false;

        workerName = workerData.workerName;
        locationName = workerData.locationName;
        activity = workerData.activity;
       
        //set init marker
        isInit = true;
    }
    
}
