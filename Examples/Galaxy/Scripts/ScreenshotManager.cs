using UnityEngine;
using System.Collections;

public class ScreenshotManager : MonoBehaviour {

    public int SuperSize = 5;
    public string FileName;

	// Use this for initialization
	void Start () 
    {
	    
	}
	
	// Update is called once per frame
	void Update () 
    {
	    if(Input.GetKeyDown(KeyCode.F2))
        {
            ScreenCapture.CaptureScreenshot(FileName + System.DateTime.Now.ToString("dd.mm.hh.mm.ss") + ".png", SuperSize);
        }
	}
}
