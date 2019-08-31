using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Networking;

/* TODO notes:
 *
 * Consider encrypting login token?
 *  Verify ship data request (is member of crew? is on board? is nearby? ship is available/not in combat?)
 * 
 */
 
public class SectorServer : MonoBehaviour
{

    #region Network Fields
    //sector server #
    public int sector;

    //consts
    private const int MAX_USER = 100;
    private const int PORT = 27989;//TODO: change back once servers are on different machines
    private const int WEB_PORT = 27990;//TODO: change this back, too
    private const int BYTE_SIZE = 1024;

    //has server started?
    private bool isStarted;

    //error flag
    private byte error;

    //communication channel between host and client(s)
    private byte reliableChannel;
    
    //host ids
    private int hostID;     //stand-alone client
    private int webHostID;  //web-gl

    //db reference
    private MongoDatabase mdb;
    #endregion

    #region GameSpace Dields

    internal struct TrackedObject
    {
        internal Vector2 pos;
        internal Vector2 vel;
        internal byte type;
    }

    internal struct StationaryObject
    {
        internal Vector2 pos;
        internal byte type;
    }

    QuadTree qt;
    Dictionary<string, List<string>> trackedByLocation; //list of objects each location is tracking
    internal Dictionary<string, TrackedObject> trackedObjects;  //list of objects the server is tracking
    internal Dictionary<string, StationaryObject> stationaryObjects;    //list of stationary objects the server is holding
       
    #endregion

    #region Monobehavior
    private void Start()
    {
        isStarted = false;
        DontDestroyOnLoad(gameObject);
        InitServer();
        InitGameSpace();
    }

    private void Update()
    {
        UpdateMessagePump();
        UpdateGameSpace();
    }
    #endregion

    #region NetworkBehavior
    public void InitServer()
    {

        mdb = new MongoDatabase();
        mdb.Init();

        NetworkTransport.Init();

        ConnectionConfig cc = new ConnectionConfig();
        //reliable -> spending gold to buy item (100% delivery but can clog)
        //unreliable -> update position (not 100% delivery but won't clog)
        //research the other QoSTypes
        reliableChannel = cc.AddChannel(QosType.Reliable);
        
        HostTopology topo = new HostTopology(cc, MAX_USER);

        //*********************server only code*********************************

        hostID = NetworkTransport.AddHost(topo, PORT, null);                
        webHostID = NetworkTransport.AddWebsocketHost(topo, WEB_PORT, null);

        isStarted = true;

        Debug.Log(string.Format("[Sector Server]: Connection open on port {0} and webport {1}.",PORT,WEB_PORT));
    }

    public void Shutdown()
    {
        NetworkTransport.Shutdown();
        isStarted = false;
    }

    public void UpdateMessagePump()
    {
        if (!isStarted)
        {
            return;
        }

        int recHostID;      //client platform
        int connectionID;   //client id
        int channelID;      //communication channel id

        byte[] recBuffer = new byte[BYTE_SIZE];
        int dataSize;

        NetworkEventType type = NetworkTransport.Receive(out recHostID, out connectionID, out channelID, recBuffer, BYTE_SIZE, out dataSize, out error);

        switch (type)
        {
            case NetworkEventType.Nothing:
                break;

            case NetworkEventType.DataEvent:
                BinaryFormatter formatter = new BinaryFormatter();
                MemoryStream ms = new MemoryStream(recBuffer);
                NetMsg msg = (NetMsg)formatter.Deserialize(ms);
                Debug.Log(string.Format("[Sector Server]: Recieved Data (RT-{0}, OP-{1}).", msg.RT, msg.OP));
                OnData(connectionID, channelID, recHostID, msg);
                break;
            
            case NetworkEventType.ConnectEvent:
                Debug.Log(string.Format("[Sector Server]: User {0} connected through host {1}.", connectionID, recHostID));
                break;

            case NetworkEventType.DisconnectEvent:
                Debug.Log(string.Format("[Sector Server]: User {0} disconnected.", connectionID));
                break;

            case NetworkEventType.BroadcastEvent:
            default:
                Debug.Log("[Sector Server]: Unexpected Network Event Type.");
                break;
        }
    }
    #endregion

    #region OnData
    private void OnData(int connectionID, int channelID, int recHostID, NetMsg msg)
    {
        switch (msg.RT)
        {
            case NetRT.SectorServer:
                OnSectorData(connectionID, channelID, recHostID, msg);
                break;

            case NetRT.ShipCombatServer:
                break;
        
            case NetRT.LoginServer:
            default:
                Debug.Log("[Sector Server]: Unexpected Net RT (" + msg.RT + ").");
                break;
        }
    }

