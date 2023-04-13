using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class GunController : NetworkBehaviour,IGunController
{

    private int maxAmmo;
    private int currentAmmo;
    private float range;
    private float damage;
    private float fireRate;
    private float reloadingTime;

    private Transform firingPoint;



    void Start()
    {
        Debug.Log("Gun::eventHandler");
        PlayerShoot.shootInput += shootEvent;
        PlayerShoot.reloadInput += reload;
    }
    public bool canShoot()
    {
        throw new System.NotImplementedException();
    }

    public void equip()
    {
        throw new System.NotImplementedException();
    }

    public bool isReloading()
    {
        throw new System.NotImplementedException();
    }

    public void reload()
    {
        throw new System.NotImplementedException();
    }

    [Client]
    public void shootEvent()
    {
        throw new System.NotImplementedException();
    }

    [Command]
    public void CmdshootEventServer()
    {
        throw new System.NotImplementedException();
    }


    public void shootProjectile()
    {
        throw new System.NotImplementedException();
    }

    public void unEquip()
    {
        throw new System.NotImplementedException();
    }


}
