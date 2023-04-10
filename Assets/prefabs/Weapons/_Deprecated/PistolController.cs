using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PistolController : MonoBehaviour
{
    public LocalPlayerController player;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 5f;
    [SerializeField] private float dispersion = 0.5f;
    private float nextFireTime = 0f;
    [SerializeField] private AudioClip fireSound;
    [SerializeField] private float fireSoundVolume = 1.0f;
    private AudioSource audioSource;
    [SerializeField] private ParticleSystem muzzleFlash;
    public Animator animator;
    public float damage;

    private Camera playerCamera;

    private void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void Update()
    {
        if (player == null || !player.hasAuthority) return;
        
        if (player.isOwned && playerCamera == null)
        {
            playerCamera = player.GetComponentInChildren<Camera>();
        }
    }

    public void Fire()
    {
        if (Time.time >= nextFireTime)
        {
            Vector3 spawnPosition = firePoint.position;
            Vector3 lookDirection = firePoint.forward + new Vector3(getRandom(), getRandom(), getRandom());
            PlayFireSound();
            PlayMuzzleFlash();
            animator.SetTrigger("Shoot");
            player.CmdfireBullet(spawnPosition, lookDirection);
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

    private float getRandom()
    {
        return Random.Range(-dispersion, dispersion);
    }
}