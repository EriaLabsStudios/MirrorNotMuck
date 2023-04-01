using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayerControllerTest : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float lookSpeed = 3f;

    private Rigidbody rb;
    public Camera mainCamera;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
   
    }


    public override  void OnStartLocalPlayer() {
        mainCamera.gameObject.SetActive(true);
    }

    private void Update()
    {
        if (!isLocalPlayer) return;

        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        Vector3 movement = new Vector3(horizontalInput, 0f, verticalInput);
        rb.velocity = transform.forward * verticalInput;

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        transform.Rotate(Vector3.up * mouseX * lookSpeed);

        Vector3 cameraRotation = mainCamera.transform.rotation.eulerAngles;
        float newRotationX = cameraRotation.x - mouseY * lookSpeed;
        mainCamera.transform.rotation = Quaternion.Euler(newRotationX, cameraRotation.y, cameraRotation.z);
    }
}
