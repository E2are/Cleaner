using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

public class BathBombMist : MonoBehaviour
{
    IObjectPool<BathBombMist> managerPool;
    [HideInInspector]
    public float Dmg = 0;
    public float TickDamageDelay = 0.3f;
    float TickTimer = 0;
    RaycastHit hit;
    Vector3 HitNormal;
    public void SetManagerPool(IObjectPool<BathBombMist> pool)
    {
        managerPool = pool;
    }

    void OnEnable()
    {
        TickTimer = TickDamageDelay-TickDamageDelay/3f;
    }

    private void Update()
    {
        if(!GetComponent<ParticleSystem>().isPlaying)
        {
            managerPool.Release(this);
        }
        if(TickTimer > TickDamageDelay)
        {
            TickTimer = 0;
        }
        else
        {
            TickTimer += Time.deltaTime;
        }
        
    }

    private void OnTriggerStay(Collider other)
    {
        IDamageAbleProps hited = other.GetComponent<IDamageAbleProps>();
        if (hited != null && TickTimer >= TickDamageDelay)
        {
            hited.OnDamaged(Dmg, Vector3.zero);
        }
    }
}
