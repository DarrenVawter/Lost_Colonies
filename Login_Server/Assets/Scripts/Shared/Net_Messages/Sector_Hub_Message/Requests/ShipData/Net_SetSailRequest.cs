[System.Serializable]
public class Net_SetSailRequest : NetSectorMessage
{
    public string token { get; }
    public string shipName { get; }

    public Net_SetSailRequest(string token, string shipName)
    {
        OP = NetSectorOP.SetSailRequest;

        this.token = token;
        this.shipName = shipName;
    }
}
