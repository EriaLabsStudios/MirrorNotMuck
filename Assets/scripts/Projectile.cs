using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    
    [SerializeField] private GameObject impactEffectPrefab;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    private void CreateImpactEffect(Vector3 position, Quaternion rotation)
    {
        if (impactEffectPrefab != null)
        {
            GameObject impactEffectInstance = Instantiate(impactEffectPrefab, position, rotation);
            Destroy(impactEffectInstance, 5f); // Ajusta el tiempo de destrucción según la duración de las partículas y el sonido.
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        Vector3 impactPosition = collision.contacts[0].point;
        Quaternion impactRotation = Quaternion.LookRotation(collision.contacts[0].normal);
        CreateImpactEffect(impactPosition, impactRotation);
        // Aquí puedes agregar lógica adicional para destruir el proyectil o infligir daño.
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
