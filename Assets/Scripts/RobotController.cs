using UnityEngine;

/// <summary>
/// Controlador del robot con sprites direccionales.
/// </summary>
public class RobotController : MonoBehaviour
{
    [Header("Configuración")]
    public float moveTime = 0.25f;
    public float rotateTime = 0.15f;

    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private bool isMoving = false;
    private bool isRotating = false;
    private float moveProgress = 0f;
    private float rotateProgress = 0f;

    private Vector3 startPosition;
    private Quaternion startRotation;

    private string currentDirection = "right";
    private RobotDirection robotDirection;

    void Start()
    {
        targetPosition = transform.position;
        targetRotation = transform.rotation;
        robotDirection = GetComponent<RobotDirection>();
    }

    void Update()
    {
        if (isMoving)
            UpdateMovement();
        else if (isRotating)
            UpdateRotation();
    }

    public void MoveForward()
    {
        if (isMoving || isRotating) return;

        startPosition = transform.position;
        targetPosition = startPosition + GetDirectionVector();
        moveProgress = 0f;
        isMoving = true;
    }

    public void MoveBack()
    {
        if (isMoving || isRotating) return;

        startPosition = transform.position;
        targetPosition = startPosition - GetDirectionVector();
        moveProgress = 0f;
        isMoving = true;
    }

    public void RotateLeft()
    {
        if (isMoving || isRotating) return;

        startRotation = transform.rotation;
        currentDirection = RotateDirection(currentDirection, -1);
        targetRotation = Quaternion.Euler(0, GetAngleFromDirection(currentDirection), 0);
        rotateProgress = 0f;
        isRotating = true;

        UpdateDirectionSprite();
    }

    public void RotateRight()
    {
        if (isMoving || isRotating) return;

        startRotation = transform.rotation;
        currentDirection = RotateDirection(currentDirection, 1);
        targetRotation = Quaternion.Euler(0, GetAngleFromDirection(currentDirection), 0);
        rotateProgress = 0f;
        isRotating = true;

        UpdateDirectionSprite();
    }

    void UpdateMovement()
    {
        moveProgress += Time.deltaTime / moveTime;

        if (moveProgress >= 1f)
        {
            transform.position = targetPosition;
            isMoving = false;
        }
        else
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, moveProgress);
        }
    }

    void UpdateRotation()
    {
        rotateProgress += Time.deltaTime / rotateTime;

        if (rotateProgress >= 1f)
        {
            transform.rotation = targetRotation;
            isRotating = false;
        }
        else
        {
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, rotateProgress);
        }
    }

    void UpdateDirectionSprite()
    {
        if (robotDirection)
            robotDirection.SetDirection(currentDirection);
    }

    public bool IsIdle()
    {
        return !isMoving && !isRotating;
    }

    public Vector3 GetGridPosition()
    {
        return targetPosition;
    }

    public string GetCurrentDirection()
    {
        return currentDirection;
    }

    Vector3 GetDirectionVector()
    {
        return currentDirection switch
        {
            "up" => Vector3.forward,
            "down" => Vector3.back,
            "left" => Vector3.left,
            "right" => Vector3.right,
            _ => Vector3.right
        };
    }

    float GetAngleFromDirection(string dir)
    {
        return dir switch
        {
            "up" => 0f,
            "right" => 90f,
            "down" => 180f,
            "left" => 270f,
            _ => 0f
        };
    }

    string RotateDirection(string dir, int steps)
    {
        string[] dirs = { "up", "right", "down", "left" };
        int idx = System.Array.IndexOf(dirs, dir);
        if (idx < 0) idx = 1; // Default to right

        idx += steps;
        if (idx < 0) idx += 4;
        if (idx >= 4) idx -= 4;

        return dirs[idx];
    }
}