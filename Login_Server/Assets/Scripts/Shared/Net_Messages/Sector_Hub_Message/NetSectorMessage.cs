public static class NetSectorOP
{
    public const byte None = 0;

    #region PlayerData
    public const byte InitPlayerDataRequest = 1;
    public const byte OnInitPlayerDataRequest = 2;
    public const byte ActiveWorkerChangeRequest = 3;
    public const byte OnActiveWorkerChange = 4;
    #endregion

    #region TrackingData
    public const byte OnTrackedObjectChange = 5;
    public const byte TrackedObjectInitRequest = 6;
    public const byte OnTrackedObjectInitRequest = 7;
    #endregion

    #region ShipData
    public const byte SetSailRequest = 17;
    public const byte ShipAllDataRequest = 11;
    public const byte OnShipAllDataRequest = 12;
    public const byte ShipCloseDataRequest = 13;
    public const byte OnShipCloseDataRequest = 14;
    public const byte ShipFarDataRequest = 15;
    public const byte OnShipFarDataRequest = 16;
    #endregion
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
