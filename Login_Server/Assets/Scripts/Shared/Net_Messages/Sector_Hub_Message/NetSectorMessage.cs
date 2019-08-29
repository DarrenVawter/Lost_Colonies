public static class NetSectorOP
{
    public const byte None = 0;

    public const byte ThisPlayerDataRequest = 1;
    public const byte OnThisPlayerDataRequest = 2;

    public const byte RequestShipCombat = 3;
    public const byte OnRequestShipCombat = 4;

    public const byte RequestCourseChange = 5;
    public const byte OnRequestCourseChange = 6;

    public const byte LocationDataRequest = 7;
    public const byte OnLocationDataRequest = 8;

    public const byte WorkerDataRequest = 9;
    public const byte OnWorkerDataRequest = 10;
}

[System.Serializable]
public abstract class NetSectorMessage : NetMsg
{

    public NetSectorMessage()
    {
        RT = NetRT.SectorServer;    //all login messages should be sent to login server
        OP = NetSectorOP.None;      //default
    }

}
