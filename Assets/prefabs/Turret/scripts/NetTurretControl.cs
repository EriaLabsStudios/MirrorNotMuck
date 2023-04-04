using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetTurretControl : NetworkBehaviour
{
    [SerializeField]
    Transform turretHead;
    [SerializeField]
    [SyncVar]
    GameObject targetEnemy;
    [SerializeField]
    Transform shootPos;

    Transform enemyParent;

    [Header("Configuración de la torreta")]
    public float maxPitch = 45f; // el ángulo máximo de pitch permitido
    public float minPitch = -45f; // el ángulo mínimo de pitch permitido
    public float rotationSpeed = 20f;

    public float fireRate = 4f; // la frecuencia de disparo en segundos
    public float detectionRadius = 10f; // el radio de detección de enemigos


    [SerializeField]
    GameObject projectile;

    // Start is called before the first frame update
    void Start()
    {
        enemyParent = GameObject.Find("EnemyParent").transform;
      
    }

    // Update is called once per frame
    void Update()
    {
        if (targetEnemy == null && isServer) CheckForEnemies();

        if (targetEnemy == null)
        {
            stopShooting();
            return;
        }

        lookAtEnemy();

    }


    [Client]
    void lookAtEnemy()
    {
        Vector3 targetPos = targetEnemy.transform.position - transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(targetPos, Vector3.up);

        // Limitar el ángulo de pitch entre minPitch y maxPitch
        Vector3 eulerAngles = targetRotation.eulerAngles;
        float pitch = eulerAngles.x;
        if (pitch > 180f) pitch -= 360f; // Convertir de 0-360 a -180-180
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        eulerAngles.x = pitch;

        Quaternion newRotation = Quaternion.Euler(eulerAngles);

        // Aplicar rotación suave usando Lerp
        turretHead.rotation = Quaternion.Lerp(turretHead.rotation, newRotation, Time.deltaTime * rotationSpeed);
    }

    [Server]
    void FireBullet()
    {
        RpcShootBulletEffects();
        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(shootPos.position, shootPos.TransformDirection(Vector3.forward), out hit, Mathf.Infinity))
        {
            Debug.DrawRay(shootPos.position, shootPos.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
            Debug.Log($"hit.collider {hit.collider.gameObject.name}");
            if (hit.collider.CompareTag("Enemy"))
            {
                hit.collider.gameObject.GetComponent<EnemyAI>().TakeDamage(35);
                Debug.Log("Did Hit");
            }
        }
        else
        {
            Debug.DrawRay(shootPos.position, shootPos.TransformDirection(Vector3.forward) * 1000, Color.white);
            Debug.Log("Did not Hit");
        }
    }

    [Server]
    void CheckForEnemies()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius);

        foreach (Collider hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Enemy"))
            {
                targetEnemy = hitCollider.gameObject;
                stopShooting();
                startShooting();
                break;
            }
        }
    }

    [ClientRpc]
    public void RpcShootBulletEffects()
    {
        GameObject projectileInstance =
          Instantiate(projectile, shootPos.position, projectile.transform.rotation);

        projectileInstance.transform.LookAt(projectileInstance.transform.position + shootPos.forward);
    }

    void startShooting()
    {
        InvokeRepeating("FireBullet", 0f, fireRate);
    }


    void stopShooting() {
        CancelInvoke("FireBullet");
    }

    void getClosestEnemy() {

        float closestEnemy = 10000;
        GameObject targetFound = null;
        for(int i = 0; i < enemyParent.childCount; i++)
        {
          

            if (Vector3.Distance(transform.position, enemyParent.GetChild(i).transform.position) < closestEnemy)
            {
                closestEnemy = Vector3.Distance(transform.position, enemyParent.GetChild(i).transform.position);
                targetFound = enemyParent.GetChild(i).gameObject;
            }
        }

        targetEnemy = targetFound;
    }
}
