using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerAim : NetworkBehaviour
{
    [SerializeField] LayerMask detectLayer;
    [SerializeField, Range(1, 10)] float distance = 1;
    
    PlayerWeapon weapon;
    PlayerInput input;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        input = GetComponent<PlayerInput>();
        weapon = GetComponent<PlayerWeapon>();
        
        if (!IsOwner) return;
        input.InteractionEvent += OnPressE;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        
        if (!IsOwner) return;
        input.InteractionEvent += OnPressE;
    }

    private void OnPressE()
    {
        print("OnPressE");
        Vector3 centerPos = new Vector3(Camera.main.rect.width, Camera.main.rect.height, 0) / 2f;
        Ray ray = Camera.main.ViewportPointToRay(centerPos);
        
        if (!Physics.Raycast(ray, out var hitInfo, distance, detectLayer)) return;
        print("OnPressE Hit!!");
        
        DecorObject decor = hitInfo.collider?.GetComponent<DecorObject>();
        if (decor == null) return;
        
        print("OnPressE GettingWeapon Send " + decor.GetWeaponType());
        // IngameManager.Instance.GettingWeaponServerRpc(decor.GetWeaponType(), NetworkManager.Singleton.LocalClientId);
        PickWeaponServerRpc(decor.GetWeaponType());
    }

    private void Update() {
        Vector3 centerPos = new Vector3(Camera.main.rect.width, Camera.main.rect.height, 0) / 2f;
        Ray ray = Camera.main.ViewportPointToRay(centerPos);
        
        // Physics.Raycast(ray, distance, detectLayer);
        
        
        Debug.DrawRay(ray.origin, ray.direction * distance, Color.red);
    }

    [ServerRpc]
    void PickWeaponServerRpc(WeaponType type) {
        IngameManager.Instance.GettingWeapon(type, weapon);
    }
}
