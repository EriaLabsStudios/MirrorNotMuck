using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Gun", menuName = "Weapon/Gun")]
public class GunData : ScriptableObject
{
    [Header("Info")]
    public new string name;
    [Header("Shooting")]
    public float damage;
    public float maxDistance;
    public float fireRate;
    [Header("Reloading")]
    public float reloadTime;
    public int currentAmmo;
    public int magazineSize;
    public bool reloading;
}
