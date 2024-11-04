using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class Prop : MonoBehaviour
{
    IObjectPool<Prop> pool;
    public float DetonateTime = 3f;
    float DetonateTimer = 0f;
    public bool WillToSetActive = true;

    private void Update()
    {
        if(DetonateTimer >= DetonateTime)
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
    }

    public void WillToRelease()
    {
        pool.Release(this);
    }

    public void WillToDestroy()
    {
        Destroy(this);
    }

    public void SetObjectPoolManager(IObjectPool<Prop> poolManager)
    {
        pool = poolManager;
    }
}
