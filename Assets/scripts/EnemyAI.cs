using System.Collections;
using System.Collections.Generic;
using Mirror;
using Mirror.Examples.Tanks;
using UnityEngine;

public class EnemyAI : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float chaseDistance = 10f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private Transform player;
    [SerializeField] private int health = 100;

    private void Start()
    {
        
    }

    void Update()
    {
        if (!isServer) return;

        // Buscar al jugador si no se ha asignado una referencia
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }

        if (player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            if (distanceToPlayer <= chaseDistance)
            {
                Vector3 moveDirection = (player.position - transform.position).normalized;
                transform.position += moveDirection * moveSpeed * Time.deltaTime;
                
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Projectile"))
        {
            // Asumiendo que el proyectil tiene un componente de daño, puedes acceder a él así:
            ProjectileLauncher projectile = collision.gameObject.GetComponent<ProjectileLauncher>();
            if (projectile != null)
            {
                TakeDamage(projectile.Damage);
            }

            // Destruir el proyectil al impactar
            Destroy(collision.gameObject);
        }
    }
    void TakeDamage(int damage)
    {
        health -= damage;

        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // Lógica de muerte del enemigo (por ejemplo, animación de muerte, sonido, etc.)
        Destroy(gameObject);
    }
}
