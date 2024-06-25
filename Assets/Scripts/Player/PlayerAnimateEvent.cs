using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimateEvent : MonoBehaviour
{
    PlayerWeapon weapon;
    private void Awake() {
        weapon = transform.root.GetComponent<PlayerWeapon>();
    }

    public void AttackTrigger() {
        weapon.OnAttack();
    }
}
