using UnityEngine;
using UnityEngine.Tilemaps;

public class SH_Map : MonoBehaviour
{
    //static reference to the SectorMap
    public static SH_Map Instance { private set; get; }

    //tiles that can be used
    [SerializeField] private TileBase empty;

    //TODO: determine sector map size
    public int WIDTH { get; private set; }
    public int HEIGHT { get; private set; }

    //map's tile array
    public int[,] map { get; private set; }

    //init all tiles
    //TODO: load from server (also, figure efficient way of doing this communication)
    public void init()
    {
        Instance = this;

        WIDTH = 100;
        HEIGHT = 100;

        map = new int[WIDTH, HEIGHT];

        for (int x = 0; x < WIDTH; x++)
        {
            for (int y = 0; y < HEIGHT; y++)
            {
                map[x, y] = 0;
                set(x, y, 0);
            }
        }
    }
    
    //set tile
    public void set(int x, int y, int i)
    {
        switch (i)
        {
            case 0:
                GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), empty);
                break;
        }
    }

    public void removeHighlight()
    {
        //TODO: implement
        //HighlightMap.ClearAllTiles();
        //GetComponent<ClickHandler>().nextClickAction = ClickHandler.ClickAction.none;
    }

}
