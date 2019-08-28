public class Location
{    
    public string locationName { get; set; }
    public bool isShip { get; private set; }//false->isColony
    public short sector { get; private set; }
    public short gridX { get; private set; }
    public short gridY { get; private set; }

    public Location(string locationName, bool isShip, short sector, short gridX, short gridY)
    {
        this.locationName = locationName;
        this.isShip = isShip;
        this.sector = sector;
        this.gridX = gridX;
        this.gridY = gridY;
    }
}