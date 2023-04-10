using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayerControls : NetworkBehaviour
{
    public LocalPlayerController localPlayerController;
    public PistolController gunController;
    public KeyCode fireKey1 = KeyCode.Mouse2;
    void Start()
    {
        localPlayerController = GetComponent<LocalPlayerController>();
    }

    // Update is called once per frame
    [Client]
    void Update()
    {
        if (!isLocalPlayer) return;

        if (gunController == null)
        {
            gunController = GetComponentInChildren<PistolController>();
        }

        // if(hasAutority)
        /*if (Input.GetKey(KeyCode.Mouse0))
        {
            Debug.Log("PlayerControls::Update::Input.GetButtonDown-1");
            gunController.Fire();
        }*/
    }
}
