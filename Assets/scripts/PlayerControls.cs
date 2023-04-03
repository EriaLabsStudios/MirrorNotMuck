using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayerControls : NetworkBehaviour
{
    public LocalPlayerController localPlayerController;
    public PistolController gunController;
    public KeyCode fireKey = KeyCode.Mouse1;
    void Start()
    {
        localPlayerController = GetComponent<LocalPlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (gunController == null)
        {
            gunController = GetComponentInChildren<PistolController>();
        }
        
        if (gunController == null || localPlayerController == null || !localPlayerController.hasAuthority) return;
        
       // if(hasAutority)
        if (Input.GetButtonDown("Fire1") || Input.GetKeyDown(fireKey) )
        {
            gunController.Fire();
        }
    }
}
