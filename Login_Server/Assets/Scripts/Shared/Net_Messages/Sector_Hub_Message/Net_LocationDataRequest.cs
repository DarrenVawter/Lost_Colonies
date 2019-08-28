[System.Serializable]
public class Net_LocationDataRequest : NetSectorMessage
{
    public string locationName { get; set; }

    public Net_LocationDataRequest(string locationName)
    {
        OP = NetSectorOP.LocationDataRequest;

        this.locationName = locationName;
    }

}
