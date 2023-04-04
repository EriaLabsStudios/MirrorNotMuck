using UnityEngine;
using TMPro;

public class FloatingDamageText : MonoBehaviour
{
    [SerializeField] private float duration = 1f;
    [SerializeField] private float yOffset = 1f;
    [SerializeField] private float speed = 1f;
    private Camera targetCamera;

    void Start()
    {
        targetCamera = Camera.main; // Asegúrate de que la cámara principal esté asignada
        Destroy(gameObject, duration);
    }

    public void SetText(string text)
    {
        GetComponentInChildren<TextMeshPro>().text = text;
    }

    void Update()
    {
        // Mueve el texto hacia arriba
        transform.position += Vector3.up * (speed * Time.deltaTime);

        // Orienta el texto hacia la cámara del jugador
        if (targetCamera != null)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - targetCamera.transform.position);
        }
    }
}