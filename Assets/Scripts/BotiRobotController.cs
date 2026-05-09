using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Simple grid-based robot controller for Boti.
/// Movement: W (forward), A (left), D (right).
/// Grid-based, snap rotation at 90 degrees.
/// </summary>
public class BotiRobotController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Size of one tile in units")]
    public float tileSize = 1f;

    [Tooltip("Time to move one tile (seconds)")]
    public float moveTime = 0.25f;

    [Tooltip("Time to rotate 90 degrees (seconds)")]
    public float rotateTime = 0.15f;

    [Header("Board Settings")]
    [Tooltip("Board minimum X coordinate")]
    public int boardMinX = -3;

    [Tooltip("Board maximum X coordinate")]
    public int boardMaxX = 3;

    [Tooltip("Board minimum Z coordinate")]
    public int boardMinZ = -3;

    [Tooltip("Board maximum Z coordinate")]
    public int boardMaxZ = 3;

    [Tooltip("Goal position X")]
    public int goalX = 3;

    [Tooltip("Goal position Z")]
    public int goalZ = 3;

    // Current direction the robot is facing (0=North, 1=East, 2=South, 3=West)
    private int currentDirection = 1; // Start facing East (right)

    // Movement state
    private bool isMoving = false;
    private bool isRotating = false;
    private float moveProgress = 0f;
    private float rotateProgress = 0f;

    // Start and target positions/rotations for animation
    private Vector3 startPos;
    private Quaternion startRot;
    private Vector3 targetPos;
    private Quaternion targetRot;

    // Array of direction vectors: North, East, South, West
    private readonly Vector3[] directionVectors = {
        Vector3.forward,  // North (0)
        Vector3.right,    // East (1)
        Vector3.back,     // South (2)
        Vector3.left      // West (3)
    };

    // Obstacle positions (X, Z)
    private readonly Vector2Int[] obstacles = {
        new Vector2Int(0, 0),
        new Vector2Int(1, 0),
        new Vector2Int(-1, 1),
        new Vector2Int(2, -1)
    };

    void Update()
    {
        // Only allow input when not moving or rotating
        if (isMoving || isRotating)
            return;

        // Get keyboard reference
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        // W - Move forward one tile
        if (keyboard.wKey.wasPressedThisFrame)
        {
            TryMoveForward();
        }
        // A - Rotate left 90 degrees
        else if (keyboard.aKey.wasPressedThisFrame)
        {
            RotateLeft();
        }
        // D - Rotate right 90 degrees
        else if (keyboard.dKey.wasPressedThisFrame)
        {
            RotateRight();
        }
    }

    void FixedUpdate()
    {
        // Handle smooth movement animation
        if (isMoving)
        {
            MoveProgress();
        }
        // Handle smooth rotation animation
        else if (isRotating)
        {
            RotateProgress();
        }
    }

    /// <summary>
    /// Try to move the robot forward one tile.
    /// Checks board limits and obstacles before moving.
    /// </summary>
    void TryMoveForward()
    {
        // Calculate target position
        Vector3 newPos = transform.position + directionVectors[currentDirection] * tileSize;
        int newX = Mathf.RoundToInt(newPos.x);
        int newZ = Mathf.RoundToInt(newPos.z);

        // Check board boundaries
        if (newX < boardMinX || newX > boardMaxX || newZ < boardMinZ || newZ > boardMaxZ)
        {
            Debug.Log("Movement blocked: outside the board!");
            return;
        }

        // Check obstacles
        if (IsObstacle(newX, newZ))
        {
            Debug.Log("Movement blocked: obstacle!");
            return;
        }

        // Check goal
        if (newX == goalX && newZ == goalZ)
        {
            Debug.Log("Boti reached the goal!");
        }

        // Start movement
        startPos = transform.position;
        targetPos = newPos;
        moveProgress = 0f;
        isMoving = true;
    }

    /// <summary>
    /// Check if a position has an obstacle.
    /// </summary>
    bool IsObstacle(int x, int z)
    {
        foreach (Vector2Int obs in obstacles)
        {
            if (obs.x == x && obs.y == z)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Rotate the robot left (counter-clockwise) by 90 degrees.
    /// </summary>
    void RotateLeft()
    {
        startRot = transform.rotation;
        currentDirection = (currentDirection + 3) % 4;
        targetRot = Quaternion.Euler(0, currentDirection * 90f, 0);
        rotateProgress = 0f;
        isRotating = true;
    }

    /// <summary>
    /// Rotate the robot right (clockwise) by 90 degrees.
    /// </summary>
    void RotateRight()
    {
        startRot = transform.rotation;
        currentDirection = (currentDirection + 1) % 4;
        targetRot = Quaternion.Euler(0, currentDirection * 90f, 0);
        rotateProgress = 0f;
        isRotating = true;
    }

    /// <summary>
    /// Update movement animation using linear interpolation (Lerp).
    /// </summary>
    void MoveProgress()
    {
        moveProgress += Time.fixedDeltaTime / moveTime;

        if (moveProgress >= 1f)
        {
            moveProgress = 1f;
            isMoving = false;
            transform.position = targetPos;
        }
        else
        {
            transform.position = Vector3.Lerp(startPos, targetPos, moveProgress);
        }
    }

    /// <summary>
    /// Update rotation animation using spherical interpolation (Slerp).
    /// </summary>
    void RotateProgress()
    {
        rotateProgress += Time.fixedDeltaTime / rotateTime;

        if (rotateProgress >= 1f)
        {
            rotateProgress = 1f;
            isRotating = false;
            transform.rotation = targetRot;
        }
        else
        {
            transform.rotation = Quaternion.Slerp(startRot, targetRot, rotateProgress);
        }
    }

    /// <summary>
    /// Check if the robot is currently idle (not moving or rotating).
    /// </summary>
    public bool IsIdle()
    {
        return !isMoving && !isRotating;
    }

    /// <summary>
    /// Get the current direction as a string (for debugging).
    /// </summary>
    public string GetDirectionName()
    {
        string[] names = { "North", "East", "South", "West" };
        return names[currentDirection];
    }
}