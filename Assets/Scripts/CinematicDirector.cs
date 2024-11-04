using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CinematicDirector : MonoBehaviour
{
    public Camera MainCamera;
    public CinemachineDollyCart CDCart;
    public CinemachineSmoothPath CTrack;
    public CinemachineVirtualCamera VCamera;
    bool watched = false;   

    private void Start()
    {
        CDCart = FindObjectOfType<CinemachineDollyCart>();
        CTrack = FindObjectOfType<CinemachineSmoothPath>();
        VCamera = FindObjectOfType<CinemachineVirtualCamera>();
    }

    public void StartCinematic()
    {
        GameManager.Instance.IsCinemachining = true;
        CDCart.transform.position = CTrack.m_Waypoints[0].position;
        VCamera.transform.position = CDCart.transform.position;
    }

    public void EndCinematic()
    {
        MainCamera.transform.localPosition = Vector3.zero;
        VCamera.gameObject.SetActive(false);
        GameManager.Instance.IsCinemachining = false;
    }

    void Update()
    {
        if((CDCart.m_Position >= CTrack.PathLength||(Input.GetKeyDown(KeyCode.Space)&&GameManager.Instance.IsCinemachining)||!GameManager.Instance.IsCinemachining) && !watched)
        {
            EndCinematic();
            watched = true;
        }
    }
}
