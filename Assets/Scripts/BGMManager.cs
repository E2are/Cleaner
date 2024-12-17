using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance;

    [HideInInspector]
    public AudioSource AS;
    public AudioClip FieldBGM;
    public AudioClip BossBGM;
    public AudioClip GameOverBGM;
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }   
        else
        {
            Destroy(Instance);
        }

        AS = GetComponent<AudioSource>();
    }

    public void ChangeMusic(string BGMname)
    {
        StartCoroutine(ChangeSeq(BGMname));
    }

    IEnumerator ChangeSeq(string BGMname)
    {
        while(AS.volume > 0.1f)
        {
            AS.volume -= Time.deltaTime;
            yield return new WaitForSeconds(0.01f);
        }
        ChangeClip(BGMname);
        yield return new WaitForSeconds(0.01f);
        while (AS.volume < 1)
        {
            AS.volume += Time.deltaTime;
            yield return new WaitForSeconds(0.01f);
        }
        AS.volume = 1;
        yield return new WaitForSeconds(0.01f);
    }

    void ChangeClip(string BGMname)
    {
        switch(BGMname) {

            case "Field":
                AS.clip = FieldBGM;
                break;
            case "Boss":
                AS.clip = BossBGM;
                break;
            case "GameOver":
                AS.clip = GameOverBGM;
                break;
        }
        AS.Play();
    }
}
