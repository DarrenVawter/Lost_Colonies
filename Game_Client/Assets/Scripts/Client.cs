using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class Client : MonoBehaviour
{
    public static Client Instance { private set; get; }

    #region Connection_Info
    //private const string LOGIN_SERVER_IP = "130.211.235.128"; //TODO: make this static @GCP
    private string SERVER_IP = "127.0.0.1"; //login server by default
    private int PORT = 27987;               //TODO: make me const
    private const int WEB_PORT = 27988;
    private const int BYTE_SIZE = 1024;

    private const int MAX_USER = 100;

    //has server started?
    private bool isStarted;
    private bool isConnected;

    //communication channel between host and client(s)
    private byte reliableChannel;

    private int connectionID;

    private int hostID;

    private byte error;
    
    private string RSAPubKey;
    private string token;
    #endregion

    #region Stored_Data
    Dictionary<string,Game_Location> locations = new Dictionary<string, Game_Location>();
    Dictionary<string, Game_Worker> workers = new Dictionary<string, Game_Worker>();
    Queue<NetMsg> sendQueue = new Queue<NetMsg>();
    #endregion

    #region Monobehavior
    private void Start()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Connect();
    }

    private void Update()
    {
        UpdateMessagePump();
        SendServer();
    }
    #endregion

    #region NetworkBehavior
    public void Connect()
    {

        NetworkTransport.Init();

        ConnectionConfig cc = new ConnectionConfig();
        //reliable -> spending gold to buy item (100% delivery but can clog)
        //unreliable -> update position (not 100% delivery but won't clog)
        //research the other QoSTypes
        reliableChannel = cc.AddChannel(QosType.Reliable);

        HostTopology topo = new HostTopology(cc, MAX_USER);

        //*********************client only code*********************************

        hostID = NetworkTransport.AddHost(topo, 0);

        //check if on web or stand-alone
#if UNITY_WEBGL && !UNITY_EDITOR
        //web client
        connectionID = NetworkTransport.Connect(hostID, SERVER_IP, WEB_PORT, 0, out error);
        Debug.Log(string.Format("[Network Client]: Connecting to {0}:{1}...", SERVER_IP, WEB_PORT));
#else
        //standalone client
        connectionID = NetworkTransport.Connect(hostID, SERVER_IP, PORT, 0, out error);
        Debug.Log(string.Format("[Network Client]: Connecting to {0}:{1}...", SERVER_IP, PORT));
#endif
        isStarted = true;
    }
    public void ShutdownNetwork()
    {        

        //TODO: disconnect from old server (trigger disconnect event for logging purposes)
        NetworkTransport.Shutdown();
        isStarted = false;
        isConnected = false;
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
                Debug.Log(string.Format("[Network Client]: Recieved Data (RT-{0}, OP-{1}).", msg.RT,msg.OP));
                OnData(connectionID, channelID, recHostID, msg);
                break;

            case NetworkEventType.ConnectEvent:
                Debug.Log(string.Format("[Network Client]: Connected to server {0}.", connectionID));
                isConnected = true;
                break;

            case NetworkEventType.DisconnectEvent:
                //TODO: attempt reconnect
                Debug.Log(string.Format("[Network Client]: Disconnected from server {0}.", connectionID));
                isConnected = false;
                break;

            case NetworkEventType.BroadcastEvent:
            default:
                Debug.Log("[Network Client]: Unexpected Network Event Type (" + type + ").");
                break;
        }
    }
    #endregion

    #region OnData
    private void OnData(int connectionID, int channelID, int recHostID, NetMsg msg)
    {
        switch (msg.RT)
        {
            case NetRT.LoginServer:
                OnLoginMessage(connectionID,channelID,recHostID,msg);
                break;

            case NetRT.SectorServer:
                OnSectorMessage(connectionID, channelID, recHostID, msg);
                break;

            case NetRT.ShipCombatServer:
                break;

            case NetRT.None:
            default:
                Debug.Log("[Network Client]: Unexpected Net RT (" + msg.RT + ").");
                break;

        }
    }
    
    #region Login_Screen
    private void OnLoginMessage(int connectionID, int channelID, int recHostID, NetMsg msg)
    {
        switch (msg.OP)
        {
            case NetLoginOP.OnCreateAccount:
                OnCreateAccount((Net_OnCreateAccount)msg);
                break;

            case NetLoginOP.OnLoginRequest:
                OnLoginRequest(connectionID, (Net_OnLoginRequest)msg);
                break;

            case NetLoginOP.OnKeyUpdate:
                OnKeyUpdate((Net_KeyUpdate)msg);
                break;

            case NetLoginOP.None:
            default:
                Debug.Log("[Network Client]: Unexpected Net OP (" + msg.OP + ").");
                break;
        }
    }

    private void OnKeyUpdate(Net_KeyUpdate ku)
    {
        RSAPubKey = ku.pubKeyString;
    }

    private void OnCreateAccount(Net_OnCreateAccount oca)
    {
        //not successful
        if (oca.response == 0)
        {
            LoginScene.Instance.EnableInputs();
            LoginScene.Instance.ChangeAuthenticationText("Account Creation Failed");
        }
        else
        {
            LoginScene.Instance.ChangeAuthenticationText("Successfully Created Account");
            LoginScene.Instance.EnableInputs();
        }
    }
    
    private void OnLoginRequest(int connectionID, Net_OnLoginRequest olr)
    {
        if (olr.response == 0)
        {
            //not successful
            LoginScene.Instance.EnableInputs();
            LoginScene.Instance.ChangeAuthenticationText("Invalid Login");
        }
        else
        {
            //successful
            LoginScene.Instance.ChangeAuthenticationText("Successful Login");

            //load sector hub
            SceneManager.LoadScene("Sector_Hub", LoadSceneMode.Single);

            //get login token from message
            token = olr.token;

            //disconnect from login server
            ShutdownNetwork();

            //connect to sector server
            SERVER_IP = olr.sectorIP;
            PORT = 27989; //TODO: change & delete me
            Connect();

            if (!isStarted)
            {
                //failed to connect to sector server
                Debug.Log("[Network Client]: Failed to connect to sector server.");
                return;
            }

            //*now connected to sector server*
            
            //request info to init player game object
            SendThisPlayerDataRequest(token);
        }
    }
    #endregion

    #region Sector_Hub
    private void OnSectorMessage(int connectionID, int channelID, int recHostID, NetMsg msg)
    {
        switch (msg.OP)
        {
            case NetSectorOP.OnThisPlayerDataRequest:
                OnThisPlayerDataRequest((Net_OnThisPlayerDataRequest)msg);
                break;

            case NetSectorOP.OnLocationDataRequest:
                OnLocationDataRequest((Net_OnLocationDataRequest)msg);
                break;

            case NetSectorOP.OnWorkerDataRequest:
                OnWorkerDataRequest((Net_OnWorkerDataRequest)msg);
                break;

            case NetSectorOP.None:
            default:
                Debug.Log("[Network Client]: Unexpected Net OP (" + msg.OP + ").");
                break;
        }
    }

    private void OnThisPlayerDataRequest(Net_OnThisPlayerDataRequest msg)
    {
        //TODO: reference player instance AFTER it has been loaded

        //load player
        Game_Player.Instance.init(msg);
    }

    private void OnLocationDataRequest(Net_OnLocationDataRequest msg)
    {
        locations.Add(msg.location.locationName, msg.location);
    }

    private void OnWorkerDataRequest(Net_OnWorkerDataRequest msg)
    {
        workers.Add(msg.name, new Game_Worker(msg.owner,msg.location,msg.sector,msg.name,msg.isInCombat,msg.activity));
    }
    #endregion
    
    #endregion

    #region Send
    private void SendServer()
    {
        if (!isConnected || sendQueue.Count == 0)
        {
            //TODO create a timer somewhere and allow max attempted time for sends
            return;
        }

        //holds message data
        byte[] buffer = new byte[BYTE_SIZE];

        //convert data into byte array
        BinaryFormatter formatter = new BinaryFormatter();
        MemoryStream ms = new MemoryStream(buffer);
        formatter.Serialize(ms, sendQueue.Dequeue());

        NetworkTransport.Send(hostID, connectionID, reliableChannel, buffer, BYTE_SIZE, out error);
    }
    private void AddToSendQueue(NetMsg msg)
    {
        sendQueue.Enqueue(msg);
    }

    #region Login_Screen
    internal void SendCreateAccount()
    {

        string username = GameObject.Find("CreateUsername").GetComponent<TMP_InputField>().text;
        string hashedPassword = Utility.Sha256FromString(GameObject.Find("CreatePassword").GetComponent<TMP_InputField>().text);
        string email = GameObject.Find("CreateEmail").GetComponent<TMP_InputField>().text;

        if (!Utility.IsUsername(username))
        {
            //invalid username
            LoginScene.Instance.ChangeAuthenticationText("Invalid Username");
            LoginScene.Instance.EnableInputs();
            return;
        }

        if (!Utility.IsEmail(email))
        {
            //invalid email
            LoginScene.Instance.ChangeAuthenticationText("Invalid e-mail");
            LoginScene.Instance.EnableInputs();
            return;
        }

        if (GameObject.Find("CreatePassword").GetComponent<TMP_InputField>().text == null || GameObject.Find("CreatePassword").GetComponent<TMP_InputField>().text == "")
        {
            //empty pass
            LoginScene.Instance.ChangeAuthenticationText("Empty Password");
            LoginScene.Instance.EnableInputs();
            return;
        }

        if (RSAPubKey == null)
        {
            //TODO: request RSA PUB KEY
            Debug.Log("[Network Client]: Awaiting Encrypted transmission.");
            return;
        }

        LoginScene.Instance.ChangeAuthenticationText("Creating Account...");
        AddToSendQueue(new Net_CreateAccount(username, hashedPassword, email));
    }

    internal void SendLoginRequest()
    {
        string user = GameObject.Find("LoginUsernameEmail").GetComponent<TMP_InputField>().text;
        string hashedPassword = Utility.Sha256FromString(GameObject.Find("LoginPassword").GetComponent<TMP_InputField>().text);
        
        if (!Utility.IsUsername(user) && !Utility.IsEmail(user))
        {
            //invalid username
            LoginScene.Instance.ChangeAuthenticationText("Invalid Username or Email");
            LoginScene.Instance.EnableInputs();
            return;
        }

        if (GameObject.Find("LoginPassword").GetComponent<TMP_InputField>().text == null || GameObject.Find("LoginPassword").GetComponent<TMP_InputField>().text == "")
        {
            //empty pass
            LoginScene.Instance.ChangeAuthenticationText("Empty Password");
            LoginScene.Instance.EnableInputs();
            return;
        }

        LoginScene.Instance.ChangeAuthenticationText("Logging in...");
        AddToSendQueue(new Net_LoginRequest(user, RSA.encrypt(hashedPassword, RSAPubKey)));
    }
    #endregion

    #region Sector_Hub

    private void SendThisPlayerDataRequest(string token)
    {
        AddToSendQueue(new Net_ThisPlayerDataRequest(token));
    }

    internal void SendWorkerDataRequest(string name)
    {
        AddToSendQueue(new Net_WorkerDataRequest(name));
    }

    private void SendLocationDataRequest(string locationName)
    {
        AddToSendQueue(new Net_LocationDataRequest(locationName));
    }
    
    #endregion

    #endregion

    #region GetData

    //TODO : maybe remove this?
    #region Sector_Hub
    internal Game_Location GetLocationData(string locationName)
    {
        if (locations[locationName] != null)
        {
            Game_Location loc = locations[locationName];
            locations.Remove(locationName);
            return loc;
        }
        else
        {
            return null;
        }
    }

    internal Game_Worker GetWorkerData(string workerName)
    {
        if (workers[workerName] != null)
        {
            Game_Worker workerData = workers[workerName];
            workers.Remove(workerName);
            return workerData;
        }
        else
        {
            return null;
        }
    }
    #endregion

    #endregion

}
