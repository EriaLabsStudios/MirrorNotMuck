using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;

public class PistolController : MonoBehaviour
{
    public LocalPlayerController player;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float LauncForce = 500f;
    [SerializeField] private int damageAmount = 10;
    [SerializeField] private float chargeTime = 2f;
    [SerializeField] private KeyCode fireKey = KeyCode.Space;
    private bool initialized = false;
    private float timeKeyPressed;
    private bool has_been_initialized;
    [SerializeField] private AudioClip fireSound;
    [SerializeField] private float fireSoundVolume = 1.0f;
    private AudioSource audioSource;
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private float fireRate = 5f;
    [SerializeField] private float dispersion = 0.5f;
    private float nextFireTime = 0f;

    // Propiedad para acceder al valor de daño
    public int Damage
    {
        get { return damageAmount; }
    }



    private Camera playerCamera;
    private bool _isplayerNull;

    private Camera FindCameraInChildren()
    {
        return GetComponentInParent<LocalPlayerController>().GetComponentInChildren<Camera>();
    }

    private void Start()
    {
        _isplayerNull = player == null;
        audioSource = gameObject.AddComponent<AudioSource>();
     


    }


    void Update()
    {
        InitializeIfNeeded();
        if (player == null || !player.hasAuthority) return;

        if (player != null && player.isOwned)
        {
            if (playerCamera == null)
            {
                playerCamera = FindCameraInChildren();
            }
            
        }
        else
        {
            return;
        }
    }

    public void Fire()
    {
        Debug.Log("PistolController::Fire");
        if (Time.time >= nextFireTime)
        {
            timeKeyPressed += Time.deltaTime;
            //Vector3 spawnPosition = new Vector3(firePoint.position.x, firePoint.position.y, firePoint.position.z + 1);
            Vector3 spawnPosition = firePoint.position;
            Vector3 lookDirection = new Vector3(firePoint.forward.x + getRandom(), firePoint.forward.y + getRandom(), firePoint.forward.z + getRandom());
            PlayFireSound();
            PlayMuzzleFlash();
            player.CmdfireBullet(spawnPosition,lookDirection);
            nextFireTime = Time.time + 1f / fireRate;
        }
    }

    private void PlayFireSound()
    {
        if (fireSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(fireSound, fireSoundVolume);
        }
    }

    private void PlayMuzzleFlash()
    {
        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }
    }

   /* [Command(channel = Channels.Unreliable)]
    private void LaunchProjectile()
    {
        if (!isOwned) return;
        float chargePercentage = Mathf.Clamp01(timeKeyPressed / chargeTime);
        float launchForce = Mathf.Lerp(minLaunchForce, maxLaunchForce, chargePercentage);
        PlayFireSound();
        PlayMuzzleFlash();
        Vector3 spawnPosition = firePoint.position;
        //Vector3 spawnPosition = new Vector3(firePoint.position.x, firePoint.position.y, firePoint.position.z + 1);
        //Vector3 lookDirection = firePoint.forward;
        Vector3 lookDirection = new Vector3(firePoint.forward.x + getRandom(),firePoint.forward.y + getRandom(),firePoint.forward.z + getRandom()) ;

        CmdspawnProjectile(spawnPosition, lookDirection, maxLaunchForce);
    }*/

    private float getRandom()
    {
        return UnityEngine.Random.Range(-0.0f, dispersion);
    } 
   /* [Command(channel = Channels.Unreliable)]
    private void CmdspawnProjectile(Vector3 spawnPosition, Vector3 lookDirection, float launchForce)
    {
        GameObject projectileInstance =
            Instantiate(projectilePrefab, spawnPosition, projectilePrefab.transform.rotation);

        projectileInstance.transform.LookAt(projectileInstance.transform.position + lookDirection);
        NetworkServer.Spawn(projectileInstance);

        Rigidbody projectileRigidbody = projectileInstance.GetComponent<Rigidbody>();

        if (projectileRigidbody != null)
        {
            projectileRigidbody.AddForce(lookDirection.normalized * launchForce);
        }
    }*/

    private void InitializeIfNeeded()
    {
        
            _isplayerNull = player == null;

            if (!_isplayerNull)
            {
                playerCamera = FindCameraInChildren();
               // Debug.Log($"The result of the player is now : {player} and camera: {playerCamera}");
                initialized = true;
            }
        
    }
}