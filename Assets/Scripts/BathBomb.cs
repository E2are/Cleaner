using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Pool;
using TMPro;

public class BathBomb : MonoBehaviour
{
    [Header("References")]
    public Camera PlayerCam;
    public GameObject WeaponHolder;
    public Animator BombAnim;
    public GameObject bombStats;
    AudioSource AS;
    [Header("Stats")]
    public float dmg = 10f;
    public float AttackDelay = 10f;
    float ChargingTimer = 0;
    public int stackedBomb = 0;
    public int LimitOfStackableBombs;
    float DelayTimer = 0;
    public float ThrowForce = 7f;
    public float TickDamageDelay = 0.3f;
    public float MistRemainTime = 5f;
    [Header("Resources")]
    public Transform ThrowPos;
    public GameObject BathBombPrefab;
    public ObjectPool<BathBombPrefab> BombPool;
    public ObjectPool<BathBombMist> MistPool;
    public GameObject MistPrefab;
    [Header("Input")]
    public KeyCode ThrowKey = KeyCode.E;

    // Update is called once per frame
    void Update()
    {
        BombPool = new ObjectPool<BathBombPrefab>(CreateBullet, OnGetBullet, OnReleaseBullet, OnDestroyBullet, true, 5, 100);
        MistPool = new ObjectPool<BathBombMist>(CreateMist, OnGetMist, OnReleaseMist, OnDestroyMist, true, 10, 10);
        Throw();
    }

    void Throw()
    {
        if (Input.GetKeyDown(ThrowKey) && DelayTimer > AttackDelay&& stackedBomb > 0 && !GameManager.Instance.IsCinemachining && !GameManager.Instance.Paused)
        {
            stackedBomb--;
            BombAnim.SetTrigger("Throwing");
            DelayTimer = 0;
        }
        else
        {
            DelayTimer += Time.deltaTime;
        }

        if(stackedBomb < LimitOfStackableBombs && ChargingTimer > 3f)
        {
            stackedBomb++;
            ChargingTimer = 0f;
        }
        else if(stackedBomb == LimitOfStackableBombs)
        {
            ChargingTimer = 0f;
            bombStats.GetComponent<Image>().fillAmount = 1;
        }
        else
        {
            ChargingTimer += Time.deltaTime;
            bombStats.GetComponent<Image>().fillAmount = ChargingTimer / 3f;
        }

        bombStats.GetComponentInChildren<TMP_Text>().text = "X " + stackedBomb;
    }

    public void Throwing()
    {
        StartCoroutine (ThrowSequence());
    }

    IEnumerator ThrowSequence()
    {
        yield return null;
        BathBombPrefab Bomb = BombPool.Get();
        Bomb.transform.position = ThrowPos.position;
        Bomb.GetComponent<BathBombPrefab>().IsMistBomb = true;
        Bomb.GetComponent<Rigidbody>().velocity = GameManager.Instance.PM.rigid.velocity;
        Bomb.GetComponent<Rigidbody>().AddForce(PlayerCam.transform.forward * ThrowForce + Vector3.up * ThrowForce/3f, ForceMode.Impulse);
        Bomb.SetParentScript(this);
        Bomb.Dmg = dmg;
        Bomb.TickDamageDelay = TickDamageDelay;
    }

    private BathBombPrefab CreateBullet()
    {
        BathBombPrefab bomb = Instantiate(BathBombPrefab).GetComponent<BathBombPrefab>();
        bomb.SetManagerPool(BombPool);
        return bomb;
    }

    void OnGetBullet(BathBombPrefab bomb)
    {
        bomb.gameObject.SetActive(true);
    }

    void OnReleaseBullet(BathBombPrefab bomb)
    {
        bomb.gameObject.SetActive(false);
    }

    void OnDestroyBullet(BathBombPrefab bomb)
    {
        Destroy(bomb.gameObject);
    }

    private BathBombMist CreateMist()
    {
        BathBombMist bomb = Instantiate(MistPrefab).GetComponent<BathBombMist>();
        bomb.SetManagerPool(MistPool);
        return bomb;
    }

    void OnGetMist(BathBombMist bomb)
    {
        bomb.gameObject.SetActive(true);
    }

    void OnReleaseMist(BathBombMist bomb)
    {
        bomb.gameObject.SetActive(false);
    }

    void OnDestroyMist(BathBombMist bomb)
    {
        Destroy(bomb.gameObject);
    }
}
