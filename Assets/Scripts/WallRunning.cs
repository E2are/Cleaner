using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using static PlayerMove;

public class WallRunning : MonoBehaviour
{
    [Header("WallRunning")]
    public LayerMask whatIsWall;
    public LayerMask whatIsGround;
    public float wallRunForce;
    public float wallJumpForce;
    public float wallSideJumpForce;
    public float maxWallRunTime;
    private float wallRunTimer;

    [Header("Input")]
    public KeyCode JumpKey = KeyCode.Space;
    private float horizontalInput;
    private float verticalInput;

    [Header("Detection")]
    public float wallCheckDistance;
    public float minJumpHeight;
    private RaycastHit leftWallHit;
    private RaycastHit rightWallHit;
    [HideInInspector] public bool wallLeft;
    [HideInInspector] public bool wallRight;

    [Header("Exiting")]
    private bool exitingWall;
    public float exitWallTime;
    private float exitWallTimer;

    [Header("Gravity")]
    public bool useGravity;
    public float gravityCounterForce; 

    [Header("References")]
    public Transform orientation;
    public PlayerCam cam;
    private PlayerMove pm;
    private audioPlayer AP;
    private Rigidbody rigid;

    private void Start()
    {
        rigid = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMove>();
        AP = GetComponentInChildren<audioPlayer>();
    }

    private void Update()
    {
        if (!GameManager.Instance.Paused)
        {
            CheckForWall();
            StateMachine();
        }
    }

    void CheckForWall()
    {
        wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallHit, wallCheckDistance, whatIsWall);
        wallLeft = Physics.Raycast(transform.position, orientation.right * -1, out leftWallHit, wallCheckDistance, whatIsWall);
    }

    bool AboveGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight,whatIsGround);
    }

    void StateMachine()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if((wallLeft || wallRight) && verticalInput > 0 && AboveGround()&&!exitingWall)
        {
            if (!pm.wallrunning)
                StartWallRunning();

            if (wallRunTimer > 0)
                wallRunTimer -= Time.deltaTime;

            if(wallRunTimer <= 0 && pm.wallrunning)
            {
                exitingWall = true;
                exitWallTimer = exitWallTime * 5;
            }

            if (Input.GetKeyDown(JumpKey))
            {
                wallJump();
            }
        }

        else if (exitingWall)
        {
            if (pm.wallrunning)
            {
                StopWallRunning();
            }
            if(exitWallTimer > 0) {
                exitWallTimer -= Time.deltaTime;
            }
            if(exitWallTimer <=0)
            {
                exitingWall = false;
            }
        }
        else
        {
            if (pm.wallrunning)
                StopWallRunning();
        }
    }

    private void FixedUpdate()
    {
        if(pm.wallrunning)
            WallRunningMovement();
    }

    void StartWallRunning()
    {
        pm.wallrunning = true;

        wallRunTimer = maxWallRunTime;

        AP.PlaySound(PlayerMove.MovementState.wallrunning);

        rigid.velocity = new Vector3(rigid.velocity.x, 0f, rigid.velocity.z);

        cam.DoFov(95f);
        if (wallLeft) cam.DoTilt(-10f);
        if (wallRight) cam.DoTilt(10f);
    }

    void WallRunningMovement()
    {
        rigid.useGravity = useGravity;

        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;

        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        if ((orientation.forward - wallForward).magnitude > (orientation.forward + wallForward).magnitude)
            wallForward = -wallForward;  

        rigid.AddForce(wallForward * wallRunForce, ForceMode.Force);
        if((wallLeft && horizontalInput > 0) ||(wallRight && horizontalInput < 0))
            rigid.AddForce(wallNormal * 100f, ForceMode.Force);
        else
            rigid.AddForce(-wallNormal * 100f, ForceMode.Force);

        if (useGravity)
        {
            rigid.AddForce(transform.up * gravityCounterForce, ForceMode.Force);
        }
    }

    void StopWallRunning()
    {
        pm.wallrunning = false;

        cam.DoFov(80f);
        cam.DoTilt(0f);
    }

    void wallJump()
    {
        AP.PlaySound(MovementState.air);

        exitingWall = true;
        exitWallTimer = exitWallTime;
        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;

        wallNormal = new Vector3(wallNormal.x, 0, wallNormal.z).normalized;

        Vector3 forceToApply = transform.up * wallJumpForce + wallNormal * wallSideJumpForce;

        rigid.velocity = new Vector3(rigid.velocity.x, 0f, rigid.velocity.z);

        rigid.AddForce(forceToApply,ForceMode.Impulse);
    }
}
