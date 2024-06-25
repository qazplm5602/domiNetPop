using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CamManager : MonoBehaviour
{
    private static CamManager instance;
    public static CamManager Instance
    {
        get
        {
            if (instance != null) return instance;
            instance = FindObjectOfType<CamManager>();

            if (instance == null)
            {
                Debug.LogError("No cam manager singleton");
            }
            return instance;
        }
    }

    [SerializeField] CinemachineVirtualCamera playerCam;

    public void SetFollowPlayerCam(Transform follow) {
        playerCam.Follow = follow;
    }
}
