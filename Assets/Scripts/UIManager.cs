using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    BubbleGun BG;

    public Slider HP;
    public Image HitedImage;
    public Image RemainedAmmoAmount_Image;
    public TMP_Text RemainedCartridgeCount_Text;

    private void Start()
    {
        BG = FindObjectOfType<BubbleGun>();
        HP = GameObject.Find("HP").GetComponent<Slider>();
        if(GameObject.Find("HitImage")!=null)
            HitedImage = GameObject.Find("HitImage").GetComponent<Image>();
        RemainedAmmoAmount_Image = GameObject.Find("CurrentAmmoAmount").GetComponent<Image>();
        RemainedCartridgeCount_Text = GameObject.Find("CurrentCnt").GetComponent<TMP_Text>();
    }

    public void Update()
    {
        HP.value = GameManager.Instance.HP / GameManager.Instance.MaxHP;
        if(HitedImage != null )
        HitedImage.material.SetFloat("_Desintegration_Value_1", GameManager.Instance.hitAlpha);

        RemainedAmmoAmount_Image.fillAmount = GameManager.Instance.RemainAmmo_Amount / GameManager.Instance.maxAmmo_Amount;

        RemainedCartridgeCount_Text.text = "X " + GameManager.Instance.RemainedCartridgeCount.ToString();
    }
}
