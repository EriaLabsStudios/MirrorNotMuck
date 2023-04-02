using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class GameObjectMessage : NetworkBehaviour
{
    public GameObject gameObject;
    public Vector3 position;
    public Quaternion rotation;

    public GameObjectMessage() { }

    public GameObjectMessage(GameObject gameObject, Vector3 position, Quaternion rotation)
    {
        this.gameObject = gameObject;
        this.position = position;
        this.rotation = rotation;
    }
}


