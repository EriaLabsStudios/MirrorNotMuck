using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class GunController : NetworkBehaviour, IGunController
{
    [SerializeField]
    private int maxAmmo, currentAmmo, magazineSize;
    [SerializeField]
    private float range, damage, fireRate, reloadTime;
    private float reloadingTime;

    [SerializeField] private GameObject trailObject;
    [SerializeField]
    PlayerControllerNet playerOwner;
    private float timeSinceLastShoot;
    private bool reloading;
    
    public enum WeaponType
        {
            SemiAutomatic,
            Automatic,
            BoltAction
        }
    
    [SerializeField] private WeaponType weaponType;
    
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
            if(weaponType==WeaponType.SemiAutomatic) PlayerShoot.singleShootInput += ShootEvent;
            if(weaponType==WeaponType.Automatic) PlayerShoot.autoShootInput += ShootEvent;
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
    public void CmdReload()
    {
        currentAmmo = magazineSize;
    }

    [Command]
    public void CmdSyncAnimation(string anim)
    {
        RpcSetAnimTrigger(anim);
    }
 



    [Command]
    public void CmdshootEventServer(Vector3 origin, Vector3 direction)
    {
        Debug.Log($"[Server][GunController] CmdshootEventServer {origin}");

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
                ShootProjectile(origin,direction);
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
    public void ShootProjectile(Vector3 origin, Vector3 direction)
    {
        Vector3 endPoint = default;
        if (Physics.Raycast(origin, direction, out RaycastHit hitInfo, range))
        {
            endPoint = hitInfo.point;
            Debug.Log($"Gun:Shoot - Raycast trasform collided:  {hitInfo.transform.name}");
            if (hitInfo.transform.gameObject.CompareTag("Enemy"))
            {
                if (hitInfo.transform.gameObject == null) return;
                IDamageable damageable = hitInfo.transform.gameObject.transform.GetComponent<IDamageable>();
                if (damageable == null) return;
                damageable?.Damage(damage, playerOwner);
                endPoint = hitInfo.point;
            }
        }
        else
        {
            endPoint = origin + direction * range;
        }
            
        // Instancia el objeto TrailRenderer
        GameObject trailInstance = Instantiate(trailObject, origin, Quaternion.identity);
        TrailRenderer trailRenderer = trailInstance.GetComponent<TrailRenderer>();

        // Inicia la Coroutine para mover el objeto a lo largo de la trayectoria del raycast
        StartCoroutine(MoveTrail(trailInstance, endPoint, 0.5f));
        Debug.DrawRay(origin, direction * range,Color.red,500);
    }
    [Server]
    private IEnumerator MoveTrail(GameObject trailInstance, Vector3 targetPosition, float duration)
    {
        float elapsedTime = 0f;
        Vector3 startPosition = trailInstance.transform.position;

        while (elapsedTime < duration)
        {
            trailInstance.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Destruye el objeto TrailRenderer después de un tiempo para que no se acumulen
        Destroy(trailInstance, 1f);
    }

    [Command(requiresAuthority = false)]
    public  void CmdsetServerParent(Transform parent)
    {
        Debug.Log("[Server][GunController ] CmdsetServerParent " + parent.name);
        transform.SetParent(parent.GetChild(0).GetChild(0));
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
            CmdSyncAnimation("Shoot");
            Ray ray = playerOwner.mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
      
            CmdshootEventServer(ray.origin, ray.direction);
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
        CmdSyncAnimation("Reload");
        reloading = true;
        yield return new WaitForSeconds(reloadTime);
        CmdReload();
        reloading = false;
    }
    [Client]
    public void StartReload()
    {
        if (!reloading && isOwned && isEquiped)
        {
            PlaySound(2);
         //   RpcSetAnimTrigger("Reload"); No se puede llamar a RPC desde el cliente
            StartCoroutine(Reload());
        }
    }
 
    public void Equip()
    {
        Transform localPlayer = GameObject.Find("LocalPlayer").transform.GetChild(0);


        if (Vector3.Distance(localPlayer.transform.position,transform.position) < 1)
        {
            CmdAssignWeapon(localPlayer.GetComponent<NetworkIdentity>());
            playerOwner = localPlayer.GetComponent<PlayerControllerNet>();

            Debug.Log("[EQUIP][Client] " + localPlayer);
            transform.SetParent(localPlayer.transform.GetChild(0).GetChild(0));
            transform.localPosition = viewPortPosition;
            transform.localRotation = Quaternion.Euler(viewPortRotation);

            CmdsetServerParent(localPlayer.transform);
        }
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
