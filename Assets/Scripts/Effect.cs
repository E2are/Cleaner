using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class Effect : MonoBehaviour
{
    ParticleSystem PS;
    IObjectPool<Effect> pool;
    public float DetonateTime = 3f;
    float DetonateTimer = 0f;
    public bool WillToSetActive = true;
    public bool LookAtTarget = false;

    private void Awake()
    {
        if(GetComponent<ParticleSystem>() != null)
        PS = GetComponent<ParticleSystem>();
    }
    private void OnEnable()
    {
        if(PS != null)
        PS.Play();
    }
    private void Update()
    {
        if (DetonateTimer >= DetonateTime)
        {
            if (WillToSetActive)
            {
                WillToRelease();
            }
            else
            {
                WillToDestroy();
            }
        }
        else
        {
            DetonateTimer += Time.deltaTime;
        }

        if (LookAtTarget && GameManager.Instance != null)
        {
            transform.forward = GameManager.Instance.BGun.PlayerCam.transform.forward;
        }
    }

    public void WillToRelease()
    {
        pool.Release(this);
        DetonateTimer = 0;
    }

    public void WillToDestroy()
    {
        Destroy(this.gameObject);
    }

    public void SetObjectPoolManager(IObjectPool<Effect> poolManager)
    {
        pool = poolManager;
    }
}
