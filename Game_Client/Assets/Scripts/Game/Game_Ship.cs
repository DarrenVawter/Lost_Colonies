using System.Collections.Generic;
using UnityEngine;

public class Game_Ship : MonoBehaviour
{

    //unique name of this ship
    private string shipName = null;

    //owner's username
    private string ownerName = null;

    //sector the ship is in
    private short sector = SECTOR_CODE.NONE;

    private float velX = 0;
    private float velY = 0;
    private float posX = -50;
    private float posY = -50;

    private byte activity = SHIP_ACTIVITY.NONE;

    //who is on board this ship?
    private List<Game_Worker> workersOnBoard = new List<Game_Worker>();

    //appearance
        //exterior
    public byte frame;
    public byte color1;
    public byte color2;
    public byte color3;
        //interior
    //**
    //TODO populate
    //**
    

    //stats
    //**
    //TODO populate
    //**

    //what colony & nation does this ship belong to?
    //**
    //TODO populate
    //**

    internal void updateData()
    {

    }

    /*
    internal void RequestChangeCourse()
    {
        Debug.Log("Not implemented.");
        //on course change
            //send request to server
                //server -> 
                    //validate that it is an authorized person and that ship is not busy (combat,docked,etc.)
                    //update others nearby of course change
    }
    */
}
