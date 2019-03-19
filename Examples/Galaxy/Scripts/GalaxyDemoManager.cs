using UnityEngine;
using System.Collections;
using Galaxia;
using UnityEngine.UI;

public class GalaxyDemoManager : MonoBehaviour {

    public GalaxyPrefab[] galaxyPrefabs;
    public Galaxy Galaxy;
	public OffscreenGalaxyRenderer OffscreenGalaxyRenderer;

	// Use this for initialization
	void Start () 
    {
        
	}

    void OnGUI()
    {
        float lastY = 10;
        for(int i = 0;i < galaxyPrefabs.Length;i++)
        {

            if (GUI.Button(new Rect(10, lastY, 128, 32), galaxyPrefabs[i].name))
            {
                Galaxy.GalaxyPrefab = galaxyPrefabs[i];
            }

            lastY += 37;
        }

        Galaxy.GPU = GUI.Toggle(new Rect(10, lastY, 128, 32), Galaxy.GPU, "GPU");
		lastY += 37;
		OffscreenGalaxyRenderer.enabled = GUI.Toggle(new Rect(10, lastY, 128, 32), OffscreenGalaxyRenderer.enabled, "Off-Screen Particles");

		string info = "";
        info = "Shader model " + SystemInfo.graphicsShaderLevel;
        if (SystemInfo.graphicsShaderLevel >= 40)
        {
            info += " (Supports DirectX11)";
        }else
        {
            info += " (Does not support DirectX11)";
        }

        info += "\n " + SystemInfo.graphicsDeviceVersion;
        if(SystemInfo.graphicsDeviceVersion.Contains("OpenGL"))
        {
            info += " (Uses OpenGL)";
        }
        else
        {
            info += " (Uses Direct3D)";
        }
        GUI.Label(new Rect(Screen.width - 128, 10, 128, 128), info);
		
    }
	
	// Update is called once per frame
	void Update () 
    {
	    
	}
}
