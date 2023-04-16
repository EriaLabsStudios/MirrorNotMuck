using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class GunController : NetworkBehaviour, IGunController
{
    [SerializeField]
    private int maxAmmo, currentAmmo, magazineSize;
    [SerializeField]
    private float range, damage, fireRate, reloadingTime, reloadTime;
    [SerializeField]
    private Transform firingPoint;

    [SerializeField]
    PlayerControllerNet playerOwner;
    private float timeSinceLastShoot;
    private bool reloading;


    //Synced vars
    [SyncVar]
    public bool isEquiped = false;

    //Client vars
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField]

    Animator animator;
    [SerializeField]
    Vector3 viewPortPosition, viewPortRotation;
    [SerializeField]
    /*
     *  0 - GunShoot
     *  1 - DryShoot
     *  3 - Reload
     */
    AudioClip[] sounds;
    AudioSource audioSource;


    void Start()
    {
        Debug.Log("Gun::eventHandler");
        if (isClient) {
            PlayerShoot.shootInput += ShootEvent;
            PlayerShoot.reloadInput += StartReload;
            animator = GetComponent<Animator>();
            audioSource = GetComponent<AudioSource>();
        }

    }
    private void Update()
    {
        timeSinceLastShoot += Time.deltaTime;
        if (isClient && Input.GetKey(KeyCode.E))
        {
            if (!isEquiped) Equip();
        }

    }

    public bool CanShoot()
    {
        Debug.Log($"[CanShoot] reloading {reloading} and timeSinceLastShoot {timeSinceLastShoot} isOwwned {isOwned} isEquiped {isEquiped}");
        return !reloading && timeSinceLastShoot > 1f / (fireRate / 60) && isEquiped;
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
            Debug.Log("[Server][GunController] CanShoot " + CanShoot());
            if (CanShoot())
            {
                RpcPlaySound(0);
                Debug.Log($"[Server][GunController] shoot");
                currentAmmo--;
                RpcVFXShoot();
                RpcUpdateAmmoUI(currentAmmo);
                ShootProjectile();
                timeSinceLastShoot = 0;
                timeSinceLastShoot -= Time.deltaTime;

            }
        }
        else RpcPlaySound(1);

        RpcUpdateAmmoUI(currentAmmo);
    }

    [Command(requiresAuthority = false)]
    void CmdAssignWeapon(NetworkIdentity player)
    { 

        Debug.Log("[Server][GunController] CmdAssignWeapon " + player);
        GetComponent<NetworkIdentity>().RemoveClientAuthority();
        GetComponent<NetworkIdentity>().AssignClientAuthority(player.connectionToClient);
        isEquiped = true;
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
        Debug.DrawRay(firingPoint.position, firingPoint.forward * range,Color.red,500);
    }


    [Command(requiresAuthority = false)]
    public  void CmdsetServerParent(Transform parent)
    {
        Debug.Log("[Server][GunController ] CmdsetServerParent " + parent.name);
        transform.SetParent(parent);
        transform.localPosition = viewPortPosition;
        transform.localRotation = Quaternion.Euler(viewPortRotation);
    }



    /*
     * Client functions
     */
    [Client]
    public void ShootEvent()
    {
        Debug.Log($"[Client][GunController] shootEvent " + CanShoot()); ;
        if (CanShoot() && isOwned)
        {
            Debug.Log($"[Client][GunController] CanShoot true");
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
        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }
    }
    [ClientRpc]
    public void RpcPlaySound(int sound)
    {
        Debug.Log("Play sound " + sound);
        PlaySound(sound);
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
        if (!reloading && isOwned && isEquiped)
        {
            PlaySound(2);
            RpcSetAnimTrigger("Reload");
            StartCoroutine(Reload());
        }
    }
 
    public void Equip()
    {
        Transform localPlayer = GameObject.Find("LocalPlayer").transform.GetChild(0);


        if (Vector3.Distance(localPlayer.transform.position,transform.position) < 1)
        {
            CmdAssignWeapon(localPlayer.GetComponent<NetworkIdentity>());
            GetComponent<NetworkIdentity>().RemoveClientAuthority();
            GetComponent<NetworkIdentity>().AssignClientAuthority(localPlayer.GetComponent<NetworkIdentity>().connectionToClient);
            playerOwner = localPlayer.GetComponent<PlayerControllerNet>();

            Debug.Log("[EQUIP][Client] " + localPlayer);
            transform.SetParent(localPlayer.transform.GetChild(0).GetChild(0));
            transform.localPosition = viewPortPosition;
            transform.localRotation = Quaternion.Euler(viewPortRotation);

            CmdsetServerParent(localPlayer.transform);

            calculateFiringPoint();
        }
    }

    [Command]
    void CmdSetFiringPoint(Vector3 pos)
    {
        firingPoint.transform.position = pos;
    }

    void calculateFiringPoint()
    {

        int screenWidth = Screen.width;
        int screenHeight = Screen.height;

        Vector2 centerScreenPosition = new Vector2(screenWidth / 2, screenHeight / 2);
        Vector3 centerWorldPosition = playerOwner.mainCamera.ScreenToWorldPoint(new Vector3(centerScreenPosition.x, centerScreenPosition.y, playerOwner.mainCamera.nearClipPlane));
        firingPoint.transform.position = centerWorldPosition + firingPoint.forward;

        Debug.DrawRay(centerWorldPosition, firingPoint.forward * range, Color.green, 500);

        CmdSetFiringPoint(firingPoint.transform.position);
    }


    public void UnEquip()
    {
        throw new System.NotImplementedException();
    }

    public void PlaySound(int sound)
    {
        Debug.Log("sounds[sound] " + sounds.Length);
        audioSource.PlayOneShot(sounds[sound]);
    }





}
