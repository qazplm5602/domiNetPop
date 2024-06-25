using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecorObject : MonoBehaviour
{
    [SerializeField] WeaponType type;

    public WeaponType GetWeaponType() => type;
}
