using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Direct grid movement controller for Boti robot.
/// </summary>
public class BotiRobotController : MonoBehaviour
{
    private readonly Vector2Int startPosition = new Vector2Int(-3, -3);
    private Vector2Int gridPosition;
    private const int boardMin = -3;
    private const int boardMax = 3;
    private readonly Vector2Int goalPosition = new Vector2Int(3, 3);

    private readonly Vector2Int[] obstacles =
    {
        new Vector2Int(0, 0),
        new Vector2Int(1, 0),
        new Vector2Int(-1, 1),
        new Vector2Int(2, -1)
    };

    private void Start()
    {
        ApplyGridPosition();
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.wKey.wasPressedThisFrame)
            MoveNorth();
        else if (keyboard.sKey.wasPressedThisFrame)
            MoveSouth();
        else if (keyboard.aKey.wasPressedThisFrame)
            MoveWest();
        else if (keyboard.dKey.wasPressedThisFrame)
            MoveEast();
    }

    public void MoveNorth()
    {
        TryMove(new Vector2Int(0, 1));
    }

    public void MoveSouth()
    {
        TryMove(new Vector2Int(0, -1));
    }

    public void MoveWest()
    {
        TryMove(new Vector2Int(-1, 0));
    }

    public void MoveEast()
    {
        TryMove(new Vector2Int(1, 0));
    }

    private void TryMove(Vector2Int direction)
    {
        Vector2Int nextPosition = gridPosition + direction;

        if (nextPosition.x < boardMin || nextPosition.x > boardMax ||
            nextPosition.y < boardMin || nextPosition.y > boardMax)
        {
            return;
        }

        if (IsObstacle(nextPosition))
        {
            return;
        }

        gridPosition = nextPosition;
        ApplyGridPosition();
    }

    private bool IsObstacle(Vector2Int position)
    {
        foreach (Vector2Int obstacle in obstacles)
            if (obstacle == position) return true;
        return false;
    }

    private void ApplyGridPosition()
    {
        transform.position = new Vector3(gridPosition.x, 0.8f, gridPosition.y);
    }

    public void ResetRobot()
    {
        gridPosition = startPosition;
        ApplyGridPosition();
    }
}