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
    private List<Game_Worker> workers = new List<Game_Worker>();
    public Game_Worker activeWorker { get; set; }
        //worker gameobj prefab
    [SerializeField] private GameObject workerGameObjPrefab;

    private void Start()
    {
        Instance = this;
    }

    internal void init(Message_Player player)
    {
        //ensure the worker list is empty
        workers.RemoveRange(0,workers.Count);

        //init player values
        username = player.username;
        discriminator = player.discriminator;

        //init player's workers from msg
        GameObject workerGameObj;
        Game_Worker worker;
        foreach (Message_Worker gwData in player.workers)
        {
            //create the game object which will contain the worker object
            workerGameObj = Instantiate(workerGameObjPrefab, transform);

            //get a reference to the worker object of the game object
            worker = workerGameObj.GetComponent<Game_Worker>();

            //init the worker object with the msg data
            worker.init(gwData);

            //add the worker object reference to the player's list
            workers.Add(workerGameObj.GetComponent<Game_Worker>());

        }

        //**
        //TODO: set active worker dynamically
        //**
        workers[0].SetActive();

    }

}
