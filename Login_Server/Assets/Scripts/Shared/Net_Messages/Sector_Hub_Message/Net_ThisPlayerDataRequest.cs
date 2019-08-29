[System.Serializable]
public class Net_ThisPlayerDataRequest : NetSectorMessage
{
    public string token { get; }

    public Net_ThisPlayerDataRequest(string token)
    {
        OP = NetSectorOP.ThisPlayerDataRequest;

        this.token = token;
    }
}
