using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;

public class ProjectileLauncher : NetworkBehaviour
{
    [SerializeField] private GameObject projectilePrefab;
    private Camera playerCamera;
    [SerializeField] private float minLaunchForce = 500f;
    [SerializeField] private float maxLaunchForce = 3000f;
    [SerializeField] private int damageAmount = 10;
    [SerializeField] private float chargeTime = 2f;
    [SerializeField] private KeyCode fireKey = KeyCode.Space;
    private bool initialized = false;
    private float timeKeyPressed;

    // Propiedad para acceder al valor de daño
    public int Damage
    {
        get { return damageAmount; }
    }
    
    void Update()
    {
        if (!isLocalPlayer) return;
        
        if (!initialized)
        {
            InitializeCamera();
            initialized = true;
        }
        if (playerCamera == null) return;

        
        if (Input.GetKeyDown(fireKey))
        {
            timeKeyPressed = 0f;
        }

        if (Input.GetKey(fireKey))
        {
            timeKeyPressed += Time.deltaTime;
        }

        if (Input.GetKeyUp(fireKey))
        {
            LaunchProjectile();
        }
    }
    void InitializeCamera()
    {
        Camera[] childCameras = GetComponentsInChildren<Camera>(true);
        if (childCameras.Length > 0)
        {
            playerCamera = childCameras[0];
            Debug.Log($"Cámara encontrada: {playerCamera.name}");
        }
        else
        {
            Debug.LogError("No se encontró una cámara en los hijos del objeto.");
        }
    }
    void LaunchProjectile()
    {
        float chargePercentage = Mathf.Clamp01(timeKeyPressed / chargeTime);
        float launchForce = Mathf.Lerp(minLaunchForce, maxLaunchForce, chargePercentage);

        Vector3 spawnPosition = playerCamera.transform.position + playerCamera.transform.forward;
        Quaternion spawnRotation = playerCamera.transform.rotation;

        GameObject projectileInstance = Instantiate(projectilePrefab, spawnPosition, spawnRotation);
        Rigidbody projectileRigidbody = projectileInstance.GetComponent<Rigidbody>();

        if (projectileRigidbody != null)
        {
            projectileRigidbody.AddForce(playerCamera.transform.forward * launchForce);
        }
    }
}