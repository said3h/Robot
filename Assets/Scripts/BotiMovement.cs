using UnityEngine;

/// <summary>
/// Script simple para mover a Boti en una cuadrícula.
/// 
/// Controles:
/// - W: Mover hacia adelante
/// - A: Girar a la izquierda
/// - D: Girar a la derecha
/// </summary>
public class BotiMovement : MonoBehaviour
{
    // Velocidad de rotación (grados por segundo)
    public float rotationSpeed = 90f;
    
    // Velocidad de movimiento (unidades por segundo)
    public float moveSpeed = 3f;

    void Update()
    {
        // Girar a la izquierda con A
        if (Input.GetKey(KeyCode.A))
        {
            transform.Rotate(0f, -rotationSpeed * Time.deltaTime, 0f);
            Debug.Log("A presionado - Girando a la izquierda");
        }

        // Girar a la derecha con D
        if (Input.GetKey(KeyCode.D))
        {
            transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);
            Debug.Log("D presionado - Girando a la derecha");
        }

        // Mover hacia adelante con W
        if (Input.GetKey(KeyCode.W))
        {
            // Mover en la dirección hacia donde mira el robot (espacio local)
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime, Space.Self);
            Debug.Log("W presionado - Moviendo hacia adelante");
        }
    }
}
