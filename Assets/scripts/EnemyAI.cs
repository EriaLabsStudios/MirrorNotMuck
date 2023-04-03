using System.Collections;
using System.Collections.Generic;
using Mirror;
using Mirror.Examples.Tanks;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float chaseDistance = 10f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private Transform target;
    [SerializeField] private int health = 100;
    private NavMeshAgent agent;
    private Animator animator;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (!isServer) return;

        // Buscar al jugador si no se ha asignado una referencia
        if (target == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                target = playerObject.transform;
            }
        }

        if (target != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, target.position);

            if (distanceToPlayer <= chaseDistance)
            {
                agent.SetDestination(target.position);
                float speed = agent.velocity.magnitude / agent.speed;
                animator.SetFloat("Speed", speed);
                Vector3 direction = (target.position - transform.position).normalized;
                Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
            }
            else
            {
                agent.ResetPath();  
                animator.SetFloat("Speed", 0f);
            }
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Projectile"))
        {
            // Asumiendo que el proyectil tiene un componente de daño, puedes acceder a él así:
            GunController projectile = collision.gameObject.GetComponent<GunController>();
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
