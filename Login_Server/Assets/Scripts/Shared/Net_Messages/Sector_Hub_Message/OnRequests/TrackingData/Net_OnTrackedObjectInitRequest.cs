[System.Serializable]
public class Net_OnTrackedObjectInitRequest : NetSectorMessage
{
    public string locationName { get; set;  }
    public byte[] locationData { get; set;  }
       
    public Net_OnTrackedObjectInitRequest()
    {
        OP = NetSectorOP.OnTrackedObjectInitRequest;

        locationName = null;
        locationData = null;
    }
}
