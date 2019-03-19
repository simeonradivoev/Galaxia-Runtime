using UnityEngine;
using System.Collections;

public class FlyCamera : MonoBehaviour
{

    /*
    EXTENDED FLYCAM
        Desi Quintans (CowfaceGames.com), 17 August 2012.
        Based on FlyThrough.js by Slin (http://wiki.unity3d.com/index.php/FlyThrough), 17 May 2011.
 
    LICENSE
        Free as in speech, and free as in beer.
 
    FEATURES
        WASD/Arrows:    Movement
                  Q:    Climb
                  E:    Drop
                      Shift:    Move faster
                    Control:    Move slower
                        End:    Toggle cursor locking to screen (you can also press Ctrl+P to toggle play mode on and off).
    */

    public float cameraSensitivity = 1;
    public float climbSpeed = 4;
    public float normalMoveSpeed = 2f;
    public float slowMoveFactor = 0.25f;
    public float fastMoveFactor = 3;

    public float rotationX = 0.0f;
    public float rotationY = 0.0f;

    public float OrbitSpeed = 1;
    public float Zoom = 10;
    public float ZoomSpeed = 10;

    private Vector3 lastFlyPos;


    void Start()
    {
        //Screen.lockCursor = true;
    }

    void Update()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            rotationX += Input.GetAxis("Mouse X") * cameraSensitivity;
            rotationY += Input.GetAxis("Mouse Y") * cameraSensitivity;
            rotationY = Mathf.Clamp(rotationY, -90, 90);

            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                transform.position += transform.forward * (normalMoveSpeed * fastMoveFactor) * Input.GetAxis("Vertical") * Time.deltaTime;
                transform.position += transform.right * (normalMoveSpeed * fastMoveFactor) * Input.GetAxis("Horizontal") * Time.deltaTime;
            }
            else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                transform.position += transform.forward * (normalMoveSpeed * slowMoveFactor) * Input.GetAxis("Vertical") * Time.deltaTime;
                transform.position += transform.right * (normalMoveSpeed * slowMoveFactor) * Input.GetAxis("Horizontal") * Time.deltaTime;
            }
            else
            {
                transform.position += transform.forward * normalMoveSpeed * Input.GetAxis("Vertical") * Time.deltaTime;
                transform.position += transform.right * normalMoveSpeed * Input.GetAxis("Horizontal") * Time.deltaTime;
            }
        

            if (Input.GetKey(KeyCode.Q)) { transform.position += transform.up * climbSpeed * Time.deltaTime; }
            if (Input.GetKey(KeyCode.E)) { transform.position -= transform.up * climbSpeed * Time.deltaTime; }

            Zoom = transform.position.magnitude;
        }
        else
        {
            if (Input.GetAxis("Mouse ScrollWheel") != 0)
            {
                Zoom = Mathf.Max(0, Zoom + Input.GetAxis("Mouse ScrollWheel") * ZoomSpeed);
            }

            //transform.position = new Vector3(Mathf.Sin(Time.time * OrbitSpeed), 0, Mathf.Cos(Time.time * OrbitSpeed)) * Zoom;
            //transform.LookAt(Vector3.zero);

            
            transform.position = -transform.forward * Zoom;

            if(Input.GetMouseButton(0))
            {
                rotationX += Input.GetAxis("Mouse X") * cameraSensitivity;
                rotationY += Input.GetAxis("Mouse Y") * cameraSensitivity;
            }
            else
            {
                rotationX += OrbitSpeed * Time.deltaTime;
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
            }else
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }

    void FixedUpdate()
    {
        transform.rotation = Quaternion.AngleAxis(rotationX, Vector3.up);
        transform.rotation *= Quaternion.AngleAxis(rotationY, Vector3.left);
    }

    void OnGUI()
    {

        GUIStyle s = new GUIStyle(GUI.skin.label);
        s.alignment = TextAnchor.MiddleCenter;
        GUI.color = new Color(1,1,1,0.4f);

        if (Cursor.lockState == CursorLockMode.Locked)
        {
            GUI.Label(new Rect(Screen.width / 2 - 256, Screen.height - 37, 512, 32), "'Space' - switch orbit camera.",s);
        }
        else
        {
            GUI.Label(new Rect(0, Screen.height - 37, Screen.width, 32), "'Space' - switch free fly camera. \n Click and Drag to rotate view", s);
        }

    }
}