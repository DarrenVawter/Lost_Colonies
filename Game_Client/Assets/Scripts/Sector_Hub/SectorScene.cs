using UnityEngine;
using UnityEngine.Tilemaps;

public class SectorScene : MonoBehaviour
{
    public static SectorScene Instance;

    [SerializeField] private Tilemap SectorHubMap;

    #region monobehaviour
    void Start()
    {
        Instance = this;

        //intit scene components
        SectorHubMap.GetComponent<SH_Map>().init();
        Camera.main.GetComponent<SH_CameraHandler>().init();
    }
    #endregion

    #region ui
    internal void displayInfo(SH_Ship ship)
    {
    }
    #endregion

}
