using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RubberDuck : MonoBehaviour
{
    AudioSource AS;
    public AudioClip[] Quack;

    private void Start()
    {
        AS = GetComponent<AudioSource>();
    }
    public void QuackActivated()
    {
        AS.PlayOneShot(Quack[Random.Range(0,Quack.Length)]);
    }
}