    private void OnSectorData(int connectionID, int channelID, int recHostID, NetMsg msg)
    {
        switch (msg.OP)
        {
            case NetSectorOP.InitPlayerDataRequest:
                InitPlayerDataRequest(connectionID, recHostID, (Net_InitPlayerDataRequest)msg);
                break;

            case NetSectorOP.ActiveWorkerChangeRequest:
                ActiveWorkerChangeRequest(connectionID, recHostID, (Net_ActiveWorkerChangeRequest)msg);
                break;

            case NetSectorOP.TrackedObjectInitRequest:
                TrackedObjectInitRequest(connectionID, recHostID, (Net_TrackedObjectInitRequest)msg);
                break;

            case NetSectorOP.SetSailRequest:
                SetSailRequest((Net_SetSailRequest)msg);
                break;
                               
            default:
                Debug.Log("[Sector Server]: Unexpected Net OP (" + msg.OP + ").");
                break;
        }
    }

    //initialize player data (this happens after login)
    private void InitPlayerDataRequest(int connectionID, int recHostID, Net_InitPlayerDataRequest msg)
    {
        //fetch player from DB by token
        DB_Player player = mdb.FetchPlayerByToken(msg.token);

        if(player == null)
        {
            Debug.Log(string.Format("[Sector Server]: invalid token from connection id {0} on host {1}.", connectionID, recHostID));
            //TODO: force update this connection's token (be careful how you do this)
            return;
        }

        //fetch model_workers by player and convert to message_this_workers
        List<Message_This_Worker> messageThisWorkers = new List<Message_This_Worker>();
        List<DB_Worker> modelWorkers = mdb.FetchWorkersByPlayer(player);
        foreach (DB_Worker worker in modelWorkers)
        {
            switch (worker.location.CollectionName) {
                case COLLECTIONS.SHIPS:
                    messageThisWorkers.Add(new Message_This_Worker(mdb.FetchShipByID(worker.location.Id.AsObjectId).shipName, worker.workerName, worker.activity));
                    break;
                default:
                    Debug.Log(string.Format("[Sector Server]: Unexpected worker location collection fetch ({0}).",worker.location.CollectionName));
                    return;
            }
        }

        //send messsage_player
        SendClient(connectionID, recHostID, new Net_OnInitPlayerDataRequest(player.CreatedOn.ToString(),messageThisWorkers,player.ActiveWorkerIndex));

        //update player's connection id
        player.ActiveConnectionID = connectionID;
        player.ActiveHostID = recHostID;
        mdb.updatePlayer(player);

        //update account's connection id
        DB_Account account = mdb.FetchAccountByToken(msg.token);
        account.ActiveConnectionID = connectionID;
        account.ActiveHostID = recHostID;
        mdb.updateAccount(account);

    }

    //change the player's active worker index
    private void ActiveWorkerChangeRequest(int connectionID, int recHostID, Net_ActiveWorkerChangeRequest msg)
    {
        //fetch player from DB by token
        DB_Player player = mdb.FetchPlayerByToken(msg.token);

        if (player == null)
        {
            Debug.Log(string.Format("[Sector Server]: invalid token from connection id {0} on host {1}.", connectionID, recHostID));
            //TODO: force update this connection's token (be careful how you do this)
            return;
        }

        //TODO: check that the player is actually allowed to switch workers

        //if player is allowed to switch
        player.ActiveWorkerIndex = msg.workerIndex;
        mdb.updatePlayer(player);

        //send response
        SendClient(connectionID, recHostID, new Net_OnActiveWorkerChange(player.ActiveWorkerIndex));
    }
         
    //send the image and type data of a mobile object
    private void TrackedObjectInitRequest(int connectionID, int recHostID, Net_TrackedObjectInitRequest msg)
    {
        //get type
            //ship
        DB_Ship ship = mdb.FetchShipByName(msg.locationName);
        if (ship != null)
        {
            //generate response
            Net_OnTrackedObjectInitRequest responseMsg = new Net_OnTrackedObjectInitRequest();
            responseMsg.locationName = msg.locationName;
            responseMsg.locationData = new byte[] {LOCATION_TYPE.SHIP,ship.frame,ship.color1,ship.color2,ship.color3};
            
            //send response
            SendClient(connectionID, recHostID, responseMsg);
            return;
        }
        
            //asteroid
        //TODO

            //other
        //TODO
    }
    
    //change a ship's status to traveling
    private void SetSailRequest(Net_SetSailRequest msg)
    {
        //get ship and requesting player refs
        DB_Player player = mdb.FetchPlayerByToken(msg.token);
        DB_Ship ship = mdb.FetchShipByName(msg.shipName);

        //verify the requestor is the owner
        if(ship.Owner.Id.AsObjectId.Equals(player._id))
        {
            //change ship activity
            ship.activity = SHIP_ACTIVITY.TRAVELING;
            mdb.updateShip(ship);

            //add ship to tracked objects
            TrackedObject newShip = new TrackedObject();
            newShip.pos = new Vector2(ship.posX,ship.posY);
            newShip.vel = new Vector2(ship.velX, ship.velY);
            newShip.type = LOCATION_TYPE.SHIP;
            trackedObjects.Add(ship.shipName, newShip);
            trackedByLocation.Add(ship.shipName, new List<string>());

        }
        //TODO or verify the player is a verified crew member or nation member
    }
    #endregion

