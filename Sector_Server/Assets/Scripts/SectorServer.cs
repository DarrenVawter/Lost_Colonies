using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Networking;

/* TODO notes:
 *
 *  Verify ship combat request
 * 
 */

public class SectorServer : MonoBehaviour
{

    #region Fields
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

    #region Monobehavior
    private void Start()
    {
        isStarted = false;
        DontDestroyOnLoad(gameObject);
        Init();
    }
    private void Update()
    {
        UpdateMessagePump();
    }
    #endregion

    #region NetworkBehavior
    public void Init()
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
            case NetSectorOP.ThisPlayerDataRequest:
                ThisPlayerDataRequest(connectionID, recHostID, (Net_ThisPlayerDataRequest)msg);
                break;

            case NetSectorOP.RequestShipCombat:
                //TODO: handle requested ship combat with targeted ship
                break;

            case NetSectorOP.LocationDataRequest:
                LocationDataRequest(connectionID, recHostID, (Net_LocationDataRequest)msg);
                break;

            case NetSectorOP.WorkerDataRequest:
                WorkerDataRequest(connectionID, recHostID, (Net_WorkerDataRequest)msg);
                break;

            default:
                Debug.Log("[Sector Server]: Unexpected Net OP (" + msg.OP + ").");
                break;
        }
    }

    private void ThisPlayerDataRequest(int connectionID, int recHostID, Net_ThisPlayerDataRequest msg)
    {
        //fetch player data by token
        Debug.Log(msg.token);
        Model_Player player = mdb.FetchPlayerByToken(msg.token);

        //fetch model_workers by player and convert to game_workers
        List<Game_Worker> workers = new List<Game_Worker>();
        List<Model_Worker> mWorkers = mdb.FetchWorkersByPlayer(player);
        foreach (Model_Worker mWorker in mWorkers)
        {            
            workers.Add(new Game_Worker(mWorker.ownerName, mWorker.locationName, mWorker.sector, mWorker.workerName, mWorker.isInCombat, mWorker.activity));
        }
                
        //send messsage
        SendClient(connectionID, recHostID, new Net_OnThisPlayerDataRequest(player.Username,player.Discriminator,workers));
    }

    private void LocationDataRequest(int connectionID, int recHostID, Net_LocationDataRequest msg)
    {
        Model_Ship ship = mdb.FetchShipByName(msg.locationName);
        if (ship != null)
        {
            SendClient(connectionID, recHostID, new Net_OnLocationDataRequest(new Game_Location(ship.shipName, true, ship.sector, (short)Mathf.RoundToInt(ship.posX), (short)Mathf.RoundToInt(ship.posY))));
        }
        else
        {
            //TODO: check if location is a colony (or other)
        }
    }

    private void WorkerDataRequest(int connectionID, int recHostID, Net_WorkerDataRequest msg)
    {
        Model_Worker worker = mdb.FetchWorkerByName(msg.workerName);
        if(worker != null)
        {
            SendClient(connectionID, recHostID, new Net_OnWorkerDataRequest(worker.ownerName, worker.workerName, worker.locationName, worker.sector, worker.isInCombat, worker.activity));
        }
        else
        {
            //worker was not found
        }
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
    #endregion

}
