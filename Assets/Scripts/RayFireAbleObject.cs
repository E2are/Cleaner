using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RayFire;

public class RayFireAbleObject : MonoBehaviour, IDamageAbleProps
{
    RayfireRigid RFrigid;
    AudioSource AS;
    public GameObject DropObject;
    public GameObject DestroyObj;
    public AudioClip[] DestroySound;
    public float mass;
    public bool IsStatic = false;
    public float hp = 3f;

    void Awake()
    {
        RFrigid = GetComponent<RayfireRigid>();
        AS = GetComponentInParent<AudioSource>();
        RFrigid.Activate();
    }
    void Start()
    {
        GetComponent<Rigidbody>().mass = mass;
        GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.Discrete;
        if (GetComponent<MeshCollider>() != null && IsStatic)
        {
            GetComponent<MeshCollider>().enabled = false;
        }
    }


    public void OnDamaged(float damage, Vector3 normal)
    {
        hp -= damage;
        if (AS != null && DestroySound != null)
        {
            AS.PlayOneShot(DestroySound[Random.Range(0, DestroySound.Length)]);
        }
        if (hp <= 0)
        {
            gameObject.layer = 7;
            gameObject.tag = "Props";
            if (DropObject != null)
            {
                Instantiate(DropObject, transform.position + Vector3.up * 0.4f, Quaternion.identity);
            }
            if(DestroyObj != null)
            {
                if (DestroyObj.GetComponent<RayfireRigid>() != null)
                {
                    DestroyObj.GetComponent<RayfireRigid>().Demolish();
                }
                else
                {
                    DestroyObj.SetActive(false);
                }

                NextStage[] TutorialSets = GameObject.Find("TutorialSets").GetComponentsInChildren<NextStage>();
                foreach(NextStage NS in TutorialSets)
                {
                    NS.QuitTexting();
                }
            }
            if (GetComponent<MeshCollider>() != null)
            {
                GetComponent<MeshCollider>().enabled = true;
            }
            RFrigid.Demolish();
        }
    }
}
