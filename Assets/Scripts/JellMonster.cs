using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class JellMonster : MonoBehaviour, IDamageAbleProps, IMonster
{
    [Header("References")]
    public Transform Target;
    GameObject Appearence;
    Animator anim;
    Rigidbody rigid;
    NavMeshAgent agent;
    AudioSource AS;
    [Header("Resources")]
    public AudioClip[] MonsterIdleClips;
    float IdleSoundTimer;
    public AudioClip[] MonsterAttackClips;
    public AudioClip[] MonsterDeathClips;
    public GameObject DeathEffect;
    [Header("Stats")]
    public float hp = 10f;
    public float Damage = 1f;
    public float Speed = 7f;
    public float NoticeRange = 10f;
    public float AttackRange = 7f;
    bool attacking = false;
    public float AtkDelay = 0.5f;
    float AtkTimer = 0;
    float ExpireTime = 2f;
    float ExTimer = 0;
    [Header("Ray")]
    public float wall_detect_Range = 7f;
    Ray front_wall = new Ray();
    bool groundhited;

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

        AS = GetComponent<AudioSource>();

        IdleSoundTimer = Random.Range(2f, 3f);
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

            groundhited = Physics.Raycast(Appearence.transform.position, Vector3.down, GetComponentInChildren<CapsuleCollider>().height * 0.85f, LayerMask.GetMask("Platform"));
            anim.SetBool("OnGround", groundhited);

            if (attacking && rigid.velocity.y <= 0)
                ExTimer += Time.deltaTime;
            else
                ExTimer = 0;
        }
    }

    void idle()
    {
        bool hit = Physics.Raycast(front_wall, wall_detect_Range, LayerMask.GetMask("Platform"));
        if (Vector3.Distance(transform.position, Target.transform.position) < NoticeRange && !hit)
        {
            currentstate = FSMState.move;
            AS.volume = 1f;
        }
        else
        { 
            agent.SetDestination(transform.position);
            if (IdleSoundTimer < 0) {
                AS.volume = 0.5f;
                AS.PlayOneShot(MonsterIdleClips[Random.Range(0, MonsterIdleClips.Length)]);
                IdleSoundTimer = Random.Range(4f, 10f);
            }
            else
            {
                IdleSoundTimer -= Time.deltaTime;
            }
        }
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
        if (Vector3.Distance(transform.position, Target.transform.position) > AttackRange && !attacking)
        {
            currentstate = FSMState.move;
            rigid.velocity = Vector3.zero;
            agent.speed = Speed;
        }
        else
        {
            agent.speed = 0;
            if(AtkTimer >= AtkDelay && !attacking)
            {
                anim.SetTrigger("Attack");
                anim.SetBool("Attacking", true);
                AtkTimer = 0;
            }
            else if(!attacking)
            {
                AtkTimer += Time.deltaTime;
                rigid.velocity = Vector3.zero;
                if (agent != null)
                    agent.SetDestination(Target.transform.position);
            }
        }
    }

    public void Attack()
    {
        StartCoroutine(AttackSeq());
        agent.enabled = false;
    }

    public IEnumerator AttackSeq()
    {
        AS.PlayOneShot(MonsterAttackClips[Random.Range(0, MonsterAttackClips.Length)]);
        attacking = true;
        rigid.constraints = RigidbodyConstraints.FreezeRotation;
        rigid.velocity = Vector3.zero;
        rigid.AddForce(((Target.transform.position - transform.position).normalized * Speed * 2  + Vector3.up * Speed / 1.5f), ForceMode.Impulse);
        yield return new WaitForSeconds(0.5f);
        yield return new WaitUntil(()=> groundhited||ExpireTime<=ExTimer);
        anim.SetBool("Attacking", false);
        rigid.constraints = RigidbodyConstraints.FreezeAll;
        attacking = false;
        agent.enabled = true;
    }

    public void OnDamaged(float dmg, Vector3 hitnormal)
    {
        hp -= dmg;
        if (attacking)
        {
            rigid.AddForce(-hitnormal * dmg, ForceMode.Impulse);
        }
        else
        {
            StopAllCoroutines();
            StartCoroutine(DamagedSeq(hitnormal));
        }
        if(hp <= 0)
        {
            dead();
        }
    }

    public IEnumerator DamagedSeq(Vector3 hitnormal)
    {
        if (currentstate != FSMState.hit && !attacking)
        {
            anim.SetTrigger("Hit");
            agent.enabled = false;
            rigid.constraints = RigidbodyConstraints.FreezeRotation;
            rigid.velocity = Vector3.zero;
            rigid.AddForce(Target.transform.forward * 2f + Vector3.up * 3f, ForceMode.Impulse);
            currentstate = FSMState.hit;
        }
        yield return new WaitForSeconds(0.5f);
        yield return new WaitUntil(() => groundhited || ExpireTime <= ExTimer);
        anim.SetBool("Attacking", false);
        rigid.constraints = RigidbodyConstraints.FreezeAll;
        agent.enabled = true;
        AtkTimer = AtkDelay / 1.2f;
        currentstate = FSMState.attack;
    }

    void dead()
    {
        AS.PlayOneShot(MonsterDeathClips[Random.Range(0,MonsterDeathClips.Length)]);
        GameObject DeathEft = Instantiate(DeathEffect,transform.position, Quaternion.identity);
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
