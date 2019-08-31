using UnityEngine;

public class SH_Clickable : MonoBehaviour
{
    public byte TYPE { get; set; }    //type of clickable

    public SH_Clickable()
    {
        TYPE = LOCATION_TYPE.NONE;    //default
    }

}
