using UnityEngine;

public class SH_Ship : MonoBehaviour
{

    private short sector;

    private float velX;
    private float velY;
    private float posX;
    private float posY;

    private bool isDocked;

    private void Start()
    {
        velX = 0;
        velY = 0;
        posX = -50;
        posY = -50;
        transform.position = new Vector3(posX, posY, -999);

        //get ship data from server

    }

    internal void RequestChangeCourse()
    {
        Debug.Log("Not implemented.");
        //on course change
            //send request to server
                //server -> 
                    //validate that it is an authorized person and that ship is not busy (combat,docked,etc.)
                    //update others nearby of course change
    }


}
