using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    public static Action shootInputDown;
    public static Action shootInputUp;
    public static Action reloadInput;
    [SerializeField] private KeyCode reloadKey = KeyCode.R;
    [SerializeField] private KeyCode shootKey = KeyCode.Mouse0;
    // Update is called once per frame

   
    void Update()
    {
        if(Input.GetKeyDown(shootKey)) shootInputDown?.Invoke();
        if (Input.GetKeyUp(shootKey)) shootInputUp?.Invoke();
        if (Input.GetKeyDown(reloadKey)) reloadInput?.Invoke();
    }
}
