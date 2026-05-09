using UnityEngine;

/// <summary>
/// Maneja la dirección del robot y cambia sprites.
/// </summary>
public class RobotDirection : MonoBehaviour
{
    private SpriteManager spriteManager;
    private string currentDirection = "right";

    public void Initialize(SpriteManager manager)
    {
        spriteManager = manager;
        UpdateSprite();
    }

    public void SetDirection(Vector3 direction)
    {
        if (direction.z > 0)
            currentDirection = "up";
        else if (direction.z < 0)
            currentDirection = "down";
        else if (direction.x < 0)
            currentDirection = "left";
        else if (direction.x > 0)
            currentDirection = "right";

        UpdateSprite();
    }

    public void SetDirection(string direction)
    {
        currentDirection = direction.ToLower();
        UpdateSprite();
    }

    void UpdateSprite()
    {
        if (!spriteManager) return;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr && spriteManager)
        {
            sr.sprite = spriteManager.GetRobotSprite(currentDirection);
        }
    }

    public string GetCurrentDirection()
    {
        return currentDirection;
    }
}