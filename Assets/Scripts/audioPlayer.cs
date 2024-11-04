using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class audioPlayer : MonoBehaviour
{
    AudioSource audioSource;
    public PlayerMove PM;

    public float playSoundTime;
    private float playSoundTimer = 0;
    Ray ray = new Ray();
    RaycastHit whatisGroundRay;
    public LayerMask water;
    public LayerMask ground;
    public AudioClip[] walkingClips;
    public AudioClip[] water_walkingClips;
    public AudioClip[] runningClips;
    public AudioClip[] water_runningClips;
    public AudioClip wallRunningClip;
    public AudioClip waterSlidingClip;
    public AudioClip[] jumpClips;
    public AudioClip[] water_jumpClips;


    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        PM = GetComponentInParent<PlayerMove>();

        ray = new Ray(transform.position, -transform.up);
    }

    private void Update()
    {
        if (!GameManager.Instance.Paused)
        {
            float horizontalInput = Input.GetAxisRaw("Horizontal");
            float verticalInput = Input.GetAxisRaw("Vertical");

            if ((horizontalInput != 0 || verticalInput != 0) && (PM.state == PlayerMove.MovementState.walking || PM.state == PlayerMove.MovementState.crouching) && PM.grounded)
            {
                if (playSoundTimer > 0)
                {
                    playSoundTimer -= Time.deltaTime;
                }
                else
                {
                    PlaySound(PM.state);
                    playSoundTimer = playSoundTime;
                }
            }
            else if ((horizontalInput != 0 || verticalInput != 0) && PM.state == PlayerMove.MovementState.sprinting)
            {
                if (playSoundTimer > 0)
                {
                    playSoundTimer -= Time.deltaTime;
                }
                else
                {
                    PlaySound(PM.state);
                    playSoundTimer = playSoundTime / 1.4f;
                }
            }
            else if (horizontalInput == 0 && verticalInput == 0)
            {
                playSoundTimer = 0.1f;
            }
        }
    }

    public void PlaySound(PlayerMove.MovementState type)
    {
        bool waterhit = Physics.Raycast(transform.position, Vector3.down, GetComponent<CapsuleCollider>().height *0.7f, water);

        switch (type) {
            case PlayerMove.MovementState.crouching:
            case PlayerMove.MovementState.walking:
                if (!waterhit)
                    audioSource.PlayOneShot(walkingClips[Random.Range(0, walkingClips.Length)]);
                if (waterhit)
                    audioSource.PlayOneShot(water_walkingClips[Random.Range(0, water_walkingClips.Length)]);
                break;
            case PlayerMove.MovementState.sprinting:
                if (!waterhit)
                    audioSource.PlayOneShot(runningClips[Random.Range(0, walkingClips.Length)]);
                if(waterhit)
                    audioSource.PlayOneShot(water_walkingClips[Random.Range(0, water_walkingClips.Length)]);
                break;
            case PlayerMove.MovementState.wallrunning:
                if (!waterhit)
                {
                    audioSource.clip = wallRunningClip;
                    audioSource.Play();
                }
                if (waterhit)
                    audioSource.PlayOneShot(waterSlidingClip);
                break;
            case PlayerMove.MovementState.air:
                if(!waterhit)
                    audioSource.clip = (jumpClips[Random.Range(0, jumpClips.Length)]);
                    audioSource.Play();
                if (waterhit)
                    audioSource.PlayOneShot(water_jumpClips[Random.Range(0, water_jumpClips.Length)]);
                break;
        }
    }
}
