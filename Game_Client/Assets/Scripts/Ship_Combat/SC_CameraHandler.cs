using UnityEngine;

public class SC_CameraHandler : MonoBehaviour
{
    
    private void Start()
    {

        #region skybox
        //rotate skybox
        RenderSettings.skybox.SetFloat("_Rotation", Random.Range(0,361));

        //tint skybox
        Vector3Int tint = new Vector3Int(128, 255, Random.Range(128, 256));
        int r = Random.Range(0, 3);
        int g = Random.Range(0, 3);
        int b;
        while (g == r)
        {
            g = Random.Range(0, 3);
        }
        if (r == 1)
        {
            r = tint.x;
            if (g == 2)
            {
                g = tint.y;
                b = tint.z;
            }
            else
            {
                g = tint.z;
                b = tint.y;
            }
        }
        else if (r == 2)
        {
            r = tint.y;
            if (g == 1)
            {
                g = tint.x;
                b = tint.z;
            }
            else
            {
                g = tint.z;
                b = tint.x;
            }
        }
        else
        {
            r = tint.z;
            if (g == 1)
            {
                g = tint.x;
                b = tint.y;
            }
            else
            {
                g = tint.y;
                b = tint.x;
            }
        }
        
        RenderSettings.skybox.SetColor("_Tint", new Color(r/255.0f,g/255.0f,b/255.0f));
        #endregion
               
    }

    private void Update()
    {
    }

    public void init()
    {

    }

}
