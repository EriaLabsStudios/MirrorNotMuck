using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class LocalPlayerController : NetworkBehaviour
{
    [Header("Player Settings")]


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

    public float moveSpeed = 5.0f;
    public float slideSpeed = 10.0f;
    public float friction = 0.5f;
   
    public Vector3 velocity;

    private bool isSliding = false;
    private Vector3 slidingDir = Vector3.zero;
    [SerializeField] private float jumpHeight = 1.0f;
    [SerializeField] private float gravityValue = -9.81f;

    private Vector3 cameraInitPos;

    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
        controller = gameObject.GetComponent<CharacterController>();
        cameraInitPos = cameraHolder.transform.position;

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
        HandleVFX();
    }



    private void HandleVFX()
    {

    }
    private void HandleMovement()
    {
    
        // Get input for movement direction
        Vector3 direction = isSliding ? slidingDir : transform.forward * Input.GetAxis("Vertical") + transform.right * Input.GetAxis("Horizontal");
        // Apply movement direction to velocity
        if (direction.magnitude > 0)
        {
            velocity += direction.normalized * moveSpeed * Time.deltaTime;

            if (Input.GetKey(KeyCode.LeftControl) && !isSliding && controller.isGrounded)
            {
                cameraHolder.transform.position -= new Vector3(0, 0.5f, 0);
                isSliding = true;
                slidingDir = direction.normalized;
                velocity += direction.normalized * slideSpeed * Time.deltaTime;
            }
        }

       if(isSliding)
        {
            velocity += slidingDir * slideSpeed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.Space) && controller.isGrounded)
        {
            velocity.y = 0;
            velocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
            if (isSliding)
                cameraHolder.transform.position += new Vector3(0, 0.5f, 0);
            isSliding = false;

        
        }

        
        if (controller.isGrounded)
        {
            // Apply friction and gravity to velocity
            velocity.x *= 1 - friction * Time.deltaTime;
            velocity.z *= 1 - friction * Time.deltaTime;   
        }
        else
        {
            Debug.Log("MENOS FRICCION");
            velocity.x *= 1 - (friction/2) * Time.deltaTime;
            velocity.z *= 1 - (friction/2) * Time.deltaTime;
        }

        if (!controller.isGrounded)
        {
            velocity.y += gravityValue * Time.deltaTime;
        }
        
        // Apply velocity to character controller
        controller.Move(velocity * Time.deltaTime);
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
