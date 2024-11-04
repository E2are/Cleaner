using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RayFire;

public class RayFireAbleObject : MonoBehaviour, IDamageAbleProps
{
    RayfireRigid RFrigid;
    public GameObject DropObject;
    public float mesh;
    float hp = 3f;
    void Start()
    {
        RFrigid = GetComponent<RayfireRigid>();
        RFrigid.Activate();
        GetComponent<Rigidbody>().mass = mesh;
    }

    public void OnDamaged(float damage, Vector3 normal)
    {
        hp -= damage;
        Debug.Log("Damaged");
        if (hp <= 0)
        {
            transform.gameObject.layer = 7;
            if(DropObject != null)
            {
                Instantiate(DropObject, transform.position, Quaternion.identity);
            }
            RFrigid.Demolish();
        }
    }
}
