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
        Debug.Log("MoveNorth called");
        TryMove(new Vector2Int(0, 1));
    }

    public void MoveSouth()
    {
        Debug.Log("MoveSouth called");
        TryMove(new Vector2Int(0, -1));
    }

    public void MoveWest()
    {
        Debug.Log("MoveWest called");
        TryMove(new Vector2Int(-1, 0));
    }

    public void MoveEast()
    {
        Debug.Log("MoveEast called");
        TryMove(new Vector2Int(1, 0));
    }

    private void TryMove(Vector2Int direction)
    {
        Vector2Int nextPosition = gridPosition + direction;

        if (nextPosition.x < boardMin || nextPosition.x > boardMax ||
            nextPosition.y < boardMin || nextPosition.y > boardMax)
        {
            Debug.Log("Movement blocked: outside the board!");
            return;
        }

        if (IsObstacle(nextPosition))
        {
            Debug.Log("Movement blocked: obstacle!");
            return;
        }

        gridPosition = nextPosition;
        ApplyGridPosition();

        if (gridPosition == goalPosition)
            Debug.Log("Boti reached the goal!");
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
        Debug.Log("Robot reset to start position");
    }
}