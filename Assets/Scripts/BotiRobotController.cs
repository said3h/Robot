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

    // Current direction the robot is facing (0=North/forward, 1=East, 2=South, 3=West)
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
    // Using 0=North(Z+), 1=East(X+), 2=South(Z-), 3=West(X-)
    private readonly Vector3[] directionVectors = {
        Vector3.forward,  // North (0)
        Vector3.right,   // East (1)
        Vector3.back,    // South (2)
        Vector3.left     // West (3)
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
            MoveForward();
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
    /// Move the robot forward one tile in the current direction.
    /// </summary>
    void MoveForward()
    {
        // Store starting position
        startPos = transform.position;

        // Calculate target position: current position + direction vector * tile size
        // directionVectors[currentDirection] gives the forward direction
        targetPos = startPos + directionVectors[currentDirection] * tileSize;

        // Reset animation progress
        moveProgress = 0f;

        // Start moving
        isMoving = true;
    }

    /// <summary>
    /// Rotate the robot left (counter-clockwise) by 90 degrees.
    /// </summary>
    void RotateLeft()
    {
        // Store starting rotation
        startRot = transform.rotation;

        // Decrease direction (wraps from 0 to 3)
        currentDirection = (currentDirection + 3) % 4;

        // Calculate target rotation (90 degrees around Y axis)
        targetRot = Quaternion.Euler(0, currentDirection * 90f, 0);

        // Reset animation progress
        rotateProgress = 0f;

        // Start rotating
        isRotating = true;
    }

    /// <summary>
    /// Rotate the robot right (clockwise) by 90 degrees.
    /// </summary>
    void RotateRight()
    {
        // Store starting rotation
        startRot = transform.rotation;

        // Increase direction (wraps from 3 to 0)
        currentDirection = (currentDirection + 1) % 4;

        // Calculate target rotation (90 degrees around Y axis)
        targetRot = Quaternion.Euler(0, currentDirection * 90f, 0);

        // Reset animation progress
        rotateProgress = 0f;

        // Start rotating
        isRotating = true;
    }

    /// <summary>
    /// Update movement animation using linear interpolation (Lerp).
    /// Called every FixedUpdate while isMoving is true.
    /// </summary>
    void MoveProgress()
    {
        // Increase progress based on time
        moveProgress += Time.fixedDeltaTime / moveTime;

        // Clamp progress to 1 (prevents overshooting)
        if (moveProgress >= 1f)
        {
            moveProgress = 1f;
            isMoving = false;

            // Snap to exact target position
            transform.position = targetPos;
        }
        else
        {
            // Interpolate position from start to target
            // Lerp(start, end, t) returns a point between start and end
            transform.position = Vector3.Lerp(startPos, targetPos, moveProgress);
        }
    }

    /// <summary>
    /// Update rotation animation using spherical interpolation (Slerp).
    /// Called every FixedUpdate while isRotating is true.
    /// </summary>
    void RotateProgress()
    {
        // Increase progress based on time
        rotateProgress += Time.fixedDeltaTime / rotateTime;

        // Clamp progress to 1 (prevents overshooting)
        if (rotateProgress >= 1f)
        {
            rotateProgress = 1f;
            isRotating = false;

            // Snap to exact target rotation
            transform.rotation = targetRot;
        }
        else
        {
            // Interpolate rotation from start to target
            // Slerp is used for rotation (handles angle interpolation correctly)
            transform.rotation = Quaternion.Slerp(startRot, targetRot, rotateProgress);
        }
    }

    /// <summary>
    /// Check if the robot is currently moving or rotating.
    /// Useful for other scripts that need to wait for robot to be idle.
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