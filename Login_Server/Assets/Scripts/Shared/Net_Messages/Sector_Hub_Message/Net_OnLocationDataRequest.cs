[System.Serializable]
public class Net_OnLocationDataRequest : NetSectorMessage
{
    public Game_Location location { get; }

    public Net_OnLocationDataRequest(Game_Location location)
    {
        OP = NetSectorOP.OnLocationDataRequest;

        this.location = location;
    }
}
