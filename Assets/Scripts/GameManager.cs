using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance = null;
    public bool IsCinemachining = true;
    public bool Paused = false;
    [Header("Player Move Stats")]
    public PlayerMove PM;
    public PlayerCam PC;
    [Header("Player Weapon Stats")]
    public BubbleGun BGun;
    public Mop Mop;
    public BathBomb BBomb;
    [Header("PlayerStats")]
    public float HP = 100f;
    public float MaxHP = 100f;
    public bool Dead = false;
    [HideInInspector]
    public float maxAmmo_Amount;
    public float RemainAmmo_Amount = 30;
    public int RemainedCartridgeCount = 3;
    [Header("PlayerUI")]
    public CanvasGroup PlayerCanvas;
    public GameObject CutSceneUI;
    public GameObject LoadSceneUI;
    Button[] Buttons;
    float fadeout = 0;
    public AudioMixer VolumeMaster;
    public Slider BGMSlider;
    public float BGMVal = 0;
    public Slider SFXSlider;
    public float SFXVal = 0;
    [Header("GameStates")]
    public string current_SceneName;
    public ScriptableSavePositions[] SavePositions;
    public ScriptableSavePositions SavePosition;
    public bool Ended = false;

    [Header("Enemys")]
    public GameObject MonsterSets;
    [HideInInspector]
    public IMonster[] Monsters;
    Boss Boss;

    public bool BubblegunHolded = true;
    [SerializeField] float swapDelay = 0.1f;
    float swapTimer= 0;
    float expiretime = 1;
    [Header("Inputs")]
    public KeyCode SwapKey = KeyCode.Q;
    public KeyCode BubbleGunKey = KeyCode.Alpha1;
    public KeyCode MopKey = KeyCode.Alpha2;
    [Header("HitImage")]
    public float hitAlpha = 1;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }
    private void Start()
    {
        DontDestroyOnLoad(this);

        SceneManager.sceneLoaded += OnSceneLoaded;

        maxAmmo_Amount = RemainAmmo_Amount;

        if (GameObject.Find("Monsters") != null)
        {
            MonsterSets = GameObject.Find("Monsters");
            Monsters = MonsterSets.GetComponentsInChildren<IMonster>();
            foreach (IMonster monster in Monsters)
            {
                monster.TargetSet(PC.transform);
            }
        }

        if (GameObject.Find("Boss") != null)
        {
            Boss = GameObject.Find("Boss").GetComponent<Boss>();
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
            PM = FindObjectOfType<PlayerMove>();
     
            PC = FindObjectOfType<PlayerCam>();

            BGun = FindObjectOfType<BubbleGun>();
        
        if (GameObject.Find("PlayerUI") != null)
            PlayerCanvas = GameObject.Find("PlayerUI").GetComponent<CanvasGroup>();

        if (GameObject.Find("CutSceneUIs"))
            CutSceneUI = GameObject.Find("CutSceneUIs");

        if (BGun != null)
            BGun.BubbleGunanim.SetBool("GunisHolded", BubblegunHolded);

        if (GameObject.Find("LoadSceneUI") != null)
        {
            LoadSceneUI = GameObject.Find("LoadSceneUI");
            Time.timeScale = 1;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Paused = false;
            Slider[] SoundSliders = LoadSceneUI.GetComponentsInChildren<Slider>();
            BGMSlider = SoundSliders[0];
            SFXSlider = SoundSliders[1];
            while(BGMSlider.value != BGMVal)
            BGMSlider.value = BGMVal;
            while(SFXSlider.value != SFXVal)
            SFXSlider.value = SFXVal;
        }

        if (GameObject.Find("Monsters") != null)
        {
            MonsterSets = GameObject.Find("Monsters");
            Monsters = MonsterSets.GetComponentsInChildren<IMonster>();
            foreach(IMonster monster in Monsters)
            {
                monster.TargetSet(PC.transform);
            }
        }

        ChangeSaveData(scene.name);

        if (GameObject.Find("Boss") != null)
        {
            Boss = GameObject.Find("Boss").GetComponent<Boss>();
        }

        if(PM != null)
        StartCoroutine(ToTheSavePoint());

        RemainAmmo_Amount = 33;

        fadeout = 0;
    }

    public void Pause()
    {
        if (LoadSceneUI != null)
        {
            if (LoadSceneUI.GetComponent<Animator>().GetBool("AnimFullyLoaded"))
            {
                if (LoadSceneUI != null)
                {
                    LoadSceneUI.GetComponent<Animator>().SetTrigger("Reveal");
                    LoadSceneUI.GetComponent<Animator>().SetBool("AnimFullyLoaded", false);
                }

                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                if(current_SceneName != "Title")
                    Time.timeScale = 0;
                
                Paused = true;
            }
        }
    }

    public void UnPause()
    {
        if (LoadSceneUI != null)
        {
            if (LoadSceneUI.GetComponent<Animator>().GetBool("AnimFullyLoaded"))
            {
                
                if (LoadSceneUI != null)
                {
                    LoadSceneUI.GetComponent<Animator>().SetTrigger("Hide");
                    LoadSceneUI.GetComponent<Animator>().SetBool("AnimFullyLoaded", false);
                }
                if (!current_SceneName.Contains("Title"))
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                    Time.timeScale = 1;
                }
                Paused = false;
            }
        }
    }

    private void Update()
    {
        if (Dead)
        {
            hitAlpha = Mathf.Lerp(hitAlpha, 0, Time.deltaTime * 2);
        }
        if (Input.GetKeyDown(KeyCode.Escape)&&!Paused)
            Pause();
        if (Input.GetKeyDown(KeyCode.Escape)&&Paused)
            UnPause();
        if (Paused && Dead)
        {
            return; 
        }

        if(BGun != null) {
            if (swapDelay <= swapTimer && BGun.shootAble && !BGun.reloading && BGun.BubbleGunanim.GetBool("WeaponIsHolded"))
            {
                if (Input.GetKeyDown(SwapKey))
                {
                    BubblegunHolded = !BubblegunHolded;
                    swapTimer = 0;
                    BGun.BubbleGunanim.SetBool("GunisHolded", BubblegunHolded);
                    BGun.BubbleGunanim.SetBool("WeaponIsHolded",false);
                }
                if (Input.GetKeyDown(BubbleGunKey))
                {
                    BubblegunHolded = true;
                    swapTimer = 0;
                    BGun.BubbleGunanim.SetBool("GunisHolded", true);
                    BGun.BubbleGunanim.SetBool("WeaponIsHolded", false);
                }
                if (Input.GetKeyDown(MopKey))
                {
                    BubblegunHolded = false;
                    swapTimer = 0;
                    BGun.BubbleGunanim.SetBool("GunisHolded", false);
                    BGun.BubbleGunanim.SetBool("WeaponIsHolded", false);
                }
            }
            else
            {
                swapTimer += Time.deltaTime;
            }

            if (!BGun.BubbleGunanim.GetBool("WeaponIsHolded"))
            {
                if(expiretime < 0)
                {
                    BGun.BubbleGunanim.SetBool("WeaponIsHolded", true);
                    expiretime = 0.2f;
                }
                else
                {
                    expiretime -= Time.deltaTime;
                }
            }
        }
        if (Ended)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        hitAlpha = Mathf.Lerp(hitAlpha, 1, Time.deltaTime);

        SoundVolumeSet();
    }
    private void FixedUpdate()
    {
        if (PlayerCanvas != null)
        {
            Cinemachining();
        }
    }

    void Cinemachining()
    {
        if (IsCinemachining && CutSceneUI != null)
        {
            if (fadeout <= 0.99f)
            {
                fadeout = Mathf.Lerp(fadeout, 1, Time.unscaledDeltaTime);
                PlayerCanvas.alpha = 0.9f - fadeout;
            }
        }
        else
        {
            if (fadeout >= 0.01f)
            {
                fadeout = Mathf.Lerp(fadeout, 0, Time.unscaledDeltaTime);
                PlayerCanvas.alpha = 1f - fadeout;
            }
        }
        if(CutSceneUI != null)
            CutSceneUI.SetActive(IsCinemachining);
        
    }

    void SoundVolumeSet()
    {
        if (BGMSlider != null)
        {
            BGMVal = BGMSlider.value;

            if (BGMVal == -40f)
                VolumeMaster.SetFloat("BGM", -80f);
            else
                VolumeMaster.SetFloat("BGM", BGMVal);
        }

        if (SFXSlider != null)
        {
            SFXVal = SFXSlider.value;

            if (SFXVal == -40f)
                VolumeMaster.SetFloat("SFX", -80f);
            else
                VolumeMaster.SetFloat("SFX", SFXVal);
        }
    }

    public void ResetPlayerStats()
    {
        HP = MaxHP;
        RemainedCartridgeCount = 3;
        RemainAmmo_Amount = maxAmmo_Amount;
        BubblegunHolded = true;
        Dead = false;
        Ended = false;
    }

    public void ChangeSaveData(string SceneName)
    {
        switch (SceneName)
        {
            case "Title":
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                break;
            case "Tutorial":
                SavePosition = SavePositions[0];
                break;
            case "Stage1":
                SavePosition = SavePositions[1];
                break;
        }
    }

    IEnumerator ToTheSavePoint()
    {
        yield return null;
        HP = SavePosition.playerHP;
        RemainedCartridgeCount = SavePosition.playerRemainedCartridge;
        while (PM.transform.position != SavePosition.Points[SavePosition.current_SavePoint_Index])
        {
            PM.transform.position = SavePosition.Points[SavePosition.current_SavePoint_Index];
            HP = SavePosition.playerHP;
            RemainedCartridgeCount = SavePosition.playerRemainedCartridge;
            yield return null;
        }
    }
}
