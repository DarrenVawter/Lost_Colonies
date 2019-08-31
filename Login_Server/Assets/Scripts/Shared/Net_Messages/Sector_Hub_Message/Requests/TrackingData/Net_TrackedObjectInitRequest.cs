[System.Serializable]
public class Net_TrackedObjectInitRequest : NetSectorMessage
{

    public string token { get; }
    public string locationName { get; }

    public Net_TrackedObjectInitRequest(string token, string locationName)
    {
        OP = NetSectorOP.TrackedObjectInitRequest;

        this.token = token;
        this.locationName = locationName;
    }
}
