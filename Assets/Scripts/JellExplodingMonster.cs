using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class JellExplodingMonster : MonoBehaviour, IDamageAbleProps, IMonster
{
    [Header("References")]
    public Transform Target;
    GameObject Appearence;
    Animator anim;
    Rigidbody rigid;
    NavMeshAgent agent;
    AudioSource AS;
    public GameObject DeathEffect;
    [Header("Stats")]
    public float hp = 10f;
    public float Damage = 1f;
    public float Speed = 7f;
    public float NoticeRange = 10f;
    public float AttackRange = 2f;
    bool attacking = false;
    public float AtkDelay = 0.5f;
    float AtkTimer = 0;
    [Header("Ray")]
    public float wall_detect_Range = 7f;
    Ray front_wall = new Ray();
    bool Trigger = false;
    [Header("Sounds")]
    public AudioClip stretching_skinSound;
    public AudioClip PopSound; 

    public FSMState currentstate;
    public enum FSMState
    {
        idle,
        move,
        attack,
        hit,
        dead
    }
    void Start()
    {
        AS = GetComponent<AudioSource>();

        agent = GetComponent<NavMeshAgent>();

        agent.speed = Speed;

        Appearence = GetComponentInChildren<SpriteRenderer>().gameObject;

        rigid = GetComponent<Rigidbody>();

        anim = Appearence.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!GameManager.Instance.Paused && !GameManager.Instance.Dead)
        {
            if (!GameManager.Instance.IsCinemachining)
            {
                switch (currentstate)
                {
                    case FSMState.idle:
                        idle();
                        break;
                    case FSMState.move:
                        move();
                        break;
                    case FSMState.attack:
                        attack();
                        break;
                    case FSMState.dead:
                        dead();
                        break;
                }

                front_wall = new Ray(transform.position, (Target.transform.position - transform.position).normalized);
            }
            Appearence.transform.forward = Target.transform.forward;
        }
    }

    public void TargetSet(Transform target)
    {
        Target = target;
    }

    void idle()
    {
        bool hit = Physics.Raycast(front_wall, wall_detect_Range, LayerMask.GetMask("Platform"));
        if (Vector3.Distance(transform.position, Target.transform.position) < NoticeRange && !hit)
        {
            currentstate = FSMState.move;
        }
        else agent.SetDestination(transform.position);
    }

    void move()
    {
        if (Vector3.Distance(transform.position, Target.transform.position) < AttackRange)
        {
            currentstate = FSMState.attack;
        }
        else if(Vector3.Distance(transform.position, Target.transform.position) > NoticeRange)
        {
            currentstate = FSMState.idle;
        }
        else
        {
            rigid.velocity = Vector3.zero;
            agent.SetDestination(Target.transform.position);   
        }
    }

    void attack()
    {
        if (agent != null)
            agent.SetDestination(Target.transform.position);
        anim.SetTrigger("Attack");
    }

    public void Attack()
    {
        StartCoroutine(AttackSeq());
    }

    public IEnumerator AttackSeq()
    {
        AS.PlayOneShot(stretching_skinSound);
        yield return new WaitUntil(()=>anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f);
        GameObject Explode = Instantiate(DeathEffect,Appearence.transform.position,Quaternion.identity);
        Explode.transform.localScale = Vector3.one * AttackRange;
        Explode.GetComponent<AudioSource>().clip = PopSound;
        Explode.GetComponent<AudioSource>().Play();
        Collider[] Explosion = Physics.OverlapSphere(transform.position, AttackRange);
        foreach(Collider collider in Explosion)
        {
            if(collider.GetComponentInParent<IDamageAbleProps>() != null && collider.CompareTag("Player"))
            {
                collider.GetComponentInParent<IDamageAbleProps>().OnDamaged(Damage,(collider.transform.position-transform.position).normalized);   
            }
            if (collider.GetComponent<Rigidbody>() != null)
            {
                collider.GetComponent<Rigidbody>().AddExplosionForce(Damage / 2, transform.position, AttackRange);
            }
        }
        AS.PlayOneShot(PopSound);
        gameObject.SetActive(false);
    }

    public void OnDamaged(float dmg, Vector3 hitnormal)
    {
        hp -= dmg;
        if (currentstate == FSMState.idle)
        {
            currentstate = FSMState.move;
        }
        if (attacking)
        {
            rigid.AddForce(-hitnormal * dmg, ForceMode.Impulse);
        }
        if(hp <= 0)
        {
            dead();
        }
    }

    void dead()
    {
        GameObject Explode = Instantiate(DeathEffect, Appearence.transform.position, Quaternion.identity);
        Explode.transform.localScale = Vector3.one * AttackRange;
        Explode.GetComponent<AudioSource>().clip = PopSound;
        Explode.GetComponent<AudioSource>().Play();
        gameObject.SetActive(false);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player") && attacking)
        {
            other.gameObject.GetComponentInParent<IDamageAbleProps>().OnDamaged(Damage,Vector3.zero);
            rigid.velocity = new Vector3(-rigid.velocity.x, rigid.velocity.y, -rigid.velocity.z);
        }
    }
}
