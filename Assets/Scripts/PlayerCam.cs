using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class PlayerCam : MonoBehaviour
{

    public float sensitive = 1;
    [HideInInspector]
    public float mouseX;
    [HideInInspector]
    public float mouseY;

    public Transform Orientation;

    public Transform CameraHolder;

    public Camera MiniMapCamera;
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!GameManager.Instance.Paused && !GameManager.Instance.IsCinemachining && !GameManager.Instance.Dead)
        {
            MouseMove();
        }
        else if (GameManager.Instance.Dead)
        {
            mouseY = Mathf.Clamp(mouseY, -85f, 90f);
            CameraHolder.rotation = Quaternion.Euler(mouseY--, mouseX, 0);
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            DoShake();
        }

        transform.localPosition = Vector3.Lerp(transform.localPosition, Vector3.zero, Time.deltaTime * 2f);
        
        MiniMapCamera.transform.position = new Vector3(Orientation.position.x,3.5f,Orientation.position.z);

        MiniMapCamera.transform.rotation = Quaternion.Euler(90, mouseX, 0);
    }

    void MouseMove()
    {
        mouseX += Input.GetAxis("Mouse X") * 0.1f * sensitive;
        mouseY += Input.GetAxis("Mouse Y") * 0.1f * sensitive;
        mouseY = Mathf.Clamp(mouseY, -85f, 85f);
        CameraHolder.rotation = Quaternion.Euler(-mouseY, mouseX, 0);
        Orientation.rotation = Quaternion.Euler(0, mouseX, 0);
    }

    public void DoFov(float endValue)
    {
        if(!GameManager.Instance.Dead)
        GetComponent<Camera>().DOFieldOfView(endValue, 0.25f);
    }

    public void DoTilt(float zTilt)
    {
        if(!GameManager.Instance.Dead)
        transform.DOLocalRotate(new Vector3(0, 0, zTilt), 0.25f);
    }

    public void DoShake(float shakeAmount = 0.1f)
    {
        if(!GameManager.Instance.Dead)
        GetComponent<Camera>().DOShakePosition(1, shakeAmount, 10,90,false,ShakeRandomnessMode.Full);

        transform.localPosition = Vector3.ClampMagnitude(transform.localPosition, 0.1f);
    }
}
