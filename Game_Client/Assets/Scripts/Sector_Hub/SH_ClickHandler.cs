using UnityEngine;

public class SH_ClickHandler : MonoBehaviour
{
    #region fields
    //ref to map component
    private SH_Map map;

    //ref to camera
    [SerializeField] private Camera mainCam;
    
    //what next click can do
    public enum ClickAction : byte {none,changeCourse};
    public ClickAction nextClickAction;

    //what is currently clicked
    private GameObject clicked;

    //the object that was just  downclicked 
    //(check to see if same as upclicked to register a complete click)
    private GameObject downClicked;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        //init map reference
        map = GetComponent<SH_Map>();

        //init empty click action
        nextClickAction = ClickAction.none;
    }

    // Update is called once per frame
    void Update()
    {
    
        //left clicked down
        if (Input.GetMouseButtonDown(0))
        {
            //get world point clicked on screen
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //convert to 2d
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

            //raycast
            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

            //check if ray cast collided with clickable
            if (hit.collider != null)
            {
                downClicked = hit.collider.gameObject;
            }
        }
        
        //left lifted up
        else if (Input.GetMouseButtonUp(0)) { 

            //get world point clicked on screen
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            
            //convert to 2d
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

            //check if action is already requested
            switch (nextClickAction)
            {
                case ClickAction.changeCourse:
                    break;

                default:
                    break;
            }

            //otherwise, check for collision clicks

            //raycast
            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

            //check if ray cast collided with clickable
            if (hit.collider != null && hit.collider.gameObject == downClicked && hit.collider.gameObject.GetComponent<SH_Clickable>() != null)
            {
                switch (hit.collider.gameObject.GetComponent<SH_Clickable>().TYPE)
                {
                    case LOCATION_TYPE.SHIP:
                        //SectorScene.Instance.displayInfo(hit.collider.gameObject.GetComponent<Game_Ship>());
                        break;

                    case LOCATION_TYPE.COLONY:
                        Debug.Log("Colony clickable not yet implemented.");
                        break;

                    case LOCATION_TYPE.NONE:
                    default:
                        Debug.Log(string.Format("Unexpected SH_Clickable type ({0}).", hit.collider.gameObject.GetComponent<SH_Clickable>().TYPE));
                        break;
                }
            }
            else
            {
                map.removeHighlight();
            }
        }

    }

}