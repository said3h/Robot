using UnityEngine;
using UnityEngine.InputSystem;

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
    public float rotationSpeed = 90f;
    public float moveSpeed = 3f;

    void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.aKey.isPressed)
        {
            transform.Rotate(0f, -rotationSpeed * Time.deltaTime, 0f);
        }

        if (keyboard.dKey.isPressed)
        {
            transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);
        }

        if (keyboard.wKey.isPressed)
        {
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime, Space.Self);
        }
    }
}