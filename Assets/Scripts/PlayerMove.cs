using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour, IDamageAbleProps
{
    [Header("MoveMent")]
    private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float slideSpeed;
    public float wallRunningSpeed;

    float desiredMoveSpeed;
    float lastDesiredMoveSpeed;

    public float speedIncreaseMultiplier;
    public float slopeIncreaseMultiplier;

    public float groundDrag;

    bool Invincible = false;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump = true;

    [Header("Crounching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;
    public KeyCode pauseKey = KeyCode.Escape;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    public bool grounded;

    [Header("Slope Handler")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    [Header("Ref")]
    public audioPlayer AP;
    public Transform orientation;
    public PlayerCam cam;
    public GameObject WaterSplashPrefab;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    [HideInInspector]
    public Rigidbody rigid;

    public MovementState state;
    public enum MovementState
    {
        walking,
        sprinting,
        wallrunning,
        crouching,
        sliding,
        air
    }

    public bool sprinting;
    public bool sliding;
    public bool crouching;
    public bool wallrunning;

    private void Awake()
    {

    }
    private void Start()
    {
        rigid = GetComponent<Rigidbody>();
        rigid.freezeRotation = true;

        AP = GetComponentInChildren<audioPlayer>();

        startYScale = transform.localScale.y;

        transform.position = GameManager.Instance.SavePosition.Points[GameManager.Instance.SavePosition.current_SavePoint_Index];
    }

    private void Update()
    {
        if (!GameManager.Instance.Dead)
        {
            if (!GameManager.Instance.Paused)
            {
                if (!GameManager.Instance.IsCinemachining)
                {
                    grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

                    MyInput();
                    SpeedControl();
                    StateHandler();

                    if (grounded)
                        rigid.drag = groundDrag;
                    else
                        rigid.drag = 0;
                }
            }
        }
        else
        {
            GetComponentInChildren<CapsuleCollider>().height = crouchYScale;
            rigid.drag = 1;
        }
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(jumpKey) && readyToJump && (grounded || OnSlope()))
        {
            readyToJump = false;
            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        if (Input.GetKeyDown(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rigid.AddForce(Vector3.down * 5f, ForceMode.Impulse);
            crouching = true;
        }

        Ray ray = new Ray(orientation.position, transform.up);
        RaycastHit hit = new RaycastHit();

        if (!Input.GetKey(crouchKey) && crouching){
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider != null)
                {
                    if (hit.distance >= startYScale)
                    {
                        transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
                        crouching = false;
                    }
                    else
                    {
                        transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
                        crouching = true;
                    }
                }
            }
            else
            {
                transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
                crouching = false;
            }
        }

        if (Input.GetKeyDown(sprintKey))
        {
            sprinting = true;
        }
        if (Input.GetKeyUp(sprintKey))
        {
            sprinting = false;
        }

        if (Vector3.Magnitude(new Vector3(rigid.velocity.x, 0, rigid.velocity.z)) > 8.5f)
        {
            UIManager.Instance.BG.BubbleGunanim.SetBool("Running", true);
        }
        else
        {
            UIManager.Instance.BG.BubbleGunanim.SetBool("Running", false);
        }
    }

    private void StateHandler()
    {
        if (wallrunning)
        {
            state = MovementState.wallrunning;
            AP.GetComponent<AudioSource>().volume = 1f;
            desiredMoveSpeed = wallRunningSpeed;
        }

        else if (sliding)
        {
            state = MovementState.sliding;
            AP.GetComponent<AudioSource>().volume = 1f;
            if (OnSlope() && rigid.velocity.y <= 0.1f)
            {
                desiredMoveSpeed = slideSpeed;
                rigid.AddForce(Vector3.down * 2f, ForceMode.Force);
                rigid.useGravity = true;
            }
            else
            {
                desiredMoveSpeed = sprintSpeed + 1f;
            }
        }

        else if (crouching)
        {
            state = MovementState.crouching;
            AP.GetComponent<AudioSource>().volume = 0.5f;
            desiredMoveSpeed = crouchSpeed;
        }

        else if (grounded && Input.GetKey(sprintKey))
        {
            state = MovementState.sprinting;
            AP.GetComponent<AudioSource>().volume = 1f;
            desiredMoveSpeed = sprintSpeed;
        }

        else if (grounded)
        {
            state = MovementState.walking;
            AP.GetComponent<AudioSource>().volume = 1f;
            desiredMoveSpeed = walkSpeed;
        }

        else
        {
            AP.GetComponent<AudioSource>().volume = 1f;
            state = MovementState.air;
        }

        if(Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed) > 4f && moveSpeed != 0)
        {
            StopAllCoroutines();
            StartCoroutine(SmoothlyLerpMoveSpeed());
        }
        else
        {
            moveSpeed = desiredMoveSpeed;
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;
    }

    IEnumerator SmoothlyLerpMoveSpeed()
    {
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        while(time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);
            if (OnSlope())
            {
                float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                float slopeAngleIncrease = 1 + (slopeAngle / 90f);
                time += Time.deltaTime * speedIncreaseMultiplier * slopeIncreaseMultiplier * slopeAngleIncrease;
            }
            else
                time += Time.deltaTime * speedIncreaseMultiplier;
            
            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
    }

    void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (OnSlope() && !exitingSlope)
        {
            rigid.AddForce(GetSlopeMoveDirection(moveDirection) * moveSpeed * 20f, ForceMode.Force);

            if(rigid.velocity.y >= 0)
            {
                rigid.AddForce(Vector3.down * 80f,ForceMode.Force);
            }
        }

        if (grounded)
        {
            rigid.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
            cam.DoTilt(-horizontalInput * 2);
        }

        else if (!grounded)
        {
            rigid.AddForce(moveDirection.normalized * moveSpeed * airMultiplier, ForceMode.Force);
        }

        if (!wallrunning) { 
            rigid.useGravity = !OnSlope();
        }
    }

    void SpeedControl()
    {
        if (OnSlope() && !exitingSlope)
        {
            if(rigid.velocity.magnitude > moveSpeed)
            {
                rigid.velocity = rigid.velocity.normalized * moveSpeed;
            }
        }
        else
        {
            Vector3 flatVel = new Vector3(rigid.velocity.x, 0f, rigid.velocity.z);

            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rigid.velocity = new Vector3(limitedVel.x, rigid.velocity.y, limitedVel.z);
            }
        }
    }

    void Jump()
    {
        AP.PlaySound(MovementState.air);

        exitingSlope = true;

        rigid.velocity = new Vector3(rigid.velocity.x,0f,rigid.velocity.z);

        rigid.AddForce(transform.up * jumpForce, ForceMode.Impulse);

        Ray ray = new Ray(transform.position, Vector3.down);
        RaycastHit hit = new RaycastHit();

        if(Physics.Raycast(ray, out hit))
        {
            if(hit.collider != null)
            {
                if(hit.collider.CompareTag("Water") && hit.distance <= playerHeight * 0.5f + 0.2f)
                {
                    GameObject Splash = Instantiate(WaterSplashPrefab, hit.point,Quaternion.identity);
                    Splash.transform.up = hit.normal;
                }
            }
        }
    }

    void ResetJump()
    {
        readyToJump = true;

        exitingSlope = false;
    }

    public bool OnSlope()
    {
        if(Physics.Raycast(transform.position,Vector3.down,out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }
        return false;
    }
    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized; 
    }

    public void OnDamaged(float dmg, Vector3 hitnNormal)
    {
        if (!Invincible)
        {
            if (!GameManager.Instance.Dead)
            {
                GameManager.Instance.HP -= dmg;
                if (GameManager.Instance.HP > 0)
                {
                    GameManager.Instance.hitAlpha = Random.Range(0.2f, dmg / GameManager.Instance.MaxHP);
                    GameManager.Instance.BGun.Recoil(dmg);
                }
                else
                {
                    GameManager.Instance.Dead = true;
                    GameManager.Instance.BGun.BubbleGunanim.SetTrigger("Dead");
                    StartCoroutine(DeathSeq());
                    rigid.AddForce(-orientation.forward * 3 + Vector3.up * jumpForce, ForceMode.Impulse);
                }
            }
        }
    }

    IEnumerator DeathSeq()
    {
        BGMManager.Instance.ChangeMusic("GameOver");
        AP.GetComponent<AudioSource>().PlayOneShot(AP.DeathClips[Random.Range(0,AP.DeathClips.Length)]);
        yield return new WaitForSeconds(3f);
        UIManager.Instance.GameOverAnim.gameObject.SetActive(true);
        UIManager.Instance.GameOverAnim.SetTrigger("Reveal");
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
