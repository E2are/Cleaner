using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponAnim : MonoBehaviour
{
    public BubbleGun BG;
    public Mop MP;
    public BathBomb BB;

    public void WeaponFullyHolded()
    {
        GetComponent<Animator>().SetBool("WeaponIsHolded",true);
    }
    public void ReloadStart()
    {
        BG.shootAble = false;
        GameManager.Instance.RemainedCartridgeCount--;
    }

    public void ReloadEnd()
    {
        GameManager.Instance.RemainAmmo_Amount = GameManager.Instance.maxAmmo_Amount;
        BG.shootAble = true;
        BG.reloading = false;
    }

    public void Shoot() {
        BG.shootCheck();
    }

    public void Throw()
    {
        BB.Throwing();
    }

}
