using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HammerWeapon : WeaponObject
{
    [SerializeField] Transform attackPoint;
    [SerializeField, Range(0, 10)] float attackRadius = 1.5f;
    [SerializeField, Range(0, 10)] float coolTime = 0.8f;
    [SerializeField] LayerMask whatIsPlayer;

    float attackTime = 0;
    readonly int attackAnimHash = Animator.StringToHash("isAttack");
    float animTime = 0;

    public override void OnNetworkSpawn()
    {
        print("OnNetworkSpawn");
        base.OnNetworkSpawn();

        if (!IsOwner) return;
        print("Hammer MouseLeftEvent Init");
        input.MouseLeftEvent += OnLeftMouse;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
    
        if (!IsOwner) return;
        input.MouseLeftEvent -= OnLeftMouse;
    }

    private void OnLeftMouse(bool down)
    {
        if (!gameObject.activeInHierarchy) return;
        print("Hammer OnLeftMouse " + down);
        if (!down || Time.time - attackTime < coolTime) return;
        attackTime = Time.time;

        animator.SetBool(attackAnimHash, true);
        animTime = 0.5f;
    }

    private void Update() {
        if (animTime > 0) {
            animTime -=Time.deltaTime;
            
            if (animTime <= 0)
                animator.SetBool(attackAnimHash, false);
        }
    }

    public override bool OnDamaged()
    {
        return false; // 근데 뿅망치 들었는데 왜 데미지 받음??
    }

    Collider[] detectedPlayers = new Collider[2];
    public override void OnAttack()
    {
        int count = Physics.OverlapSphereNonAlloc(attackPoint.position, attackRadius, detectedPlayers, whatIsPlayer);
        print("Overlap " + count);
        if (count == 0) return;

        PlayerWeapon targetWeapon = null;
        for (int i = 0; i < 2; i++)
        {
            if (detectedPlayers[i]?.gameObject != transform.root.gameObject) {
                targetWeapon = detectedPlayers[i].GetComponent<PlayerWeapon>();
                break;
            }
        }

        // if (targetWeapon == null) return;
        targetWeapon?.HitDamageServerRpc();
    }
    
    private void OnDrawGizmos() {
        if (!attackPoint) return;
        
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        Gizmos.color = Color.white;
    }

    public override void Hide()
    {
        
    }
}
