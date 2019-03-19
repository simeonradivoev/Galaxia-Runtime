using UnityEngine;
using System.Collections;

public class GalaxyCam : MonoBehaviour {

    public float MaxZoom = 40;
    public float MinZoom = 5;
    [Range(0,1)]
    public float Zoom = 1;
    public float Sensitivity = 1.5f;
    public float FastSensitivityMultiplyer = 3;
    public float ZoomSensitivity = 0.65f;
    public float ZoomSmoothing = 2f;
    [Galaxia.CurveRange(0,0,1,1)]
    public AnimationCurve ZoomRotation = AnimationCurve.Linear(0,1,1,0.8f);
    [Galaxia.CurveRange(0, 0, 1, 1)]
    public AnimationCurve ZoomMoveSensitivity = AnimationCurve.Linear(0, 0.3f, 1, 1);
    public float Rotation = 50;

	// Use this for initialization
	void Start () 
    {
	
	}
	
	// Update is called once per frame
	void Update () 
    {

        Zoom -= Input.GetAxis("Mouse ScrollWheel") * ZoomSensitivity;
        Zoom = Mathf.Clamp01(Zoom);

	    if(Input.GetMouseButton(1))
        {
            float Sensitivity = this.Sensitivity;
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                Sensitivity *= FastSensitivityMultiplyer;

            transform.position -= new Vector3(Input.GetAxis("Mouse X"), 0, Input.GetAxis("Mouse Y")) * Sensitivity * ZoomMoveSensitivity.Evaluate(Zoom);
        }

        transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x, (Zoom * (MaxZoom - MinZoom)) + MinZoom, transform.position.z), ZoomSmoothing * Time.deltaTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.AngleAxis(Rotation * ZoomRotation.Evaluate(Zoom), transform.right), ZoomSmoothing * Time.deltaTime);
	}
}
