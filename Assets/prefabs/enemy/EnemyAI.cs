using System.Collections;
using System.Collections.Generic;
using Mirror;
using Mirror.Examples.Tanks;
using UnityEngine;
using UnityEngine.AI;
using TMPro;
using Unity.VisualScripting;

public class EnemyAI : Damagable
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float chaseDistance = 10f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private Transform target;
    private NavMeshAgent agent;
    private Animator animator;
    [Header("Audio")] [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip damagaSound;
    [SerializeField] private AudioClip deathSound;
    private float deathTimer = 1f;
    private bool isAlive;
    [SerializeField] Transform healthBar;
    private void Start()
    {
        isAlive= true;
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (!isServer) return;

        // Buscar al jugador si no se ha asignado una referencia
        if (target == null)
        {
            Transform playerObject = GameObject.Find("PlayersParent").transform;
            GameObject closestPlayer = playerObject.GetChild(0).gameObject;
            float distanceClosestPlayer = 0;
            for(int x = 0; x < playerObject.childCount; x++)
            {
                float distance = Vector3.Distance(playerObject.GetChild(x).position, transform.position);
                if (distance < distanceClosestPlayer)
                {
                    closestPlayer = playerObject.GetChild(x).gameObject;
                    distanceClosestPlayer = distance;
                }
            }

            target = closestPlayer.transform;

            
        }
        if (target != null && isAlive)
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
            PlayerControllerNet pc = ni.GetComponent<PlayerControllerNet>();
            //TODO: El daño instanciado en el enemigo y no en el arma?
            pc.CmdShootEnemy(this.gameObject, 20);


            // Destruir el proyectil al impactar
            Destroy(collision.gameObject);
        }
    }

    private void ShowFloatingText(float damage)
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
    }

    void Die(PlayerControllerNet attacker)
    {
        if(attacker !=null)attacker.AddScore(10);
        NetworkServer.Destroy(gameObject);
    }

    public override void Damage(float damage, PlayerControllerNet attacker)
    {
        if(attacker != null) attacker.AddScore(1);
        UpdateHealthBar(base.health, health- damage);
        health -= damage;
        ShowFloatingText(damage);
        audioSource.clip = deathSound;
        audioSource.Play();
        if (health <= 0)
        {
            audioSource.clip = damagaSound;
            audioSource.Play();
            StartCoroutine(Dying(attacker));

            Destroy(this);
        }
    }
    private IEnumerator Dying(PlayerControllerNet attacker)
    {
        isAlive = false;
        agent.enabled = false;
        animator.enabled = false;
        Vector3 force = transform.forward + transform.up;
        gameObject.GetComponent<Rigidbody>().AddForce(-force * 500);
        healthBar.gameObject.SetActive(false);
        yield return new WaitForSeconds(5);
        Die(attacker);
    }
    
    protected void UpdateHealthBar(float oldHealth, float newHealth)
    {
        var calculaHealthScale = (newHealth * 6) / 100;
        healthBar.localScale = new Vector3(calculaHealthScale, 0.43801f, 0.34225f);
    }
}