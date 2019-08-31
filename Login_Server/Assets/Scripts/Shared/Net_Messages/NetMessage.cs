//**
//TODO: make custom binary reader/writer instead of using serializable
//**

//which server needs to recieve the message (LoginServer, SCServer, etc...)
public static class NetRT
{
    public const byte None = 0;
    public const byte LoginServer = 1;
    public const byte SectorServer = 2;
    public const byte ShipCombatServer = 3;
}

//operations should be defined by net message type (NetLoginMessage, NetSCMessage, etc...)
public static class NetOP
{
    public const byte None = 0;
}

[System.Serializable]
public abstract class NetMsg
{
    public byte RT { get; set; }    //which server to route to
    public byte OP { get; set; }    //which operation to perform

    public NetMsg()
    {
        RT = NetRT.None;    //default
        OP = NetOP.None;    //default
    }

}
