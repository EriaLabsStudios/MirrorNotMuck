using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    
    [SerializeField] private GameObject impactEffectPrefab;
    [SerializeField] private bool destroyedAfterImpact;
    [SerializeField] private int launchForce = 500;
    // Start is called before the first frame update
    
    void Start()
    {
        Rigidbody projectileRigidbody = gameObject.GetComponent<Rigidbody>();

        if (projectileRigidbody != null)
        {
            projectileRigidbody.AddForce(transform.forward * launchForce);
        }
    }
    private void CreateImpactEffect(Vector3 position, Quaternion rotation)
    {
        if (impactEffectPrefab != null)
        {
            GameObject impactEffectInstance = Instantiate(impactEffectPrefab, position, rotation);
            Destroy(impactEffectInstance, 1f);
            if (destroyedAfterImpact) Destroy(this.gameObject);
            
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Weapon"))
        {
            Vector3 impactPosition = collision.contacts[0].point;
            Quaternion impactRotation = Quaternion.LookRotation(collision.contacts[0].normal);
            CreateImpactEffect(impactPosition, impactRotation);
        }
        
        
    }

}