    #region Send
    public void SendClient(int connectionID, int recHostID,  NetMsg msg)
    {
        //holds message data
        byte[] buffer = new byte[BYTE_SIZE];

        //convert data into byte array
        BinaryFormatter formatter = new BinaryFormatter();
        MemoryStream ms = new MemoryStream(buffer);
        formatter.Serialize(ms, msg);

        if (recHostID == 0)//stand-alone client
        {
            NetworkTransport.Send(hostID, connectionID, reliableChannel, buffer, BYTE_SIZE, out error);
        }
        else//assumes Web_GL
        {
            NetworkTransport.Send(webHostID, connectionID, reliableChannel, buffer, BYTE_SIZE, out error);
        }
    }

    private void SendOnTrackedObjectChange(string tracker, string trackedName)
    {
        //get location type
        switch (trackedObjects[tracker].type)
        {
            case LOCATION_TYPE.SHIP:
                List<int> sentToIDs = new List<int>();
                DB_Ship ship = mdb.FetchShipByName(tracker);
                TrackedObject tracked = trackedObjects[trackedName];
                foreach (DB_Worker worker in mdb.FetchWorkersByShip(ship))
                {
                    DB_Player player = mdb.FetchPlayerByID(worker.owner.Id.AsObjectId);
                    //only send if the player has not already been sent to
                    if (!sentToIDs.Contains(player.ActiveConnectionID))
                    {
                        SendClient(player.ActiveConnectionID, player.ActiveHostID, new Net_OnTrackedObjectChange(trackedName,tracked.pos.x,tracked.pos.y,tracked.vel.x,tracked.vel.y));
                        sentToIDs.Add(player.ActiveConnectionID);
                    }
                }
                break;

            case LOCATION_TYPE.COLONY:
            case LOCATION_TYPE.ASTEROID:
                Debug.Log("not implemented");
                break;

            case LOCATION_TYPE.NONE:
            default:
                Debug.Log(string.Format("[Sector Server]: Unexpected tracked location type ({0}).", trackedObjects[tracker].type));
                break;

        }
    }
    #endregion

    #region GameSpace
    private void InitGameSpace()
    {
        trackedByLocation = new Dictionary<string, List<string>>();

        trackedObjects = new Dictionary<string, TrackedObject>();
        //TODO populate tracked objects with PERSISTENT things like asteroids
        
        stationaryObjects = new Dictionary<string, StationaryObject>();
        //TODO populate stationary objects with PERSISTENT things like colonies
        
    }

    private void UpdateGameSpace()
    {
        //re-build quad tree
        //TODO: dynamically pull in sector width/height
        qt = new QuadTree(new QuadTree.Rect(0, 0, 100, 100), 4);
        /*
        foreach (KeyValuePair<string, StationaryObject> stationary in stationaryObjects)
        {
            qt.insert(new QuadTree.Entity(stationary.Key, stationary.Value.pos.x, stationary.Value.pos.y));
        }*/
        foreach (KeyValuePair<string,TrackedObject> tracked in trackedObjects)
        {
            qt.insert(new QuadTree.Entity(tracked.Key,tracked.Value.pos.x,tracked.Value.pos.y));
        }

        List<string> nearby;
        foreach (KeyValuePair<string, StationaryObject> stationary in stationaryObjects)
        {
            //check a 7x7 square around each stationary object
            nearby = new List<string>();
            qt.checkArea(new QuadTree.Rect(stationary.Value.pos.x - 3.5f, stationary.Value.pos.y - 3.5f, 7, 7),nearby);

            //TODO do the same stuff as below but for the stationary here
        }
        foreach (KeyValuePair<string, TrackedObject> tracked in trackedObjects)
        {
            //check a 7x7 square around each tracked object
            nearby = new List<string>();
            qt.checkArea(new QuadTree.Rect(tracked.Value.pos.x - 3.5f, tracked.Value.pos.y - 3.5f, 7, 7), nearby);

            //compare each nearby list with what the location is currently aware of -> if they are different, send update to all players at that location
            if(trackedByLocation[tracked.Key].Count != nearby.Count)
            {
                //if the lists are different sizes -> find the difference
                
                //check if the location is not tracking something that is nearby
                foreach (string nearbyName in nearby)
                {
                    if (!trackedByLocation[tracked.Key].Contains(nearbyName))
                    {
                        trackedByLocation[tracked.Key].Add(nearbyName);
                        SendOnTrackedObjectChange(tracked.Key, nearbyName);
                    }
                }

                //remove anything the location was tracking that was not nearby
                trackedByLocation[tracked.Key] = nearby;
            }
        }
    }

    private void ShutDownGameSpace()
    {
        //TODO: force-to-hangar all ship (?)
    }
    #endregion

}
