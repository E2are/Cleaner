using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance = null;
    public bool IsCinemachining = true;
    public bool Paused = false;
    [Header("References")]
    [Space(5f)]
    [Header("Player Move Stats")]
    public PlayerMove PM;
    [Header("Player Weapon Stats")]
    public BubbleGun BGun;
    public Mop Mop;
    public BathBomb BBomb;
    [Header("PlayerStats")]
    public float HP = 100f;
    public float MaxHP = 100f;
    [HideInInspector]
    public float maxAmmo_Amount;
    public float RemainAmmo_Amount = 30;
    public int RemainedCartridgeCount = 3;
    [Header("PlayerUI")]
    public CanvasGroup PlayerCanvas;
    public GameObject CutSceneUI;
    [HideInInspector]
    public GameObject LoadSceneUI;
    Button[] Buttons;
    float fadeout = 0;
    public AudioMixer VolumeMaster;
    public Slider BGMSlider;
    public Slider SFXSlider;
    
    public bool BubblegunHolded = true;
    [SerializeField] float swapDelay = 0.3f;
    float swapTimer= 0;
    float expiretime = 1;
    [Header("Inputs")]
    public KeyCode SwapKey = KeyCode.Q;
    public KeyCode BubbleGunKey = KeyCode.Alpha1;
    public KeyCode MopKey = KeyCode.Alpha2;
    [Header("HitImage")]
    public float hitAlpha = 1;
    private void Start()
    {
        DontDestroyOnLoad(this);

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }

        maxAmmo_Amount = RemainAmmo_Amount;
        if (PM == null)
            PM = FindObjectOfType<PlayerMove>();

        if (BGun == null)
            BGun = FindObjectOfType<BubbleGun>();

        if (PlayerCanvas == null)
            PlayerCanvas = GameObject.Find("PlayerUI").GetComponent<CanvasGroup>();

        if (CutSceneUI == null)
            CutSceneUI = GameObject.Find("CutSceneUIs");

        if (LoadSceneUI == null)
            LoadSceneUI = GameObject.Find("LoadSceneUI");

        if (BGMSlider == null)
            BGMSlider = GameObject.Find("BGMVolume").GetComponent<Slider>();
        
    }

    private void OnLevelWasLoaded(int level)
    {
        if (PM == null)
            PM = FindObjectOfType<PlayerMove>();
        if (BGun == null)
            BGun = FindObjectOfType<BubbleGun>();
        
        if (GameObject.Find("PlayerUI") != null)
            PlayerCanvas = GameObject.Find("PlayerUI").GetComponent<CanvasGroup>();
        Debug.Log(PlayerCanvas);

        if (GameObject.Find("CutSceneUIs"))
            CutSceneUI = GameObject.Find("CutSceneUIs");
        Debug.Log(CutSceneUI);

        if (BGun != null)
            BGun.BubbleGunanim.SetBool("GunisHolded", BubblegunHolded);

        if (GameObject.Find("LoadSceneUI") != null)
        {
            LoadSceneUI = GameObject.Find("LoadSceneUI");
            Buttons = LoadSceneUI.GetComponentsInChildren<Button>();
            Buttons[0].onClick.AddListener(() => CallScene("title"));
            Buttons[1].onClick.AddListener(() => ToQuit());

            Slider[] slider = LoadSceneUI.GetComponentsInChildren<Slider>();
            foreach(Slider sliderItem in slider)
            {
                sliderItem.onValueChanged.AddListener(delegate { SoundVolumeSet(); });
            }

            Time.timeScale = 1;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Paused = false;
        }

        if (GameObject.Find("BGMSlider") != null)
        {
            BGMSlider = GameObject.Find("BGMSlider").GetComponent<Slider>();
        }
        
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
                Time.timeScale = 1;
                if (LoadSceneUI != null)
                {
                    LoadSceneUI.GetComponent<Animator>().SetTrigger("Hide");
                    LoadSceneUI.GetComponent<Animator>().SetBool("AnimFullyLoaded", false);
                }
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                Paused = false;
            }
        }
    }

    private void Update()
    {
        if (!Paused)
        {
            if(BGun != null) {
                if (swapDelay <= swapTimer && BGun.shootAble && BGun.BubbleGunanim.GetBool("WeaponIsHolded"))
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
                        expiretime = 1;
                    }
                    else
                    {
                        expiretime -= Time.deltaTime;
                    }
                }
            }            

            hitAlpha = Mathf.Lerp(hitAlpha, 1, Time.deltaTime);
    
        }

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
            float soundVal = BGMSlider.value;

            if (soundVal == -40f)
                VolumeMaster.SetFloat("BGM", -80f);
            else
                VolumeMaster.SetFloat("BGM", soundVal);
        }

        if (SFXSlider != null)
        {
            float soundVal = SFXSlider.value;

            if (soundVal == -40f)
                VolumeMaster.SetFloat("SFX", -80f);
            else
                VolumeMaster.SetFloat("SFX", soundVal);
        }
    }

    void CallScene(string wantedScene)
    {
        LoadingScene.LoadScene(wantedScene);
        IsCinemachining = false;
    }   

    public void ToQuit()
    {
        Application.Quit();
    }
}
