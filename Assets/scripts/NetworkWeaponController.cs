using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkWeaponController : NetworkBehaviour
{
    [SerializeField]
    GameObject projectile;

    [Command]
    public void CmdfireBullet(Vector3 spawnPos, Vector3 lookDirection)
    {
        GameObject projectileInstance =
          Instantiate(projectile, spawnPos, projectile.transform.rotation);

        projectileInstance.transform.LookAt(projectileInstance.transform.position + lookDirection);

        NetworkServer.Spawn(projectileInstance);
        
        Rigidbody projectileRigidbody = projectileInstance.GetComponent<Rigidbody>();

        if (projectileRigidbody != null)
        {
            projectileRigidbody.AddForce(lookDirection.normalized * 500);
        }

    }
}
