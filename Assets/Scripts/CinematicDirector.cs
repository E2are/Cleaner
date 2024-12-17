using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CinematicDirector : MonoBehaviour
{
    public static CinematicDirector Instance;
    public Camera MainCamera;
    public CinemachineDollyCart CDCart;
    public CinemachineSmoothPath CTrack;
    public CinemachineVirtualCamera VCamera;
    bool watched = false;

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
        if (CDCart == null)
        {
            CDCart = FindObjectOfType<CinemachineDollyCart>();
        }

        if(CTrack == null)
        CTrack = FindObjectOfType<CinemachineSmoothPath>();

        if(VCamera == null)
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
        if(GameManager.Instance.Monsters != null)
        foreach(IMonster monster in GameManager.Instance.Monsters)
        {
            if(monster != null)
            {
                monster.TargetSet(GameManager.Instance.PM.orientation);
            }
        }
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
