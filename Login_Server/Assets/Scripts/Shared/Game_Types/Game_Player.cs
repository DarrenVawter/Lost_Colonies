using System.Collections.Generic;
using UnityEngine;

public class Game_Player : MonoBehaviour
{
    //static ref to player obj
    public static Game_Player Instance;
    
    //player's username
    public string username { get; set; }
    public string discriminator { get; set; }

    //player's workers
    private List<Game_Worker> workers;
    private Game_Worker activeWorker;
        //worker gameobj prefab
    [SerializeField] private GameObject workerObj;

    private void Start()
    {
        Instance = this;
    }

    internal void init(Net_OnThisPlayerDataRequest msg)
    {
        username = msg.username;
        discriminator = msg.discriminator;
        workers = msg.workers;
        /*
        IFormatter formatter = new BinaryFormatter();
        msg.workers.Seek(0, SeekOrigin.Begin);
        workers = (List<Game_Worker>)formatter.Deserialize(msg.workers);
        */

        //create a game object for each worker
        GameObject worker;
        workers.ForEach(delegate (Game_Worker workerData)
        {
            worker = Instantiate(workerObj, transform);
            worker.GetComponent<Game_Worker>().init(workerData);
        });

        //**
        //TODO: set active worker dynamically
        //**
        activeWorker.SetActive();

    }

}
