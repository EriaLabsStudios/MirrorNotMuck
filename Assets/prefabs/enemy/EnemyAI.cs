using System.Collections;
using System.Collections.Generic;
using Mirror;
using Mirror.Examples.Tanks;
using UnityEngine;
using UnityEngine.AI;
using TMPro;
using Unity.VisualScripting;

public class EnemyAI : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float chaseDistance = 10f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private Transform target;

    [SyncVar(hook = nameof(updateHealthBar))] [SerializeField]
    private int health = 100;

    private NavMeshAgent agent;
    private Animator animator;
    [SerializeField] Transform healthBar;
    [SerializeField] private GameObject floatingTextPrefab;
    [SerializeField] private float yOffset;
    [Header("Audio")] [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip damagaSound;
    [SerializeField] private AudioClip deathSound;
    private float deathTimer = 1f;

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
            Debug.Log("Es un projectil " + collision.gameObject.tag);
            // Asumiendo que el proyectil tiene un componente de daño, puedes acceder a él así:
            NetworkIdentity ni = NetworkClient.connection.identity;
            LocalPlayerController pc = ni.GetComponent<LocalPlayerController>();
            //TODO: El daño instanciado en el enemigo y no en el arma?
            pc.CmdShootEnemy(this.gameObject, 20);


            // Destruir el proyectil al impactar
            Destroy(collision.gameObject);
        }
    }

    private void ShowFloatingText(int damage)
    {
        if (floatingTextPrefab != null)
        {
            GameObject textInstance = Instantiate(floatingTextPrefab, transform.position + Vector3.up * yOffset,
                Quaternion.identity);
            FloatingDamageText floatingText = textInstance.GetComponent<FloatingDamageText>();
            floatingText.SetText(damage.ToString());
        }
    }

    public void TakeDamage(int damage)
    {
        Debug.Log("Taken damage " + damage);
        ShowFloatingText(damage);
        health -= damage;

        audioSource.clip = damagaSound;
        audioSource.Play();

        if (health <= 0)
        {
            audioSource.clip = deathSound;
            audioSource.Play();
            Die();
        }
    }

    void updateHealthBar(int oldHealth, int newHealth)
    {
        var calculaHealthScale = (newHealth * 6) / 100;
        healthBar.localScale = new Vector3(calculaHealthScale, 0.43801f, 0.34225f);
    }


    void Die()
    {
        // Lógica de muerte del enemigo (por ejemplo, animación de muerte, sonido, etc.)
        NetworkServer.Destroy(gameObject);
    }
}