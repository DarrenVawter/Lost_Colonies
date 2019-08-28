using UnityEngine;

public class SH_Player : MonoBehaviour
{
    [SerializeField] private GameObject workerObj;

    private GameObject[] Workers;
    private GameObject ActiveWorker;

    public static SH_Player Instance;

    private void Start()
    {
        Instance = this;
    }
    
    internal void init()
    {
        //**
        //TODO: get player's workers from server
        //**
        //currently -> getting a default worker obj
        Workers = new GameObject[7];
        Workers[0] = Instantiate(workerObj, transform);
        ActiveWorker = Workers[0];
        ActiveWorker.name = "Worker0";

        //**
        //TODO: set active worker dynamically
        //**
        //currently -> initing default worker with 1 default ship
        ActiveWorker.GetComponent<Worker>().SetActive();

    }

}
