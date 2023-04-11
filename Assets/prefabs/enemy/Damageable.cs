using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public abstract class Damagable : NetworkBehaviour, IDamageable
{
    public float health = 100;
    [SerializeField] protected GameObject floatingTextPrefab;
    [SerializeField] protected float yOffset;
    
    
    public virtual void Damage(float damage, LocalPlayerController player)
    {
        
        health -= damage;
        ShowFloatingText(damage);
        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
    protected void ShowFloatingText(float damage)
    {
        if (floatingTextPrefab != null)
        {
            GameObject textInstance = Instantiate(floatingTextPrefab, transform.position + Vector3.up * yOffset,
                Quaternion.identity);
            FloatingDamageText floatingText = textInstance.GetComponent<FloatingDamageText>();
            floatingText.SetText(damage.ToString());
        }
    }
}
