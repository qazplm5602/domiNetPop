using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerCamera : NetworkBehaviour
{
    [SerializeField] float speed = 10;
    [SerializeField] Transform camPoint;

    PlayerInput _input;
    
    private void Awake() {
        _input = GetComponent<PlayerInput>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        CamManager.Instance.SetFollowPlayerCam(transform.Find("CamPoint"));
    }

    private void Update() {
        if (!IsOwner) return;

        UpdateVertical();
        UpdateHorizontal();
    }

    void UpdateHorizontal() {
        float domi = _input.MouseInput.x * Time.deltaTime * speed;
        transform.Rotate(new Vector3(0, domi, 0));
    }

    float currentX = 0;
    void UpdateVertical() {
        currentX -= _input.MouseInput.y * Time.deltaTime * speed;
        currentX = Mathf.Clamp(currentX, -40, 70); // 제한

        camPoint.localRotation = Quaternion.Euler(new Vector3(currentX, 0, 0));
    }
}
