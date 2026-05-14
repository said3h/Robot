using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Movimiento simple por grilla para Boti 2D.
/// Un tile por tecla, sin fisicas, sin interpolacion.
/// </summary>
public class Boti2DGrid : MonoBehaviour
{
    [Header("Configuracion")]
    public float tileSize = 1f;

    private Vector2 gridPosition;

    void Start()
    {
        gridPosition = transform.position;
    }

    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        Vector2 newPos = gridPosition;

        bool wPressed = keyboard.wKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame;
        bool sPressed = keyboard.sKey.wasPressedThisFrame || keyboard.downArrowKey.wasPressedThisFrame;
        bool aPressed = keyboard.aKey.wasPressedThisFrame || keyboard.leftArrowKey.wasPressedThisFrame;
        bool dPressed = keyboard.dKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame;

        if (wPressed)
            newPos.y += tileSize;
        else if (sPressed)
            newPos.y -= tileSize;
        else if (aPressed)
            newPos.x -= tileSize;
        else if (dPressed)
            newPos.x += tileSize;
        else
            return;

        if (!IsBlocked(newPos))
        {
            gridPosition = newPos;
            transform.position = gridPosition;
        }
    }

    bool IsBlocked(Vector2 pos)
    {
        Collider2D hit = Physics2D.OverlapPoint(pos);
        return hit != null && hit.CompareTag("Obstacle");
    }
}