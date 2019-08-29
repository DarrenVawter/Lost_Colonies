[System.Serializable]
public class Net_OnLocationDataRequest : NetSectorMessage
{
    public Message_Location location { get; }

    public Net_OnLocationDataRequest(Message_Location location)
    {
        OP = NetSectorOP.OnLocationDataRequest;

        this.location = location;
    }
}
