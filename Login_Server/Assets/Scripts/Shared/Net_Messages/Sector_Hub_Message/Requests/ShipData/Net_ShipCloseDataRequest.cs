[System.Serializable]
public class Net_ShipCloseDataRequest : NetSectorMessage
{
    public string token { get; }
    public string shipName { get; }

    public Net_ShipCloseDataRequest(string token, string shipName)
    {
        OP = NetSectorOP.ShipCloseDataRequest;

        this.token = token;
        this.shipName = shipName;
    }
}
