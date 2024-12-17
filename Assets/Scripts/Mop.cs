using RayFire;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

public class Mop : MonoBehaviour
{
    [Header("References")]
    public Camera PlayerCam;
    public GameObject WeaponHolder;
    public Animator MopAnim;
    AudioSource AS;
    Ray shootRay = new Ray();
    RaycastHit shootHit = new RaycastHit();
    [Header("Stats")]
    public float Damage = 3f;
    public float Range = 1.5f;
    public float AttackDelay = 0.8f;
    float AttackTimer = 0;
    public Vector3 ReCoilKickBack = new Vector3(0.2f, 0.3f, 0);
    public float ShakeAmount = 1;
    [Header("Resources")]
    public GameObject crosshair;
    public GameObject HitEffect;
    public AudioClip HitSound;
    public AudioClip[] SwingSound;
    public AudioClip[] WaterSound;
    public ObjectPool<Effect> WaterSplashPool;
    [Header("Inputs")]
    public KeyCode AttackKey = KeyCode.Mouse0;
    void Start()
    {
        AS = GameObject.Find("WeaponHolder").GetComponent<AudioSource>();

        WaterSplashPool = new ObjectPool<Effect>(CreateBullet, OnGetBullet, OnReleaseBullet, OnDestroyBullet, true, 10, 100);
    }

    void Update()
    {
        if (!GameManager.Instance.Paused && !GameManager.Instance.IsCinemachining && !GameManager.Instance.Dead)
        {
            Vector3 ShootPoint = transform.position;
            shootRay = new Ray(transform.position, transform.forward);

            if (!GameManager.Instance.BubblegunHolded)
            {
                Attack();
            }
        }
    }

    void Attack()
    {
        if (Input.GetKeyDown(AttackKey) && MopAnim.GetBool("WeaponIsHolded") && AttackDelay < AttackTimer)
        {
            AttackStart();
            AttackTimer = 0;
        }
        else
        {
            AttackTimer += Time.deltaTime;
        }
    }

    public void AttackStart()
    {
        AS.pitch = 1.3f;
        PlaySound("swing");
        Recoil();
        MopAnim.SetTrigger("Shoot");
        if (Physics.Raycast(shootRay,out shootHit))
        {
            if (shootHit.collider.CompareTag("Player")) return;
            if (shootHit.distance <= Range)
            {
                IDamageAbleProps hitedObject = shootHit.collider.GetComponentInParent<IDamageAbleProps>();
                if (hitedObject != null)
                {
                    crosshair.SetActive(true);
                    hitedObject.OnDamaged(Damage, shootHit.normal);
                }

                Rigidbody hitedrigid = shootHit.collider.GetComponent<Rigidbody>();
                if (hitedrigid != null)
                {
                    hitedrigid.AddForce((PlayerCam.transform.forward - shootHit.normal).normalized * Damage, ForceMode.Impulse);
                }

                if (shootHit.collider.gameObject.layer != LayerMask.GetMask("Bomb"))
                {
                    Effect Eft = WaterSplashPool.Get();
                    Eft.transform.position = shootHit.point;
                    Eft.transform.localScale = new Vector3(1, 1, 1) * Random.Range(0.5f, 1);
                    Eft.transform.up = shootHit.normal;
                    Eft.SetObjectPoolManager(WaterSplashPool);
                    Eft.GetComponent<RayfireBomb>().Explode(0.1f);
                    Eft.GetComponent<AudioSource>().clip = WaterSound[Random.Range(0, WaterSound.Length)];
                    Eft.GetComponent<AudioSource>().Play();
                }
            }
        }
        crosshair.SetActive(false);
    }

    void Recoil()
    {
        Vector3 kickRotation = new Vector3(-Random.Range(0, ReCoilKickBack.y) * 200f, Random.Range(-ReCoilKickBack.x, 0) * 200f, ReCoilKickBack.z);
        Vector3 kickVector = new Vector3(0,0, ReCoilKickBack.z);

        WeaponHolder.transform.localRotation = Quaternion.Slerp(WeaponHolder.transform.localRotation, Quaternion.Euler(WeaponHolder.transform.localEulerAngles + kickRotation), ShakeAmount);
        WeaponHolder.transform.localPosition = Vector3.Lerp(WeaponHolder.transform.localPosition, WeaponHolder.transform.localPosition + kickVector, ShakeAmount);
    }

    public void PlaySound(string soundName)
    {
        switch (soundName)
        {
            case "swing":
                AS.clip = SwingSound[Random.Range(0,SwingSound.Length)];
                break;
        }
        AS.Play();
    }

    private Effect CreateBullet()
    {
        Effect bullet = Instantiate(HitEffect).GetComponent<Effect>();
        bullet.SetObjectPoolManager(WaterSplashPool);
        return bullet;
    }

    void OnGetBullet(Effect bullet)
    {
        bullet.gameObject.SetActive(true);
    }

    void OnReleaseBullet(Effect bullet)
    {
        bullet.gameObject.SetActive(false);
    }

    void OnDestroyBullet(Effect bullet)
    {
        Destroy(bullet.gameObject);
    }
}
