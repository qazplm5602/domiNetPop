using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public abstract class WeaponObject : NetworkBehaviour
{
    protected PlayerInput input;
    protected PlayerWeapon weapon;
    protected Animator animator;
    
    public override void OnNetworkSpawn()
    {
        print("OnNetworkSpawn()");

        print(transform.root);
        print(transform.root.Find("visual"));

        input = transform.root.GetComponent<PlayerInput>();
        animator = transform.root.Find("visual").GetComponent<Animator>();
        weapon = transform.root.GetComponent<PlayerWeapon>();

        if (!weapon.CurrentWeaponObject(gameObject)) {
            // ㄹㅇ 초기화
            gameObject.SetActive(false);
        }
    }

    public override void OnNetworkDespawn()
    {
        
    }

    public abstract bool OnDamaged(); // true 막음

    public abstract void OnAttack();

    public abstract void Hide(); // 무기 내려놩
}
