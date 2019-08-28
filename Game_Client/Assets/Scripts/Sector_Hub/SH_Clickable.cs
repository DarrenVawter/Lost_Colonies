using UnityEngine;

public static class SH_TYPE
{
    public const byte None = 0;
    public const byte Ship = 1;
    public const byte Colony = 2;
    //Asteroid belt
    //Mega-asteroid
    //Other events
}

public class SH_Clickable : MonoBehaviour
{
    public byte TYPE { get; set; }    //type of clickable

    public SH_Clickable()
    {
        TYPE = SH_TYPE.None;    //default
    }

}
