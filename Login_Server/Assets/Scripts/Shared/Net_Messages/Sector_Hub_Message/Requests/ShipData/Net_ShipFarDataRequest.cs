[System.Serializable]
public class Net_ShipFarDataRequest : NetSectorMessage
{
    public string shipName { get; }

    public Net_ShipFarDataRequest(string shipName)
    {
        OP = NetSectorOP.ShipFarDataRequest;

        this.shipName = shipName;
    }
}
