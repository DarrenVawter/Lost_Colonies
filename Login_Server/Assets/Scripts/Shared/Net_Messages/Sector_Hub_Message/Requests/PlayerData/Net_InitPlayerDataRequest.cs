[System.Serializable]
public class Net_InitPlayerDataRequest : NetSectorMessage
{
    public string token { get; }

    public Net_InitPlayerDataRequest(string token)
    {
        OP = NetSectorOP.InitPlayerDataRequest;

        this.token = token;
    }
}
