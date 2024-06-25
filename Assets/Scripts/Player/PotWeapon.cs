using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PotWeapon : WeaponObject
{
    [SerializeField] GameObject explodeEffect;
    [SerializeField] float activeTime = 1;
    [SerializeField] float coolTime = 1;
    NetworkVariable<bool> isProtected;
    readonly int blockAnimationHash = Animator.StringToHash("isBlock");

    float saveTime = 0;
    float saveTime2 = -999;


    private void Awake() {
        isProtected = new(false);
    }

    public override void OnNetworkSpawn()
    {
        print("PotWeapon OnNetworkSpawn");
        base.OnNetworkSpawn();

        if (!IsOwner) return;
        print("PotWeapon MouseLeftEvent Init");
        isProtected.OnValueChanged += HandleProtectChange;
        input.MouseLeftEvent += OnMouseLeft;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (!IsOwner) return;
        input.MouseLeftEvent -= OnMouseLeft;
        isProtected.OnValueChanged -= HandleProtectChange;
    }

    private void HandleProtectChange(bool previousValue, bool newValue)
    {
        animator.SetBool(blockAnimationHash, newValue);
    }

    private void OnMouseLeft(bool down)
    {
        if (!gameObject.activeInHierarchy) return;
        if ((down && isProtected.Value) || (Time.time - saveTime) < coolTime) return;

        if (down) {
            SetProtectServerRpc(true);
            saveTime2 = activeTime;
        } else {
            saveTime = Time.time;
            saveTime2 = -999;
            SetProtectServerRpc(false);
        }
    }

    private void Update() {
        if (saveTime2 == -999) return;

        saveTime2 -= Time.deltaTime;
        if (saveTime2 < 0) {
            saveTime2 = -999;
            saveTime = Time.time;
            SetProtectServerRpc(false);
;        }
    }

    [ServerRpc]
    void SetProtectServerRpc(bool value) {
        isProtected.Value = value;
    }

    public override void OnAttack()
    {
        // 이걸로 어케 때리노;;
    }

    public override bool OnDamaged()
    {
        if (isProtected.Value) {
            SpawnExplodeEffectClientRpc(); // 서버가 실행하는거임
            return true;
        }
        
        // 좀있다.
        return false;
    }

    [ClientRpc]
    void SpawnExplodeEffectClientRpc() {
        var effect = Instantiate(explodeEffect, transform.root.position + (Vector3.up * 1f), Quaternion.identity);
        effect.transform.forward = -transform.root.forward;
    }

    public override void Hide()
    {
        isProtected.Value = false;
    }
}
