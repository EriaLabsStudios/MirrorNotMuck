using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class GunController : NetworkBehaviour,IGunController
{
    [SerializeField]
    private int maxAmmo, currentAmmo, magazineSize;
    [SerializeField]
    private float range,damage,fireRate,reloadingTime, reloadTime;
    [SerializeField]
    private Transform firingPoint;



    [SerializeField]
    PlayerControllerNet playerOwner;
    private float timeSinceLastShoot;
    private bool reloading;


    //Synced vars
    public bool isEquiped = false;

    //Client vars
    [SerializeField]
    Animator animator;

    void Start()
    {
        Debug.Log("Gun::eventHandler");
        if (isClient) {
            PlayerShoot.shootInput += ShootEvent;
            PlayerShoot.reloadInput += StartReload;
        }
 
    }
    private void Update()
    {
        timeSinceLastShoot += Time.deltaTime;

    }

    public bool CanShoot()
    {
       return !reloading && timeSinceLastShoot > 1f / (fireRate / 60) && isOwned;
    }


    public bool IsReloading()
    {
        throw new System.NotImplementedException();
    }



    [Command]
    public void CmdshootEventServer()
    {
        Debug.Log($"[Server][GunController] CmdshootEventServer");
     
        if (currentAmmo > 0)
        {
            if (CanShoot())
            {
                Debug.Log($"[Server][GunController] shoot");
                currentAmmo--;
                RpcVFXShoot();
                RpcUpdateAmmoUI(currentAmmo);
         
                timeSinceLastShoot = 0;
                timeSinceLastShoot -= Time.deltaTime;

            }
        }

        RpcUpdateAmmoUI(currentAmmo);
    }

    [Server]
    public void ShootProjectile()
    {
        if (Physics.Raycast(firingPoint.position, firingPoint.forward, out RaycastHit hitInfo, range))
        {
            Debug.Log($"Gun:Shoot - Raycast trasform collided:  {hitInfo.transform.name}");
            if (hitInfo.transform.gameObject.CompareTag("Enemy"))
            {
                if (hitInfo.transform.gameObject == null) return;
                IDamageable damageable = hitInfo.transform.gameObject.transform.GetComponent<IDamageable>();
                if (damageable == null) return;
               
                damageable?.Damage(damage, playerOwner);
            }
       
        }
    }





    /*
     * Client functions
     */
    [Client]
    public void ShootEvent()
    {
        Debug.Log($"[Client][GunController] shootEvent");
        if (CanShoot())
        {
            CmdshootEventServer();
            currentAmmo--;
            timeSinceLastShoot = 0;
        }
    }

    [ClientRpc]
    public void RpcUpdateAmmoUI(int ammoLeft)
    {

    }
    [ClientRpc]
    public void RpcVFXShoot()
    {

    }
    [ClientRpc]
    public void RpcPlaySound(int sound)
    {

    }
    [ClientRpc]
    public void RpcSetAnimTrigger(string anim)
    {
        animator.SetTrigger(anim);
    }

    [Client]
    public IEnumerator Reload()
    {
        reloading = true;
        yield return new WaitForSeconds(reloadTime);
        currentAmmo = magazineSize;
        reloading = false;
    }
    [Client]
    public void StartReload()
    {
        if (!reloading)
        {
            RpcPlaySound(2);
            RpcSetAnimTrigger("Reload");
            StartCoroutine(Reload());
        }
    }
    [Client]
    public void Equip()
    {
        GameObject localPlayer = GameObject.Find("LocalPlayer").transform.GetChild(0).gameObject;
        if (!isEquiped && Input.GetKey(KeyCode.E) && Vector3.Distance(localPlayer.transform.position,transform.position) < 1)
        {
            GetComponent<NetworkIdentity>().AssignClientAuthority(connectionToClient);
            playerOwner = localPlayer.GetComponent<PlayerControllerNet>();
            transform.SetParent(localPlayer.transform.Find("WeaponHolder").transform);

        }
    }


    public void UnEquip()
    {
        throw new System.NotImplementedException();
    }





}
