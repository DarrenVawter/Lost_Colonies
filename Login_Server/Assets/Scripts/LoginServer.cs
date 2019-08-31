using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Networking;

/* TODO notes:
 *
 *  -remove connection id from dictionary on logout
 *  -make sector ip's static and tie them in to the switch statement @on login request
 * 
 */
public class LoginServer : MonoBehaviour
{

    #region Fields
    //consts
    private const int MAX_USER = 100;
    private const int PORT = 27987;
    private const int WEB_PORT = 27988;
    private const int BYTE_SIZE = 1024;

    //Dictionary mapping connection IDs to their respective private RSA keys
    private Dictionary<int,string> connectionKeys;

    //has server started?
    private bool isStarted;

    //error flag
    private byte error;

    //communication channel between host and client(s)
    private byte reliableChannel;
    
    //host ids
    private int hostID;
    private int webHostID;

    //db reference
    private MongoDatabase mdb;
    #endregion

    #region Monobehavior
    private void Start()
    {
        connectionKeys = new Dictionary<int, string>();
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

        Debug.Log(string.Format("[Login Server]: Connection open on port {0} and webport {1}.", PORT,WEB_PORT));
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
                Debug.Log(string.Format("[Login Server]: Recieved Data (RT-{0}, OP-{1}).", msg.RT, msg.OP));
                OnData(connectionID, channelID, recHostID, msg);
                break;
            
            case NetworkEventType.ConnectEvent:
                Debug.Log(string.Format("[Login Server]: User {0} connected through host {1}.", connectionID, recHostID));
                GenerateConnectionRSAKeys(connectionID, recHostID);
                break;

            case NetworkEventType.DisconnectEvent:
                Debug.Log(string.Format("[Login Server]: User {0} disconnected.", connectionID));
                break;

            case NetworkEventType.BroadcastEvent:
            default:
                Debug.Log("[Login Server]: Unexpected Network Event Type.");
                break;
        }
    }
    private void GenerateConnectionRSAKeys(int connectionID, int recHostID)
    {
        //generate new key pair
        string[] keys = RSA.getKeys();

        //add private key to active, on-server dictionary
        connectionKeys[connectionID] = keys[1];

        //send public key to user
        SendClient(connectionID, recHostID, new Net_KeyUpdate(keys[0]));
    }
    #endregion

    #region OnData
    private void OnData(int connectionID, int channelID, int recHostID, NetMsg msg)
    {

        switch (msg.OP)
        {
            case NetLoginOP.CreateAccount:
                CreateAccount(connectionID, recHostID, (Net_CreateAccount) msg);
                break;
            case NetLoginOP.LoginRequest:
                LoginRequest(connectionID, recHostID, (Net_LoginRequest)msg);
                break;

            case NetLoginOP.None:
                Debug.Log("[Login Server]: Unexpected Net OP.");
                break;
        }
    }

    private void CreateAccount(int connectionID, int recHostID, Net_CreateAccount ca)
    {

        if (mdb.CreateAccount(ca.username, ca.password, ca.email))
        {
            //respond to client (success)
            SendClient(connectionID, recHostID, new Net_OnCreateAccount(1));
            //deal with client data
            Debug.Log(string.Format("[Login Server]: Created Account: {0}: ({1})", ca.username, ca.email));

        }
        else
        {
            //respond to client (invalid)
            SendClient(connectionID, recHostID, new Net_OnCreateAccount(0));
        }
    }
  
    private void LoginRequest(int connectionID, int recHostID, Net_LoginRequest lr)
    {
        string loginToken = Utility.GenerateRandomToken(256);
        DB_Account account = mdb.LoginAccount(connectionID, recHostID, lr.user, RSA.decrypt(lr.password,connectionKeys[connectionID]), loginToken);
                
        if(account != null)
        {
            //successful login
            
            //get appropriate sector ip
            string sectorIP = null;
            switch (account.Sector)
            {
                case SECTOR_CODE.REDSECTOR:
                    sectorIP = "127.0.0.1";
                    break;

                case SECTOR_CODE.NONE:
                default:
                    break;
            }

            //update player connection id
            DB_Player player = mdb.FetchPlayerByUsernameAndDiscriminator(account.Username + "#" + account.Discriminator);
            player.Token = loginToken;
            mdb.updatePlayer(player);

            //respond to client
            SendClient(connectionID, recHostID, new Net_OnLoginRequest(1, account.Username, account.Discriminator, loginToken, sectorIP));
            //server log
            Debug.Log(string.Format("[Login Server]: {0}#{1} logged in.", account.Username, account.Discriminator));
        }
        else
        {
            //invalid login attempt
            //respond to client
            SendClient(connectionID, recHostID, new Net_OnLoginRequest(0, null, null, null,null));
            //respond to client
            Debug.Log(string.Format("[Login Server]: Invalid login attempt ({0}).", lr.user));
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
