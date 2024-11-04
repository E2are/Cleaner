using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;

public class LoadingScene : MonoBehaviour
{
    public static string nextScene;
    [SerializeField] Slider ProgressBar;
    private void OnLevelWasLoaded(int level)
    {
        StartCoroutine(LoadScene());
    }
    public static void LoadScene(string scene)
    {
        nextScene = scene;
        SceneManager.LoadScene("LoadingScene");
    }
    IEnumerator LoadScene()
    {
        yield return null;
        Time.timeScale = 1.0f;
        AsyncOperation op = SceneManager.LoadSceneAsync(nextScene);
        op.allowSceneActivation = false;
        float timer = 0;
        while(!op.isDone)
        {
            yield return null;
            timer += Time.deltaTime;
            if(op.progress < 0.9f)
            {
                ProgressBar.value = Mathf.Lerp(ProgressBar.value, op.progress, timer);
                if(ProgressBar.value >= op.progress)
                {
                    timer = 0f;
                }
            }
            else
            {
                ProgressBar.value = Mathf.Lerp(ProgressBar.value, 1f, timer);
                if(ProgressBar.value == 1.0f)
                {
                    op.allowSceneActivation = true;
                    yield break;
                }
            }
        }
    }
}
