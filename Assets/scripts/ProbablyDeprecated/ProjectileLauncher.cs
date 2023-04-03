using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;

public class ProjectileLauncher : NetworkBehaviour
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private GameObject cubePrefabTest;
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


        Transform ts = playerCamera.transform;
        Vector3 spawnPosition = playerCamera.transform.position + playerCamera.transform.forward;
        Vector3 lookDirection = playerCamera.transform.rotation * Vector3.forward;

        CmdspawnProjectile(spawnPosition, lookDirection, launchForce);
    }

    [Command]
    void CmdspawnProjectile(Vector3 spawnPosition,Vector3 lookDirection, float launchForce)
    {
        Debug.Log("[SPAWNPROJECTILE] " + spawnPosition + " LAUNCHFORCE " + launchForce);
   

        GameObject projectileInstance = Instantiate(projectilePrefab, spawnPosition, projectilePrefab.transform.rotation);

        projectileInstance.transform.LookAt(projectileInstance.transform.position + lookDirection);
        NetworkServer.Spawn(projectileInstance);

        Rigidbody projectileRigidbody = projectileInstance.GetComponent<Rigidbody>();

        if (projectileRigidbody != null)
        {
            projectileRigidbody.AddForce(lookDirection.normalized * launchForce);
        }

     
    }

    IEnumerator DestroBulletAfterTime()
    {
        yield return new WaitForSeconds(.1f);
        NetworkServer.Destroy(this.gameObject);
    }


    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        // Haz algo para que el objeto se vea o se active en el cliente que lo controla
    }
}