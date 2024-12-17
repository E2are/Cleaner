using RayFire;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Boss : MonoBehaviour
{
    NavMeshAgent agent;
    AudioSource AS;
    public GameObject Targets;
    public AudioClip[] BossSounds;
    public bool BossRunning = false;
    float Timer = 1;
    void Start()
    {
        agent = GetComponentInParent<NavMeshAgent>();
        AS = GetComponentInParent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (BossRunning)
        {
            agent.SetDestination(Targets.transform.position);
            if(Timer < 0 )
            {
                Timer = 1;
                GameManager.Instance.PC.DoShake(Random.Range(0.05f,0.09f));
                AS.pitch = Random.Range(0.3f, 1f);
                AS.PlayOneShot(BossSounds[1]);
            }
            else
            {
                Timer -= Time.deltaTime;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        IDamageAbleProps Hited = other.GetComponentInParent<IDamageAbleProps>();
        if(Hited != null)
        {
            Hited.OnDamaged(200, transform.forward);
            StartCoroutine(BombSeq());
            GameManager.Instance.PC.DoShake(0.1f);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        IDamageAbleProps Hited = other.GetComponentInParent<IDamageAbleProps>();
        if (Hited != null)
        {
            Hited.OnDamaged(200, transform.forward);
            GetComponentInParent<RayfireBomb>().Explode(0);
        }
    }

    IEnumerator BombSeq()
    {
        yield return new WaitForSeconds(0.1f);
        GetComponentInParent<RayfireBomb>().Explode(0);
        AS.PlayOneShot(BossSounds[0]);
    }
}
