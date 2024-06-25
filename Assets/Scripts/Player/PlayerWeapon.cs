using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public enum WeaponType : ushort {
    None,
    Hammer,
    Pot
}

[System.Serializable]
public struct WeaponData {
    public WeaponType type;
    public GameObject entity;
}

public class PlayerWeapon : NetworkBehaviour
{
    [SerializeField] WeaponData[] configWeapons;
    [SerializeField] bool animWeapon;
    [SerializeField] Transform handTrm;
    [SerializeField] GameObject attackEffect;

    Dictionary<WeaponType, GameObject> weaponList;

    NetworkVariable<WeaponType> currentWeapon;

    Animator animator;
    int topLayerIndex = 0;
    
    private void Awake() {
        currentWeapon = new(WeaponType.None);
        
        animator = transform.Find("visual").GetComponent<Animator>();
        topLayerIndex = animator.GetLayerIndex("Top Layer");

        weaponList = new();
        foreach (var item in configWeapons)
            weaponList[item.type] = item.entity;
    }

    public override void OnNetworkSpawn()
    {
        // if (!IsHost) return;
        currentWeapon.OnValueChanged += HandleChangeWeapon;
        HandleChangeWeapon(WeaponType.None, currentWeapon.Value);

        // ㄹㅇ 테스트 코드
        // if (IsHost)
        //     Invoke(nameof(TestCode), 1f);
    }

    public void SetWeapon(WeaponType type) {
        if (!IsServer) {
            throw new Exception("님아. SetWeapon은 서버에서 실행해야 함니다.");
        }

        currentWeapon.Value = type;
    }

    public WeaponType GetCurrentWeapon() => currentWeapon.Value;

    void TestCode() {
        currentWeapon.Value = IsOwner ? WeaponType.Hammer : WeaponType.Pot;
        // currentWeapon.Value = WeaponType.Pot;
    }
    
    public override void OnNetworkDespawn()
    {
        // if (!IsHost) return;
        currentWeapon.OnValueChanged -= HandleChangeWeapon;
    }

    private void Start() {
        // TEST
        
    }

    private void HandleChangeWeapon(WeaponType previousValue, WeaponType newValue)
    {
        print("HandleChangeWeapon / IsOwner: " + IsOwner + " / " + newValue);

        animWeapon = newValue != WeaponType.None;

        if (previousValue != WeaponType.None && previousValue != newValue) {
            weaponList[previousValue].GetComponent<WeaponObject>().Hide();
        }
        
        if (newValue == WeaponType.None) { // 클리어
            if (previousValue != WeaponType.None) {
                foreach (var item in weaponList)
                    item.Value.SetActive(false);
            }
                
            return;
        }

        weaponList[newValue].SetActive(true); // 밍
    }

    private void Update() {
        animator.SetLayerWeight(topLayerIndex, Mathf.Lerp(animator.GetLayerWeight(topLayerIndex), animWeapon ? 1 : 0, Time.deltaTime * 20));
    }

    // public void HitDamage() {
    //     print("HitDamage Checking.. Host: " + IsHost);
    //     if (currentWeapon.Value != WeaponType.None) {
    //         bool result = weaponList[currentWeapon.Value].GetComponent<WeaponObject>().OnDamaged();
            
    //         if (result) return; // 막음 ㄷㄷㄷ
    //     }

    //     // 걍 뚫림
    //     print("Hit!!!! / Host: " + IsHost);
    // }

    [ServerRpc(RequireOwnership = false)]
    public void HitDamageServerRpc() {
        print("HitDamage Checking.. Host: " + IsHost);
        if (currentWeapon.Value != WeaponType.None) {
            bool result = weaponList[currentWeapon.Value].GetComponent<WeaponObject>().OnDamaged();
            
            if (result) return; // 막음 ㄷㄷㄷ
        }

        // 걍 뚫림
        print("Hit!!!! / Host: " + IsHost);
        CompressPlayerClientRpc(); // 전부다 찌부
        IngameManager.Instance.PlayerAttack(OwnerClientId);

        {
            // 다른 플레이어 찾기
            Transform attacker = null;
            foreach (var item in IngameManager.Instance.GetPlayers())
            {
                if (OwnerClientId != item.Key) {
                    attacker = item.Value.transform;
                    break;
                }
            }
            if (attacker != null) {
                ExplodeEffectClientRpc(attacker.forward);
            }
        }
        
        
    }

    [ClientRpc]
    void CompressPlayerClientRpc() {
        transform.localScale = new Vector3(1, 0.2f, 1);
    }

    [ClientRpc]
    public void RestorePlayerClientRpc() {
        transform.localScale = new Vector3(1, 1, 1);
    }

    [ClientRpc]
    void ExplodeEffectClientRpc(Vector3 dir) {
        var effect = Instantiate(attackEffect, transform.position, Quaternion.identity);
        effect.transform.forward = dir;
    }
    
    [ClientRpc]
    public void ShowResultClientRpc(bool draw, ulong loseID) {
        if (draw) {
            IngameManager.Instance.resultTextUI.Show(ResultType.Draw);
            return;
        }

        IngameManager.Instance.resultTextUI.Show(loseID == NetworkManager.Singleton.LocalClientId ? ResultType.Lose : ResultType.Win);
    }
    
    public void OnAttack() {
        if (!IsOwner) return;

        if (currentWeapon.Value != WeaponType.None)
            weaponList[currentWeapon.Value].GetComponent<WeaponObject>().OnAttack();
    }

    public bool CurrentWeaponObject(GameObject entity) {
        if (!weaponList.TryGetValue(currentWeapon.Value, out var target)) return false;
        return target == entity;
    }    
}
