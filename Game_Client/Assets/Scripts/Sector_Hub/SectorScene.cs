using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SectorScene : MonoBehaviour
{
    public static SectorScene Instance;

    [SerializeField] private Tilemap SectorHubMap;

    [SerializeField] private GameObject sh_defaultPrefab;
    [SerializeField] private GameObject sh_colonyPrefab;
    [SerializeField] private GameObject sh_shipPrefab;
    [SerializeField] private GameObject sh_asteroidPrefab;
    [SerializeField] private GameObject sh_lightRadius;
    //TODO: add other SH game object prefabs here

    //our active tracking location
    private string activeTrackingLocationName;
    //private string currentCameraCenter; //a var to allow for other objects to be clicked on and have the camera centered on them (but not set them as tracking center)

    internal struct TrackedObject
    {
        internal GameObject obj;
        internal Vector2 pos;
        internal Vector2 vel;
    }
    private struct StationaryObject
    {
        private GameObject obj;
        private Vector2 pos;
    }

    private Dictionary<string, TrackedObject> trackedObjects  = new Dictionary<string, TrackedObject>();
    private Dictionary<string, StationaryObject> stationaryObjects = new Dictionary<string, StationaryObject>();

    #region monobehaviour
    void Start()
    {
        Instance = this;

        //init scene components
        SectorHubMap.GetComponent<SH_Map>().init();
        Camera.main.GetComponent<SH_CameraHandler>().init();
    }

    private void Update()
    {
        //verify there is an active location
        if(activeTrackingLocationName == null)
        {
            if(Game_Player.Instance == null || !Game_Player.Instance.isInit)
            {
                //-> player has not been init yet
                return;
            }

            activeTrackingLocationName = Game_Player.Instance.activeWorker.locationName;

            if (activeTrackingLocationName == null)
            {
                //if still null -> ?
                //TODO idk
                return;
            }
        }

        //update position of each tracked object
        TrackedObject objUpdate;
        foreach (KeyValuePair<string, TrackedObject> tracked in trackedObjects)
        {
            //set reference
            objUpdate = trackedObjects[tracked.Key];

            //update objects' position vars       
            objUpdate.pos = new Vector2(objUpdate.pos.x + objUpdate.vel.x * Time.deltaTime, objUpdate.pos.y + objUpdate.vel.y * Time.deltaTime);

            //if tracked object is more than (4?) tiles away, remove it from tracking (and destroy object)
            if (new Vector2(trackedObjects[activeTrackingLocationName].pos.x - objUpdate.pos.x, trackedObjects[activeTrackingLocationName].pos.y - objUpdate.pos.y).magnitude > 3)//todo figure out tracking range?
            {
                Destroy(objUpdate.obj);
                trackedObjects.Remove(tracked.Key);
                continue;
            }

            //update objects' transforms
            objUpdate.obj.transform.position = new Vector3(objUpdate.pos.x, objUpdate.pos.y, objUpdate.obj.transform.position.z);

            //move camera to current obj if it is active obj
            if (tracked.Key == activeTrackingLocationName)
            {
                Camera.main.GetComponent<SH_CameraHandler>().reCenter(new Vector2(tracked.Value.pos.x,tracked.Value.pos.y));
            }

        }
    }
    #endregion

    #region tracked object handling
    //init tracked object
    internal void initTrackedObject(Net_OnTrackedObjectInitRequest msg)
    {
        TrackedObject newObj;
        switch (msg.locationData[0])
        {
            case LOCATION_TYPE.SHIP:
                newObj = trackedObjects[msg.locationName];
                Destroy(newObj.obj);
                newObj.obj = Instantiate(sh_shipPrefab);
                trackedObjects[msg.locationName] = newObj;
                break;

            case LOCATION_TYPE.COLONY:
            case LOCATION_TYPE.ASTEROID:
                Debug.Log("Not implemented");
                break;

            case LOCATION_TYPE.NONE:
            default:
                Debug.Log(string.Format("[Sector Scene]: Unexpected SH_Location_Type ({0}).", msg.locationData[0]));
                break;
                
        }

        if(msg.locationName == activeTrackingLocationName)
        {
            Instantiate(sh_lightRadius, trackedObjects[msg.locationName].obj.transform);
        }
    }

    //update or add object to tracking list
    internal void updateTrackedMobileObject(Net_OnTrackedObjectChange msg)
    {
        TrackedObject objUpdate;

        //check if obj already exists or not
        if (!trackedObjects.ContainsKey(msg.locationName))
        {
            //->if not: use default object, add to list, & request init
            objUpdate = new TrackedObject();
            objUpdate.obj = Instantiate(sh_defaultPrefab);
            trackedObjects.Add(msg.locationName, objUpdate);
            Network_Client.Instance.SendTrackedObjectInitRequest(msg.locationName);
        }

        objUpdate = trackedObjects[msg.locationName];
        objUpdate.pos = new Vector2(msg.posX, msg.posY);
        objUpdate.vel = new Vector2(msg.velX, msg.velY);
        trackedObjects[msg.locationName] = objUpdate;

    }
    
    //reset tracking position
    internal void changeActiveTrackingLocation()
    {
        //remove previously tracked objects if they exist
        if(trackedObjects != null || trackedObjects.Count != 0)
        {
            trackedObjects.Clear();
        }
        if (stationaryObjects != null || stationaryObjects.Count != 0)
        {
            stationaryObjects.Clear();
        }

        //set this location as active location
        activeTrackingLocationName = Game_Player.Instance.activeWorker.locationName;
    }
    #endregion

    #region ui
    internal void displayInfo(GameObject shObj)
    {
        //TODO add
    }
    #endregion

}
