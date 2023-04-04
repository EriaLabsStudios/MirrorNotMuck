using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class LocalPlayerController : NetworkBehaviour
{
    [Header("Player Settings")]
    [SerializeField] private float playerSpeed = 2.0f;
    [SerializeField] private float jumpHeight = 1.0f;
    [SerializeField] private float gravityValue = -9.81f;
    [SerializeField] Vector3 respawnPos = Vector3.zero;
    
    [Header("Camera Settings")]
    public Camera mainCamera;
    public Transform cameraHolder;
    [SerializeField] private float lookSpeed = 3f;
    public float lookXLimit = 70.0f;
    private float rotationX = 0;
    
    [Header("Weapon Settings")]
    public Transform weaponHolder;
    [SerializeField] GameObject projectile;
    
    private CharacterController controller;
    private Vector3 playerVelocity;
    private bool groundedPlayer;

    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
        controller = gameObject.GetComponent<CharacterController>();

        if (isLocalPlayer)
        {
            mainCamera.gameObject.SetActive(true);
        }
    }

    private void Update()
    {
        if (!isLocalPlayer) return;

        HandleMovement();
        HandleCameraRotation();
    }

    private void HandleMovement()
    {
        groundedPlayer = controller.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        Vector3 move = transform.forward * Input.GetAxis("Vertical") + transform.right * Input.GetAxis("Horizontal");
        controller.Move(move * (Time.deltaTime * playerSpeed));

        if (Input.GetKey(KeyCode.Space) && groundedPlayer)
        {
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
        }

        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);

        if (transform.position.y < -5) transform.position = respawnPos;
    }

    private void HandleCameraRotation()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        transform.Rotate(Vector3.up * (mouseX * lookSpeed));

        rotationX += -mouseY * lookSpeed;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        cameraHolder.localRotation = Quaternion.Euler(rotationX, 0, 0);
    }

    [Command]
    public void CmdShootEnemy(GameObject enemy, int damage)
    {
        if (enemy != null)
            enemy.GetComponent<EnemyAI>().TakeDamage(damage);
    }

    [Command]
    public void CmdfireBullet(Vector3 spawnPos, Vector3 lookDirection)
    {
        GameObject projectileInstance =
            Instantiate(projectile, spawnPos, projectile.transform.rotation);

        projectileInstance.transform.LookAt(projectileInstance.transform.position + lookDirection);
        NetworkServer.Spawn(projectileInstance);
    }
}
