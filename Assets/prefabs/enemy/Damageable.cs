using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public abstract class Damagable : NetworkBehaviour, IDamageable
{
    [SyncVar(hook = nameof(UpdateHealthBar))]
    public float health = 100;
    [SerializeField] protected GameObject floatingTextPrefab;
    [SerializeField] protected float yOffset;
    [SerializeField] Transform healthBar;

    public virtual void Damage(float damage, PlayerControllerNet player)
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

    protected void UpdateHealthBar(float oldHealth, float newHealth)
    {
        var calculaHealthScale = (newHealth * 6) / 100;
        healthBar.localScale = new Vector3(calculaHealthScale, 0.43801f, 0.34225f);

        if (newHealth < 0) healthBar.gameObject.SetActive(false);

    }
}
