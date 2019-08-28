using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SH_CameraHandler : MonoBehaviour
{

    //TODO: make cam vel prop to zoom

    //screen aspect ratio
    public static float aspect;

    //handler references

    //camera ref
    Camera cam;       
    [SerializeField] private float minZoom;
    [SerializeField] private float maxZoom;

    //UI padding
    //0->top 1->bottom 2->left 3->right
    [SerializeField] private float[] UIPadding;

    //clip camera to SectorHubMap
    [SerializeField] private Tilemap SectorHubMap;
    private float mapWidth;
    private float mapHeight;

    //max camera speed and accleration rate
    [SerializeField] private float maxVel, accel;

    //camera zoom, position, and velocity
    private float zoom;
    private Vector3 pos;
    private Vector2 vel;

    //track mouse position
    private Vector3 lastMousePos;

    //track initialized
    private bool isInit = false;

    //check if the mouse is over a UI scroll bar
    public bool overScrollbar { get; set; }

    // Start is called before the first frame update
    public void init()
    {        
        //get aspect ratio
        aspect = (Screen.width + 1.0f) / Screen.height;

        //set cam reference
        cam = GetComponent<Camera>();

        //init map dimensions (actual)
        mapWidth = SH_Map.Instance.WIDTH;
        mapHeight = SH_Map.Instance.HEIGHT;
        
        //init zoom
        zoom = 3;
        cam.orthographicSize = zoom;

        //init velocity
        vel.x = 0;
        vel.y = 0;

        //init position
        //TODO: determine where to start camera at
        pos.x = zoom*aspect -1;
        pos.y = zoom - 1;
        pos.z = -1000;
        transform.position = pos;

        overScrollbar = false;

        isInit = true;
    }

    // Update is called once per frame
    private void Update()
    {
        if (!isInit)
        {
            return;
        }
        //check for mouse scroll
        if (Input.mouseScrollDelta.y != 0 && !overScrollbar)
        {
            zoom -= Input.mouseScrollDelta.y;
            if (zoom > minZoom)
            {
                zoom = minZoom;
            }
            else if(zoom < maxZoom)
            {
                zoom = maxZoom;
            }
        }
        if (zoom < cam.orthographicSize - .1f || zoom > cam.orthographicSize + .1f)
        {
            //lerp actual zoom to zoom var
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, zoom, 5*Time.deltaTime);
 
            //update camera position
            updateCameraPosition();
        }

        //get key input
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            vel.y += accel * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            vel.x -= accel * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            vel.y -= accel * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            vel.x += accel * Time.deltaTime;
        }
        //check no key press
        if (!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.D) && vel.magnitude > 0)
        {
            vel.x -= vel.x / 10;
            vel.y -= vel.y / 10;
        }
        //check middle mouse button (if no keys are pressed)
        else if (Input.GetMouseButton(2))
        {

            //check if this is first frame of click-drag
            if (Input.GetMouseButtonDown(2))
            {
                lastMousePos = Input.mousePosition;
            }

            //decrement coordinates by drag amount
            pos.x -= (2*cam.orthographicSize) * (Input.mousePosition.x - lastMousePos.x) / Screen.height;
            pos.y -= (2*cam.orthographicSize) * (Input.mousePosition.y - lastMousePos.y) / Screen.height;

            updateCameraPosition();

            //update last mouse pos for current click-drag
            lastMousePos = Input.mousePosition;

        }

        //check if camera is moving or not
        if (vel.magnitude < accel * Time.deltaTime / 10)  // <---not moving
        {
            vel.x = 0;
            vel.y = 0;
        }
        else    // <---moving
        {
            //cap to max speed
            if (vel.magnitude > maxVel) 
            {
                vel.Normalize();
                vel.x *= maxVel;
                vel.y *= maxVel;
            }

            //update position var
            pos.x += vel.x;
            pos.y += vel.y;

            //update actual position
            updateCameraPosition();
        }

    }

    internal void recenter(GameObject target)
    {
        transform.position = new Vector3(target.transform.position.x, target.transform.position.y, -1000);
    }

    private void updateCameraPosition()
    {
        //check if out of bounds            
        if (pos.x < cam.orthographicSize * aspect - 1 - UIPadding[2] * 2 * cam.orthographicSize * aspect)
        {
            pos.x = cam.orthographicSize * aspect - 1 - UIPadding[2] * 2 * cam.orthographicSize * aspect;
        }
        else if (pos.x > mapWidth + 1 - aspect * cam.orthographicSize + UIPadding[3] * 2 * cam.orthographicSize * aspect)
        {
            pos.x = mapWidth + 1 - aspect * cam.orthographicSize + UIPadding[3] * 2 * cam.orthographicSize * aspect;
        }

        if (pos.y < cam.orthographicSize - 1 - UIPadding[1] * 2 * cam.orthographicSize)
        {
            pos.y = cam.orthographicSize - 1 - UIPadding[1] * 2 * cam.orthographicSize;
        }
        else if (pos.y > mapHeight - cam.orthographicSize + UIPadding[0] * 2 * cam.orthographicSize)
        {
            pos.y = mapHeight - cam.orthographicSize + UIPadding[0] * 2 * cam.orthographicSize;
        }

        //update actual position
        transform.position = pos;
    }

}