using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class BathBombPrefab : MonoBehaviour
{
    IObjectPool<BathBombPrefab> managerPool;
    BathBomb Bomb;
    public float DetonateTime = 3f;
    [HideInInspector]
    public float Dmg = 0;
    float DTimer = 0;
    public float TickDamageDelay = 0.3f;
    
    [Header("Type")]
    public bool IsMistBomb;
    public Transform Target;
    public GameObject Appearence;
    [Header("Effect")]
    public GameObject MistPrefab;
    public void SetManagerPool(IObjectPool<BathBombPrefab> pool)
    {
        managerPool = pool;
    }

    public void SetParentScript(BathBomb bomb)
    {
        Bomb = bomb;
    }

    private void Update()
    {
        DTimer += Time.deltaTime;    
        if(Target!= null)
        {
            Appearence.transform.forward = Target.forward;
        }
    }

    void DetonateBomb()
    {
        if (managerPool != null)
        {
            BathBombMist Mist = Bomb.MistPool.Get();
            Mist.transform.position = transform.position;
            Mist.TickDamageDelay = TickDamageDelay;
            Mist.Dmg = Dmg;
            Mist.SetManagerPool(Bomb.MistPool);
            managerPool.Release(this);
        }
        if(!IsMistBomb)
        {
            Collider[] HitCheck = Physics.OverlapSphere(transform.position, GetComponent<SphereCollider>().radius * 4);
            foreach (Collider col in HitCheck)
            {
                if (col.CompareTag("Player"))
                {
                    col.GetComponentInParent<IDamageAbleProps>().OnDamaged(Dmg, col.transform.position - transform.position);
                    col.GetComponentInParent<Rigidbody>().AddExplosionForce(Dmg, transform.position, GetComponent<SphereCollider>().radius * 4f);
                }
            }
            GameObject BombEft = Instantiate(MistPrefab, transform.position, Quaternion.identity);
            BombEft.GetComponent<Effect>().LookAtTarget = true;
            BombEft.transform.localScale *= GetComponent<SphereCollider>().radius * 4f;
            Destroy(gameObject);
        }
        DTimer = 0;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if ((collision.gameObject.CompareTag("Enemy") || collision.gameObject.layer == LayerMask.GetMask("Water")) && IsMistBomb)
        {
            collision.gameObject.GetComponent<IDamageAbleProps>().OnDamaged(Dmg * 5f, (collision.transform.position - transform.position).normalized);
            DetonateBomb();
        }

        if (!IsMistBomb)
        {
            DetonateBomb();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Water") && IsMistBomb)
        {
            DetonateBomb();
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Platform") && IsMistBomb && DTimer > DetonateTime)
        {
            DetonateBomb();
        }
    }
}
