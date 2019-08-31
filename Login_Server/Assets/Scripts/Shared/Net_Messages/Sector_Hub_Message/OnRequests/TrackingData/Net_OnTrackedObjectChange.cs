[System.Serializable]
public class Net_OnTrackedObjectChange : NetSectorMessage
{

    public string locationName { get; private set; }

    public float posX { get; private set; }
    public float posY { get; private set; }
    public float velX { get; private set; }
    public float velY { get; private set; }
    
    public Net_OnTrackedObjectChange(string locationName, float posX, float posY, float velX, float velY)
    {
        OP = NetSectorOP.OnTrackedObjectChange;

        this.locationName = locationName;
        this.posX = posX;
        this.posY = posY;
        this.velX = velX;
        this.velY = velY;
    }
}
