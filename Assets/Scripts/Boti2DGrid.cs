using UnityEngine;

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
        Vector2 newPos = gridPosition;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            newPos.y += tileSize;
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            newPos.y -= tileSize;
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            newPos.x -= tileSize;
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
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
