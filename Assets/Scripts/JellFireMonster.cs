using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class JellFireMonster : MonoBehaviour, IDamageAbleProps, IMonster
{
    [Header("References")]
    public Transform Target;
    GameObject Appearence;
    Animator anim;
    Rigidbody rigid;
    NavMeshAgent agent;
    public GameObject Fire_Object;
    [Header("Stats")]
    public float hp = 10f;
    public float Damage = 1f;
    public float Speed = 7f;
    public float NoticeRange = 10f;
    public float AttackRange = 7f;
    bool attacking = false;
    public float AtkDelay = 0.5f;
    float AtkTimer = 0;
    [Header("Ray")]
    public float wall_detect_Range = 7f;
    Ray front_wall = new Ray();

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
            AtkTimer = AtkDelay / 2f;
        }
        else if(Vector3.Distance(transform.position, Target.transform.position) > NoticeRange)
        {
            currentstate = FSMState.idle;
        }
        else
        {
            agent.SetDestination(Target.transform.position);
        }
    }

    void attack()
    {
        rigid.velocity = Vector3.zero;
        if (Vector3.Distance(transform.position, Target.transform.position) > AttackRange && !attacking)
        {
            currentstate = FSMState.move;
            agent.speed = Speed;
        }
        else
        {
            agent.speed = 0;
            if(AtkTimer >= AtkDelay && !attacking)
            {
                StartCoroutine(AttackSeq());
                AtkTimer = 0;
            }
            else if(!attacking)
            {
                AtkTimer += Time.deltaTime;
                if(agent!= null)
                    agent.SetDestination(Target.transform.position);
            }
        }
    }

    public void Attack()
    {
        StartCoroutine (AttackSeq());
    }

    public IEnumerator AttackSeq()
    {
        yield return null;
        attacking = true;
        GameObject FireObject = Instantiate(Fire_Object,transform.position,Quaternion.identity);
        if(FireObject.GetComponent<Rigidbody>() != null)
        {
            FireObject.GetComponent<Rigidbody>().AddForce((Target.transform.position - transform.position).normalized * Vector3.Distance(Target.position,transform.position) + transform.up * Speed * 0.75f,ForceMode.Impulse);
        }
        FireObject.GetComponent<BathBombPrefab>().Dmg = Damage;
        attacking = false;
        
    }

    public void OnDamaged(float dmg, Vector3 hitnormal)
    {
        hp -= dmg;
        if(hp <= 0)
        {
            dead();
        }
    }

    void dead()
    {
        gameObject.SetActive(false);
    }

    private void OnCollisionEnter(Collision other)
    {

    }
}
