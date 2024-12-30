using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NextStage : MonoBehaviour
{
    [Header("SavePointStats")]
    public bool IsSavePoint = false;
    public int SavePointIndex;
    [Header("SceneLoaderStats")]
    public bool IsSceneCalling = true;
    public string wantedScene;
    [SerializeField] bool ResetPlayer = false;
    [Header("DoorStats")]
    public bool IsDoorWaiter = false;
    [Header("TutorialSets")]
    public bool IsTutorialSec = false;
    public GameObject TutorialSets;
    int count = 0;
    public Canvas TextCanvas;
    public TMP_Text TutorialText;
    public string[] texts;
    [SerializeField] AudioClip TextClip;
    public GameObject OnPassDestroyObj;
    [SerializeField] AudioClip PassClip;

    CanvasGroup PlayerUI;
    Animator LoadingSceneUI;
    AudioSource AS;

    

    void Awake()
    {
        PlayerUI = FindObjectOfType<CanvasGroup>();
        if (GameObject.Find("Transition") != null)
            LoadingSceneUI = GameObject.Find("Transition").GetComponent<Animator>();
        if (LoadingSceneUI != null)
        {
            LoadingSceneUI.SetTrigger("Hide");
        }

        if (IsTutorialSec)
        {
            if (GameObject.Find("TutorialUI") != null && TextCanvas == null)
            {
                TextCanvas = GameObject.Find("TutorialUI").GetComponent<Canvas>();
                TutorialText = TextCanvas.GetComponentInChildren<TMP_Text>();
            }
            if(GameObject.Find("TutorialSets") != null)
            {
                TutorialSets = GameObject.Find("TutorialSets");
            }
        }

        if(IsSavePoint)
        SceneManager.sceneLoaded += CheckUsedSavePoint;

        AS = GetComponent<AudioSource>();
    }

    void CheckUsedSavePoint(Scene scene, LoadSceneMode mode)
    {
        if (SavePointIndex <= GameManager.Instance.SavePosition.current_SavePoint_Index)
        {
            Destroy(gameObject);
        }
    }

    private void OnDisable()
    {
        if(IsSavePoint)
        SceneManager.sceneLoaded -= CheckUsedSavePoint;
    }

    private void Start()
    {
        if(TextCanvas != null)
        {
            TextCanvas.gameObject.SetActive(false);
        }
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (IsSceneCalling && !GameManager.Instance.Dead)
            {
                StartCoroutine(LoadingProsess());
            }
            if(IsDoorWaiter)
            {
                GetComponentInChildren<Animator>().SetBool("Opening",true);
            }
            if(IsSavePoint)
            {
                GameManager.Instance.SavePosition.current_SavePoint_Index = SavePointIndex;
                GameManager.Instance.SavePosition.playerHP = GameManager.Instance.HP;
                GameManager.Instance.SavePosition.playerRemainedCartridge = GameManager.Instance.RemainedCartridgeCount;
                GetComponent<BoxCollider>().enabled = false;
            }
            if (IsTutorialSec)
            {
                QuitTexting();
                AS.PlayOneShot(PassClip);
                StartTexting();
            }
            if (OnPassDestroyObj != null)
            {
                Destroy(OnPassDestroyObj);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (IsDoorWaiter)
            {
                GetComponentInChildren<Animator>().SetBool("Opening", false);
            }
        }
    }

    public void StartTexting()
    {
        GetComponent<BoxCollider>().enabled = false;
        TutorialText.text = "";
        count = 0;
        TextCanvas.gameObject.SetActive(true);
        StartCoroutine(Texting());
    }

    public void QuitTexting()
    {
        NextStage[] TutorialSet = TutorialSets.GetComponentsInChildren<NextStage>();
        foreach(NextStage nextStage in TutorialSet)
        {
            if (nextStage.IsTutorialSec)
            {
                nextStage.StopAllCoroutines();
            }
        }
        TutorialText.text = "";
        TextCanvas.gameObject.SetActive(false);
    }

    IEnumerator Texting()
    {
        while (count < texts.Length)
        {
            for (int i = 0; i < texts[count].Length; i++)
            {
                TutorialText.text += texts[count][i];
                GetComponent<AudioSource>().PlayOneShot(TextClip);
                yield return new WaitForSeconds(0.05f);
            }
            yield return new WaitForSeconds(1f);
            count++;
            TutorialText.text = "";
        }
        TextCanvas.gameObject.SetActive(false);
        yield return null;
    }

    public void LoadScene(string sceneName)
    {
        wantedScene = sceneName;
        GameManager.Instance.current_SceneName = wantedScene;
        GameManager.Instance.ResetPlayerStats();
        StartCoroutine(LoadingProsess());
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void Pause()
    {
        GameManager.Instance.Pause();
    }

    public void UnPause()
    {
        GameManager.Instance.UnPause();
    }

    IEnumerator LoadingProsess()
    {
        if (wantedScene == "Ending") GameManager.Instance.Ended = true;
        if (wantedScene != "Tutorial")
        {
            GameManager.Instance.ChangeSaveData(wantedScene);
            GameManager.Instance.SavePosition.current_SavePoint_Index = 0;
            GameManager.Instance.SavePosition.playerHP = 100;
            GameManager.Instance.SavePosition.playerRemainedCartridge = 3;
        }
        yield return new WaitForSeconds(0.01f);
        Time.timeScale = 0;
        if (LoadingSceneUI != null)
        {
            LoadingSceneUI.SetBool("AnimFullyLoaded", false);
            LoadingSceneUI.SetTrigger("Reveal");
            yield return new WaitUntil(() => LoadingSceneUI.GetBool("AnimFullyLoaded"));
        }
        Time.timeScale = 1;
        if(ResetPlayer)GameManager.Instance.ResetPlayerStats();
        LoadingScene.LoadScene(wantedScene);
    }

}
