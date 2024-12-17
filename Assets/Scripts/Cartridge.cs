using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cartridge : MonoBehaviour
{
    AudioSource AS;
    public float HealAmount = 40;
    public AudioClip[] UseSound;
    public Transform Target;
    Transform Appearence;
    private void Start()
    {
        AS = GetComponentInParent<AudioSource>();
        Target = GameObject.Find("Player").GetComponent<PlayerMove>().orientation;
        Appearence = transform.GetChild(0);
    }
    private void Update()
    {
        Appearence.forward = Target.forward;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player")){ 
            GameManager.Instance.RemainedCartridgeCount++;
            AS.PlayOneShot(UseSound[Random.Range(0, UseSound.Length)]);
            Destroy(gameObject);
        }
    }
}
