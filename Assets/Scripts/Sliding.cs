using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sliding : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform playerObj;
    private Rigidbody rigid;
    private PlayerMove pm;
    public PlayerCam cam;

    [Header("Sliding")]
    public float maxSlideTime;
    public float slideForce;
    private float slideTimer;
    public float slideDisableTime;
    private float slideDisableTimer = 0;

    [Header("Input")]
    public KeyCode slideKey = KeyCode.LeftControl;
    private float horizontalInput;
    private float verticalInput;

    private void Start()
    {
        rigid = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMove>();
    }

    private void Update()
    {
        if (!GameManager.Instance.Paused)
        {
            horizontalInput = Input.GetAxisRaw("Horizontal");
            verticalInput = Input.GetAxisRaw("Vertical");

            if (Input.GetKeyDown(slideKey) && (verticalInput > 0) && pm.sprinting && (pm.grounded || pm.OnSlope()) && slideDisableTimer <= 0)
                StartSlide();

            if (Input.GetKeyUp(slideKey) && pm.sliding)
                StopSlide();

            if (slideDisableTimer >= 0)
                slideDisableTimer -= Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        if (pm.sliding)
            SlidingMovement();
    }

    void StartSlide()
    {
        pm.sliding = true;

        slideDisableTimer = slideDisableTime;

        pm.AP.PlaySound(PlayerMove.MovementState.wallrunning);

        cam.DoFov(95f);

        slideTimer = maxSlideTime;
    }

    void SlidingMovement()
    {
        Vector3 inputDirection = (orientation.forward * verticalInput + orientation.right * horizontalInput / 2f);

        if (!pm.OnSlope() || rigid.velocity.y > -0.1f)
        {
            if(!pm.grounded)
            {
                rigid.AddForce(inputDirection.normalized * pm.airMultiplier, ForceMode.Force);
            }
            else
            {
                rigid.AddForce(inputDirection.normalized * slideForce, ForceMode.Force);
            }

            slideTimer -= Time.deltaTime;
        }
        else
        {
            rigid.AddForce(pm.GetSlopeMoveDirection(inputDirection) * slideForce, ForceMode.Force);
        }


        if (slideTimer < 0)
            StopSlide();
    }

    void StopSlide()
    {
        pm.sliding = false;

        cam.DoFov(80f);
    }
}
