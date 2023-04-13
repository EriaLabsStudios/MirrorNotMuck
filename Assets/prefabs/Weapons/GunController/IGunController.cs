using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGunController
{
    void shootEvent();
    void shootProjectile();
    bool canShoot();
    void reload();
    void equip();
    void unEquip();
    bool isReloading();
}