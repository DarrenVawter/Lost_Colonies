[System.Serializable]
public class Net_OnThisPlayerDataRequest : NetSectorMessage
{
    internal Message_Player player { get; set; }

    public Net_OnThisPlayerDataRequest(Message_Player player)
    {
        OP = NetSectorOP.OnThisPlayerDataRequest;

        this.player = player;
    }
}
