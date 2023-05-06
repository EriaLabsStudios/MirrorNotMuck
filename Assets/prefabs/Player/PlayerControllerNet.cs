using System.Collections;
using System.Collections.Generic;
using Mirror;
using Mirror.SimpleWeb;
using UnityEngine;

public class PlayerControllerNet : NetworkBehaviour
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
    [Header("Player Stats")]
    [SerializeField] [SyncVar] private int score = 0;
    
    private CharacterController controller;

    public float moveSpeed = 5.0f;
    public float slideSpeed = 10.0f;
    public float friction = 0.5f;
    public float lastfriction = 0.5f;
    public Vector3 velocity;

    private bool isSliding = false;
    private Vector3 slidingDir = Vector3.zero;
    private float slidingTime = 0;
    private float maxSlidingTime = 1;


    GameController gameController;



   // private PlayerUIController playerUIController;
    private void Start()
    {
        transform.position = new Vector3(4, 4, 66);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        controller = gameObject.GetComponent<CharacterController>();
      /*  playerUIController = FindObjectOfType<PlayerUIController>();
        if (playerUIController == null)
        {
            Debug.LogError("PlayerUIController no se encuentra en la escena");
        }*/


        if (isLocalPlayer)
        {
            mainCamera.gameObject.SetActive(true);
        }

        Transform playersParent;
        if (isLocalPlayer && isOwned)
        {     playersParent = GameObject.Find("LocalPlayer").transform;

        }else
        playersParent = GameObject.Find("PlayersParent").transform;

       transform.SetParent(playersParent);
            if(isServer)
            gameController = GameObject.Find("GameController").GetComponent<GameController>();
       

    }

    private void Update()
    {
       if (!isOwned) return;

        HandleMovement();
        HandleCameraRotation();
        HandleVFX();
        handleInputs();
    }

    public void AddScore(int points)
    {
        Log.Info("LocalPlayerController::AddScore before isLocalPlayer");
        if (isLocalPlayer)
        {
            Log.Info("LocalPlayerController::AddScore after isLocalPlayer");
            score += points;
        //    playerUIController.UpdatePoints(score);
        }
    }


    private void handleInputs()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            CmdVoteStartRound();
        }
    }

    [Command]
    private void CmdVoteStartRound()
    {
        Debug.Log("[Server] start game");
        gameController.StartGame();
    }
    
    private void HandleVFX()
    {

    }
    private void HandleSliding(Vector3 direction)
    {

        if (Input.GetKeyDown(KeyCode.LeftControl) && !isSliding && controller.isGrounded && direction.magnitude > 0)
        {
            cameraHolder.transform.position -= new Vector3(0, 0.5f, 0);
            isSliding = true;
            slidingDir = direction.normalized;
            velocity += direction.normalized * slideSpeed * Time.deltaTime;
            slidingTime = Time.timeSinceLevelLoad + maxSlidingTime;
        }
        Debug.Log("[SlidingTIME ] " + slidingTime + " Time " + Time.timeSinceLevelLoad);

         

        if (isSliding)
        {
            velocity += slidingDir * slideSpeed * Time.deltaTime;
            if (Time.timeSinceLevelLoad > slidingTime )
            {
                isSliding = false;
                cameraHolder.transform.position += new Vector3(0, 0.5f, 0);
            }

        }
    }

    private int lastFrameSpaceBar = 0;
    

    private bool handleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            lastFrameSpaceBar = Time.frameCount + 5;
        }

        if (Time.frameCount > lastFrameSpaceBar && lastFrameSpaceBar != 0)
        {
            lastFrameSpaceBar = 0;
            return true;
        }

        return false;
    }
    private void HandleMovement()
    {
   
        // Get input for movement direction
        Vector3 direction =  new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        direction = isSliding ? slidingDir :transform.TransformDirection(direction);
  
        // Apply movement direction to velocity
        if (direction.magnitude > 0)
        {
            velocity += direction.normalized * moveSpeed * Time.deltaTime;

        }
        HandleSliding(direction);

        if (!controller.isGrounded)
        {
            velocity.y += gravityValue * Time.deltaTime;
        }
        else velocity.y = -0.5f;



        if (handleJump() && controller.isGrounded)
        {
            velocity.y = -0.5f;
            velocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
            if (isSliding)
                cameraHolder.transform.position += new Vector3(0, 0.5f, 0);
            isSliding = false;    
        }

        var frictionLocal = handleFriction(direction);

        velocity.x *= 1 - frictionLocal * Time.deltaTime;
        velocity.z *= 1 - frictionLocal * Time.deltaTime;
        // Apply velocity to character controller
        controller.Move(velocity * Time.deltaTime);
        if (transform.position.y < -5) transform.position = respawnPos;

       
    }
 

    private float handleFriction(Vector3 direction)
    {
        float fric = friction;
  
        if (!controller.isGrounded)
        {
            fric *= 0.8f;
        }
        fric +=  (1 - direction.magnitude) * 2;
        fric = Mathf.Lerp(fric, lastfriction, 2 * Time.deltaTime);
        lastfriction = fric;
 
        return fric;
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
    public void CmdShootEnemy(GameObject enemy, float damage)
    {
        Debug.Log("[Server] Damage enemy " + enemy + " damage " + damage);
        if (enemy == null) return;  
        
        IDamageable damageable = enemy.transform.GetComponent<IDamageable>();
        if(damageable == null) return;
        
        damageable?.Damage(damage, this);
        

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
