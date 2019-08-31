public class Game_Location
{    
    public string locationName { get; set; }
    public byte locationType { get; set; }
    public short sector { get; private set; }
    public short gridX { get; private set; }
    public short gridY { get; private set; }

    public Game_Location(string locationName, byte locationType, short sector, short gridX, short gridY)
    {
        this.locationName = locationName;
        this.locationType = locationType;
        this.sector = sector;
        this.gridX = gridX;
        this.gridY = gridY;
    }
}