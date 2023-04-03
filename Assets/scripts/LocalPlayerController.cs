using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class LocalPlayerController : NetworkBehaviour
{

    [SerializeField] private float lookSpeed = 3f;
    [SerializeField] Vector3  respawnPos = Vector3.zero;
    private CharacterController controller;
    private Vector3 playerVelocity;
    private bool groundedPlayer;
    [SerializeField] private float playerSpeed = 2.0f;
    [SerializeField] private float jumpHeight = 1.0f;
    [SerializeField] private float gravityValue = -9.81f;
    public Transform weaponHolder;
    public float lookXLimit = 70.0f;
    float rotationX = 0;
    public Camera mainCamera;



    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
        controller = gameObject.GetComponent<CharacterController>();



    }
    public override  void OnStartLocalPlayer() {
        mainCamera.gameObject.SetActive(true);
    }

    private void Update()
    {
        if (!isLocalPlayer) return;

        groundedPlayer = controller.isGrounded;

        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        Vector3 move = transform.forward * Input.GetAxis("Vertical") + transform.right * +Input.GetAxis("Horizontal");

        controller.Move(move * (Time.deltaTime * playerSpeed));

        // Changes the height position of the player..
        if (Input.GetKey(KeyCode.Space) && groundedPlayer)
        {
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
        }

        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        transform.Rotate(Vector3.up * (mouseX * lookSpeed));

        Vector3 cameraRotation = mainCamera.transform.rotation.eulerAngles;
        rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        mainCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);

        if (transform.position.y < -5) transform.position = respawnPos;

    }

    [Command]
    public void CmdShootEnemy(GameObject enemy, int damage)
    {
        if(enemy != null)
        enemy.GetComponent<EnemyAI>().TakeDamage(damage);
    }

    [SerializeField]
    GameObject projectile;

    [Command]
    public void CmdfireBullet(Vector3 spawnPos, Vector3 lookDirection)
    {
        GameObject projectileInstance =
          Instantiate(projectile, spawnPos, projectile.transform.rotation);

        projectileInstance.transform.LookAt(projectileInstance.transform.position + lookDirection);

        NetworkServer.Spawn(projectileInstance);

    }
}
