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
        Target = FindObjectOfType<PlayerMove>().orientation;

        agent = GetComponent<NavMeshAgent>();

        agent.speed = Speed;

        Appearence = GetComponentInChildren<SpriteRenderer>().gameObject;

        rigid = GetComponent<Rigidbody>();

        anim = Appearence.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!GameManager.Instance.Paused)
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

            Appearence.transform.forward = Target.transform.forward;
        }
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
        bool hit = Physics.Raycast(front_wall, wall_detect_Range, LayerMask.GetMask("Platform"));

        if (Vector3.Distance(transform.position, Target.transform.position) < AttackRange && !hit)
        {
            currentstate = FSMState.attack;
            AtkTimer = AtkDelay / 1.2f;
        }
        else if(Vector3.Distance(transform.position, Target.transform.position) > NoticeRange && hit)
        {
            currentstate = FSMState.idle;
        }
        else
        {
            if (!attacking)
            {
                rigid.velocity = Vector3.zero;
                agent.SetDestination(Target.transform.position);
            }
        }
    }

    void attack()
    {
        if (agent != null)
            agent.SetDestination(Target.transform.position);
        if (AtkTimer >= AtkDelay && !attacking)
        {
            anim.SetTrigger("Attack");
            AtkTimer = 0;
        }
        else if(!attacking)
        {
            AtkTimer += Time.deltaTime;
        }
    }

    public void Attack()
    {
        StartCoroutine(AttackSeq());
    }

    public IEnumerator AttackSeq()
    {
        yield return null;
        GameObject Explode = Instantiate(DeathEffect,Appearence.transform.position,Quaternion.identity);
        Explode.transform.localScale = Vector3.one * AttackRange;
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
        GameObject DeathEft = Instantiate(DeathEffect,Appearence.transform.position, Quaternion.identity);
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
