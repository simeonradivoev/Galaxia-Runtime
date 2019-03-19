using UnityEngine;
using System.Collections;
using Galaxia;

public class VRGalaxyManager : MonoBehaviour
{
	public Galaxy Galaxy;
	public Transform m_camera;
	public Transform viewPointsGroup;
	public bool offscreenParticlesEnabled = true;
	public bool hdr = true;
	public bool adjustCameraRotation = false;

	private bool lastOffscreenParticlesEnabled = true;
	private bool lastHDR = true;
	private int viewsCounter;

	private void Start()
	{
		ApplyOffscreenParticleStates();
		lastOffscreenParticlesEnabled = offscreenParticlesEnabled;
		lastHDR = hdr;
	}

	private void OnGUI()
	{
		Transform view = viewPointsGroup.GetChild(viewsCounter % viewPointsGroup.childCount);
        GUILayout.Label("Currently view: " + view.name);
		GUILayout.Label("O - Toggle Offscreen Galaxy Rendering");
		GUILayout.Label("V - Cycle Views");
		GUILayout.Label("H - Toggle HDR");
		offscreenParticlesEnabled = GUILayout.Toggle(offscreenParticlesEnabled, "Offscreen Particles");
		if(offscreenParticlesEnabled)
			hdr = GUILayout.Toggle(hdr, "HDR");
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.O))
		{
			offscreenParticlesEnabled = !offscreenParticlesEnabled;
		}

		if (Input.GetKeyDown(KeyCode.V))
		{
			viewsCounter++;
			Transform view = viewPointsGroup.GetChild(viewsCounter % viewPointsGroup.childCount);
			m_camera.transform.position = view.position;
			if (adjustCameraRotation) m_camera.transform.rotation = view.rotation;
		}

		if (Input.GetKeyDown(KeyCode.H))
		{
			hdr = !hdr;
		}

		if (offscreenParticlesEnabled != lastOffscreenParticlesEnabled || hdr != lastHDR)
		{
			lastOffscreenParticlesEnabled = offscreenParticlesEnabled;
			lastHDR = hdr;
			ApplyOffscreenParticleStates();
		}
	}

	private void ApplyOffscreenParticleStates()
	{
		foreach (OffscreenGalaxyRenderer galaxyRenderer in m_camera.GetComponents<OffscreenGalaxyRenderer>())
		{
			galaxyRenderer.enabled = offscreenParticlesEnabled;
			galaxyRenderer.Hdr = hdr;
		}

		foreach (OffscreenGalaxyRenderer galaxyRenderer in m_camera.GetComponentsInChildren<OffscreenGalaxyRenderer>())
		{
			galaxyRenderer.enabled = offscreenParticlesEnabled;
			galaxyRenderer.Hdr = hdr;
		}
	}
}
