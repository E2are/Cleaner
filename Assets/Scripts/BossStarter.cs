using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossStarter : MonoBehaviour
{
    public Boss boss;
    void Start()
    {
        if (GameObject.Find("Boss") != null)
        {
            boss = GameObject.Find("Boss").GetComponent<Boss>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            boss.BossRunning = true;
            BGMManager.Instance.ChangeMusic("Boss");
            Destroy(gameObject);
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            boss.BossRunning = true;
            BGMManager.Instance.ChangeMusic("Boss");
            Destroy(gameObject);
        }
    }
}
