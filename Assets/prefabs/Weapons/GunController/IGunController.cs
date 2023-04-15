using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGunController
{
    void ShootEvent();
    void ShootProjectile();
    bool CanShoot();
    IEnumerator Reload();
    void Equip();
    void UnEquip();
    bool IsReloading();
}