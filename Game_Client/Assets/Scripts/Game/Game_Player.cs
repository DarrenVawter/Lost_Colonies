using System.Collections.Generic;
using UnityEngine;

public class Game_Player : MonoBehaviour
{
    //static ref to player obj
    public static Game_Player Instance = null;
    public bool isInit = false;

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

    internal void init(Net_OnInitPlayerDataRequest playerData)
    {
        //ensure the worker list is empty
        workers.RemoveRange(0,workers.Count);
        
        //init player's workers from msg
        GameObject workerGameObj;
        Game_Worker worker;
        foreach (Message_This_Worker gwData in playerData.Workers)
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
        
        //set active worker dynamically
        activeWorker = workers[playerData.ActiveWorkerIndex];

        //set init flag
        isInit = true;

        //TODO REMOVE THIS
        SETSAILTESTTTTTT();
    }

    internal void setActive(Net_OnActiveWorkerChange msg)
    {
        activeWorker = workers[msg.workerIndex];
    }

    private void SETSAILTESTTTTTT()
    {
        Network_Client.Instance.SendSetSailRequest(activeWorker.locationName);
    }
}
