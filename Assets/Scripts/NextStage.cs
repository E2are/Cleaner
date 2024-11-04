using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class NextStage : MonoBehaviour
{
    public string wantedScene;
    CanvasGroup PlayerUI;
    Animator LoadingSceneUI;
    private Volume volume;
    private Vignette vig;
    void Awake()
    {
        PlayerUI = FindObjectOfType<CanvasGroup>();
        volume = FindObjectOfType<Volume>();
        volume.profile.TryGet(out vig);
        if (GameObject.Find("Transition") != null)
            LoadingSceneUI = GameObject.Find("Transition").GetComponent<Animator>();
        Debug.Log(LoadingSceneUI);
        if (LoadingSceneUI != null)
        {
            LoadingSceneUI.SetTrigger("Reveal");
            LoadingSceneUI.SetTrigger("Hide");
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            StartCoroutine(LoadingProsess());
        }
    }

    public void LoadScene(string sceneName)
    {
        wantedScene = sceneName;
        StartCoroutine(LoadingProsess());
    }

    public void Quit()
    {
        Application.Quit();
    }

    IEnumerator LoadingProsess()
    {
        yield return new WaitForSeconds(0.01f);
        Time.timeScale = 0;
        if (LoadingSceneUI != null)
        {
            LoadingSceneUI.SetBool("AnimFullyLoaded", false);
            LoadingSceneUI.SetTrigger("Reveal");
            yield return new WaitUntil(() => LoadingSceneUI.GetBool("AnimFullyLoaded"));
        }
        Time.timeScale = 1;
        LoadingScene.LoadScene(wantedScene);
    }
}
