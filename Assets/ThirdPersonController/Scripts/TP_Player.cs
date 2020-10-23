using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TP_Player : MonoBehaviour
{
    protected Rigidbody _rb;

    #region Camera
    protected Camera _cam;
    protected CameraMovement _cm;
    protected Vector3 camFwd;
    #endregion

    #region Movement
    protected float horizontalInput;
    protected float verticalInput;


    [Range(1.0f, 10.0f)]
    public float walk_speed;
    [Range(1.0f, 10.0f)]
    public float backwards_walk_speed;
    [Range(1.0f, 10.0f)]
    public float strafe_speed;
    
    [Range(0.1f, 1.5f)]
    public float rotation_speed;

    [Range(2.0f, 10.0f)]
    public float jump_force;
    #endregion

    #region Animations
    protected MyTPCharacter tpc;
    protected bool walking = false;
    protected bool strafeLeft = false;
    protected bool strafeRight = false;
    protected bool backwards = false;
    protected bool jump = false;
    #endregion



    protected void Awake()
    {
        tpc = FindObjectOfType<MyTPCharacter>();
        _cm = GetComponent<CameraMovement>();
        _cam = _cm.GetCamera();
        _rb = GetComponent<Rigidbody>();
    }

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    protected void FixedUpdate()
    {
        // Gets the input
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        jump = Input.GetButtonDown("Jump");

        // Calculate camera relative directions to move:
        camFwd = Vector3.Scale(_cam.transform.forward, new Vector3(1, 1, 1)).normalized;
        Vector3 camFlatFwd = Vector3.Scale(_cam.transform.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 flatRight = new Vector3(_cam.transform.right.x, 0, _cam.transform.right.z);
        
        Vector3 m_CharForward = Vector3.Scale(camFlatFwd, new Vector3(1, 0, 1)).normalized;
        Vector3 m_CharRight = Vector3.Scale(flatRight, new Vector3(1, 0, 1)).normalized;


        // Draws a ray to show the direction the player is aiming at
        Debug.DrawLine(transform.position, transform.position + camFwd * 5f, Color.red);

        // Move the player (movement will be slightly different depending on the camera type)
        float w_speed;
        Vector3 move = Vector3.zero;
        if (_cm.type == CameraMovement.CAMERA_TYPE.FREE_LOOK)
        {
            w_speed = walk_speed;
            move = v * m_CharForward * w_speed + h * m_CharRight * walk_speed;
            _cam.transform.position += move * Time.deltaTime;

            // Rotate body
            tpc.transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(tpc.transform.forward, move, rotation_speed, 0.0f));
        }
        else if (_cm.type == CameraMovement.CAMERA_TYPE.LOCKED) {
            w_speed = (v > 0) ? walk_speed : backwards_walk_speed;
            move = v * m_CharForward * w_speed + h * m_CharRight * strafe_speed;
        }

        transform.position += move * Time.deltaTime;    // Move the actual player

        // Jump 
        if (jump) {
            _rb.AddForce(Vector3.up * jump_force, ForceMode.Impulse);
        }

        // Update animation flags
        if (_cm.type == CameraMovement.CAMERA_TYPE.FREE_LOOK)
        {
            walking = (h != 0 || v != 0);

        }
        else if (_cm.type == CameraMovement.CAMERA_TYPE.LOCKED) {
            walking = (v > 0 && h == 0);
            backwards = (v < 0 && h == 0);
            strafeLeft = (h < 0);
            strafeRight = (h > 0);
        }
    }

    // Update is called once per frame
    void Update()
    {
        PlayerStateMachine();

        // Animations
        tpc.GetFullBodyAnimator().SetBool("walking", walking);
        tpc.GetFullBodyAnimator().SetBool("strafeLeft", strafeLeft);
        tpc.GetFullBodyAnimator().SetBool("strafeRight", strafeRight);
        tpc.GetFullBodyAnimator().SetBool("backwards", backwards);
        tpc.GetFullBodyAnimator().SetBool("jump", jump);
    }

    protected void PlayerStateMachine() {


    }
}
