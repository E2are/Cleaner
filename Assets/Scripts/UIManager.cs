using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public BubbleGun BG;

    public Slider HP;
    public Image HitedImage;
    public Image RemainedAmmoAmount_Image;
    public TMP_Text RemainedCartridgeCount_Text;
    public Animator GameOverAnim;
    private void Awake()
    {
        if(Instance == null)
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
        if(FindObjectOfType<BubbleGun>() != null)
        BG = FindObjectOfType<BubbleGun>();
        if (GameObject.Find("HP") != null)
            HP = GameObject.Find("HP").GetComponent<Slider>();
        if(GameObject.Find("HitImage")!=null)
            HitedImage = GameObject.Find("HitImage").GetComponent<Image>();
        if (GameObject.Find("CurrentAmmoAmount") != null)
            RemainedAmmoAmount_Image = GameObject.Find("CurrentAmmoAmount").GetComponent<Image>();
        if (GameObject.Find("CurrentCnt") != null)
            RemainedCartridgeCount_Text = GameObject.Find("CurrentCnt").GetComponent<TMP_Text>();
        if (GameObject.Find("GameOverUI") != null)
        {
            GameOverAnim = GameObject.Find("GameOverUI").GetComponent<Animator>();
            GameOverAnim.gameObject.SetActive(false);
        }
    }

    public void Update()
    {
        if(HP !=null)
        HP.value = GameManager.Instance.HP / GameManager.Instance.MaxHP;

        if(HitedImage != null )
        HitedImage.material.SetFloat("_Desintegration_Value_1", GameManager.Instance.hitAlpha);

        if(RemainedAmmoAmount_Image != null )
        RemainedAmmoAmount_Image.fillAmount = GameManager.Instance.RemainAmmo_Amount / GameManager.Instance.maxAmmo_Amount;

        if(RemainedCartridgeCount_Text != null )
        RemainedCartridgeCount_Text.text = "X " + GameManager.Instance.RemainedCartridgeCount.ToString();
    }
    public void CallScene(string wantedScene)
    {
        LoadingScene.LoadScene(wantedScene);
        GameManager.Instance.ResetPlayerStats();
        GameManager.Instance.current_SceneName = wantedScene;
        if (wantedScene.Contains("Title"))
            GameManager.Instance.SavePosition.current_SavePoint_Index = 0;
    }

    public void UnPause()
    {
        GameManager.Instance.UnPause();
    }

    public void Quit()
    {
        Application.Quit();
    }
}
