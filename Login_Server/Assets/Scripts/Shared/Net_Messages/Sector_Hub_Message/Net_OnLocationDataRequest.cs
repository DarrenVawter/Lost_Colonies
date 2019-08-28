[System.Serializable]
public class Net_OnLocationDataRequest : NetSectorMessage
{
    public string locationName { get; set; }
    public bool isShip { get; set; }//false->isColony
    public short sector { get; set; }
    public short gridX { get; set; }
    public short gridY { get; set; }

    public Net_OnLocationDataRequest(string locationName, bool isShip, short sector, short gridX, short gridY)
    {
        OP = NetSectorOP.OnLocationDataRequest;

        this.locationName = locationName;
        this.isShip = isShip;
        this.sector = sector;
        this.gridX = gridX;
        this.gridY = gridY;
    }
}
