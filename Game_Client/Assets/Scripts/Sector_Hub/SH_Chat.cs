using UnityEngine;

public class SH_Chat : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnMouseOver()
    {
        Camera.main.GetComponent<SH_CameraHandler>().overScrollbar = true;
    }

    public void OnMouseExit()
    {
        Camera.main.GetComponent<SH_CameraHandler>().overScrollbar = false;
    }

}
