using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{

    [Header("references")]
    [SerializeField] private GunData gunData;
    [SerializeField] private PlayerControllerNet player;
    [SerializeField] private Transform firePoint;
    [Header("Sound")]
    [SerializeField] private AudioClip fireSound;
    [SerializeField] private AudioClip dryShootSound;
    [SerializeField] private AudioClip reloadSound;
    [SerializeField] private float fireSoundVolume = 1.0f;
    private AudioSource audioSource;
    [Header("Animation")]
    [SerializeField] private ParticleSystem muzzleFlash;
    public Animator animator;
    private PlayerUIController playerUIController;
    private PlayerControllerNet playerLocalController;
    
    private float timeSinceLastShoot = 0;
    // Start is called before the first frame update
    void Start()
    {
        playerUIController = FindObjectOfType<PlayerUIController>();
        playerLocalController = gameObject.transform.parent.parent.parent.GetComponent<PlayerControllerNet>();
        playerUIController.UpdateBullets(gunData.currentAmmo);
        eventHandler();
        animator = GetComponent<Animator>();
        audioSource = gameObject.AddComponent<AudioSource>();
    }
    
    private void eventHandler()
    {
        Debug.Log("Gun::eventHandler");
        PlayerShoot.singleShootInput += Shoot;
        PlayerShoot.reloadInput += StartReload;
    }

    public void StartReload()
    {
        if (!gunData.reloading)
        {
            
            PlaySound(reloadSound);
            animator.SetTrigger("Reload");
            StartCoroutine(Reload());
        }
    }

    private IEnumerator Reload()
    {
        gunData.reloading = true;
        yield return new WaitForSeconds(gunData.reloadTime);
        gunData.currentAmmo = gunData.magazineSize;
        gunData.reloading = false;
    }

    private bool CanShoot() => !gunData.reloading && timeSinceLastShoot > 1f / (gunData.fireRate / 60);
    
    public void Shoot()
    {
        
        Debug.Log($"Gun:Shoot Pre Pew");
        playerUIController.UpdateBullets(gunData.currentAmmo);
        if (gunData.currentAmmo > 0)
        {
            if (CanShoot())
            {
                Debug.Log($"Gun:Shoot Pew");
                PlaySound(fireSound);
                PlayMuzzleFlash();
                animator.SetTrigger("Shoot");
                if (Physics.Raycast(firePoint.position, firePoint.forward, out RaycastHit hitInfo, gunData.maxDistance))
                {
                    Debug.Log($"Gun:Shoot - Raycast trasform collided:  {hitInfo.transform.name}");
                    playerLocalController.CmdShootEnemy(hitInfo.transform.gameObject, gunData.damage);
                }
                gunData.currentAmmo--;
                timeSinceLastShoot = 0;
                playerUIController.UpdateBullets(gunData.currentAmmo);
                OnGunShot();
            }
        }
        else
        {
            Debug.Log($"Gun:Shoot No Pew");
            PlaySound(dryShootSound);
            animator.SetTrigger("DryShoot");
        }
    }

    private void PlaySound(AudioClip sound)
    {
        if (sound != null && audioSource != null)
        {
            audioSource.PlayOneShot(sound, fireSoundVolume);
        }
    }

    private void PlayMuzzleFlash()
    {
        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }
    }
    private void Update()
    {
        timeSinceLastShoot += Time.deltaTime;
        Debug.DrawRay(firePoint.position, firePoint.forward, Color.red);
    }
    private void OnGunShot()
    {
        timeSinceLastShoot -= Time.deltaTime;
    }
}
