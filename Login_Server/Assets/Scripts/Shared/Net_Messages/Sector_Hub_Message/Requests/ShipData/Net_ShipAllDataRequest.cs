[System.Serializable]
public class Net_ShipAllDataRequest : NetSectorMessage
{
    public string token { get; }
    public string shipName { get; }

    public Net_ShipAllDataRequest(string token, string shipName)
    {
        OP = NetSectorOP.ShipAllDataRequest;

        this.token = token;
        this.shipName = shipName;
    }
}
