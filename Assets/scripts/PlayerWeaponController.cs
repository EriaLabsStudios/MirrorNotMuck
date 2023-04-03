using UnityEngine;
using Mirror;
public class PlayerWeaponController : MonoBehaviour
{
    public GameObject weaponPrefab;
    public Transform weaponHolder;
    private GameObject currentWeapon;
    public Camera mainCamera;
    public Vector3 weaponPositionOffset;
    public Vector3 weaponRotationOffset;

    public NetworkIdentity localPlayerController;

    void Start()
    {
        EquipWeapon();
    }

    private void Update()
    {
        if (mainCamera != null)
        {
            Vector3 cameraRotation = mainCamera.transform.localRotation.eulerAngles;
            transform.localRotation = Quaternion.Euler(cameraRotation.x, 0, 0);
        }
        if (currentWeapon != null && mainCamera != null)
        {
            currentWeapon.transform.position = mainCamera.transform.position + mainCamera.transform.TransformDirection(weaponPositionOffset);
            currentWeapon.transform.rotation = mainCamera.transform.rotation * Quaternion.Euler(weaponRotationOffset);
        }
        
    }
    
    void EquipWeapon()
    {
        if (weaponPrefab != null && weaponHolder != null)
        {
            currentWeapon = Instantiate(weaponPrefab, weaponHolder.position, weaponHolder.rotation, weaponHolder);
            NetworkServer.Spawn(currentWeapon, localPlayerController.connectionToClient);
        }
    }
}