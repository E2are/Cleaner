using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class BubbleGun : MonoBehaviour
{
    [Header("References")]
    public Camera PlayerCam;
    public GameObject WeaponHolder;
    public Animator BubbleGunanim;
    AudioSource AS;
    Ray shootRay = new Ray();
    RaycastHit shootHit = new RaycastHit();
    [Header("Stats")]
    public float Damage = 1;
    public float RapidShootSpeed = 0.1f;
    public int shotAmount = 3;
    public bool shootAble = true;
    public bool reloading = false;
    public Vector3 ReCoilKickBack = new Vector3(0.2f,0.3f,0);
    public float ShakeAmount = 0.5f;
    [Header("Resources")]
    public GameObject crosshair;
    public GameObject HitEffect;
    public LineRenderer ShotPath;
    public GameObject FlashLight;
    bool lightOn = true;
    public AudioClip ShootSound;
    public AudioClip[] WaterSound;
    public AudioClip HitSound;
    public ObjectPool<Effect> WaterSoundPool;
    [Header("InputKeys")]
    public KeyCode ShootKey = KeyCode.Mouse0;
    public KeyCode ZoomKey = KeyCode.Mouse1;
    public KeyCode ReloadKey = KeyCode.R;
    public KeyCode LightKey = KeyCode.F;

    private void Start()
    {
        AS = GameObject.Find("WeaponHolder").GetComponent<AudioSource>();

        WaterSoundPool = new ObjectPool<Effect>(CreateBullet, OnGetBullet, OnReleaseBullet, OnDestroyBullet, true, 10, 100);

        ShotPath = GetComponentInChildren<LineRenderer>();

        ShotPath.positionCount = 2;

        ShotPath.enabled = false;
    }

    private void Update()
    {
        if (!GameManager.Instance.Paused && !GameManager.Instance.IsCinemachining)
        {
            PlayerCam = GetComponent<Camera>();

            shootRay = new Ray(transform.position, transform.forward);

            Debug.DrawRay(transform.position, transform.forward);

            RecoilRecover();
            if (GameManager.Instance.BubblegunHolded)
            {
                Shoot();

                ShotPath.SetPosition(0, ShotPath.transform.position);

                Reload();
            }
            LightTurn();

            if (Input.GetKeyDown(ZoomKey))
            {
                PlayerCam.GetComponent<PlayerCam>().DoFov(60f);
            }
            else if (Input.GetKeyUp(ZoomKey) || Input.GetKeyDown(ShootKey) && Input.GetKey(ZoomKey))
            {
                PlayerCam.GetComponent<PlayerCam>().DoFov(80f);
            }
        }
    }

    void Shoot()
    {
        if (Input.GetKeyDown(ShootKey) && shootAble&&!reloading && BubbleGunanim.GetBool("WeaponIsHolded"))
        {
            StartCoroutine(ShootSeq());
        }
    }

    void Reload()
    {
        if(Input.GetKeyDown(ReloadKey) && GameManager.Instance.RemainedCartridgeCount > 0 && !reloading)
        {
            StopAllCoroutines();
            BubbleGunanim.SetTrigger("Reload");
            reloading = true;
        }
    }

    IEnumerator ShootSeq()
    {
        shootAble = false;
        BubbleGunanim.SetTrigger("Shoot");
        for (int i = 0; i < shotAmount; i++)
        {
            crosshair.SetActive(false);
            shootCheck();
            AS.pitch = 0.9f + (1-GameManager.Instance.RemainAmmo_Amount / GameManager.Instance.maxAmmo_Amount);
            PlaySound("shoot");
            
            if (GameManager.Instance.RemainAmmo_Amount <= 0)
                break;
            else
                GameManager.Instance.RemainAmmo_Amount -= 1;

            yield return new WaitForSeconds(RapidShootSpeed*2/3);
            ShotPath.enabled = false;
            yield return new WaitForSeconds(RapidShootSpeed/3);
            crosshair.SetActive(false);
        }
        yield return new WaitForSeconds(RapidShootSpeed * 4 );
        shootAble = true;
    }

    public void shootCheck()
    {
        if (GameManager.Instance.RemainAmmo_Amount > 0)
        {
            ShotPath.enabled = true;
            if (Physics.Raycast(shootRay, out shootHit))
            {
                IDamageAbleProps hitedProp = shootHit.collider.GetComponentInParent<IDamageAbleProps>();
                if (hitedProp != null)
                {
                    crosshair.SetActive(true);
                    hitedProp.OnDamaged(Damage, shootHit.normal);
                }
                Rigidbody hitedrigid = shootHit.collider.GetComponent<Rigidbody>();
                if (hitedrigid != null)
                {
                    hitedrigid.AddForce((PlayerCam.transform.forward - shootHit.normal).normalized * Damage, ForceMode.Impulse);
                }
                Effect Eft = WaterSoundPool.Get();
                Eft.transform.position = shootHit.point;
                Eft.transform.localScale = new Vector3(Random.Range(0.1f, 0.2f), Random.Range(0.1f, 0.2f), 1);
                Eft.transform.forward = shootHit.normal;
                Eft.SetObjectPoolManager(WaterSoundPool);
                Eft.GetComponent<AudioSource>().spatialBlend = 1f;
                Eft.GetComponent<AudioSource>().clip = WaterSound[Random.Range(0, WaterSound.Length)];
                Eft.GetComponent<AudioSource>().Play();

                if (shootHit.collider != null)
                {
                    ShotPath.SetPosition(1, shootHit.point);
                }
                else
                {
                    ShotPath.SetPosition(1, ShotPath.transform.position + new Vector3(ShotPath.transform.position.x - 0.5f, ShotPath.transform.position.y + 0.2f, 3));
                }
            }

            Recoil();
        }
    }

    public void Recoil(float amount = 1)
    {
        Vector3 kickRotation = new Vector3(-Random.Range(0, ReCoilKickBack.y) *200f, Random.Range(-ReCoilKickBack.x, ReCoilKickBack.x) * 200f, ReCoilKickBack.z) * amount;

        WeaponHolder.transform.localRotation = Quaternion.Slerp(WeaponHolder.transform.localRotation, Quaternion.Euler(WeaponHolder.transform.localEulerAngles + kickRotation), ShakeAmount);
    }

    void RecoilRecover()
    {
        WeaponHolder.transform.localRotation = Quaternion.Slerp(WeaponHolder.transform.localRotation, Quaternion.identity, Time.deltaTime * 2f);
        WeaponHolder.transform.localPosition = Vector3.Lerp(WeaponHolder.transform.localPosition, Vector3.zero, Time.deltaTime * 2f);
    }

    void LightTurn()
    {
        if (Input.GetKeyDown(LightKey))
        {
            lightOn = !lightOn;
            FlashLight.SetActive(lightOn);
        }
    }

    void PlaySound(string sound)
    {
        switch(sound)
        {
            case "shoot":
                AS.clip = ShootSound; 
                break;
        }
        AS.Play();
    }
    private Effect CreateBullet()
    {
        Effect bullet = Instantiate(HitEffect).GetComponent<Effect>();
        bullet.SetObjectPoolManager(WaterSoundPool);
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