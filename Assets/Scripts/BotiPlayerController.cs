using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Simple keyboard-based movement controller for Boti robot.
/// Provides smooth grid-aligned movement with WASD keys.
/// </summary>
public class BotiPlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float gridSize = 1f;

    [Header("References")]
    public Camera mainCamera;

    // Movement state
    private Vector3 targetPosition;
    private bool isMoving = false;
    private Vector3 moveDirection = Vector3.zero;

    void Start()
    {
        targetPosition = transform.position;

        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        bool wPressed = keyboard.wKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame;
        bool sPressed = keyboard.sKey.wasPressedThisFrame || keyboard.downArrowKey.wasPressedThisFrame;
        bool aPressed = keyboard.aKey.wasPressedThisFrame || keyboard.leftArrowKey.wasPressedThisFrame;
        bool dPressed = keyboard.dKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame;

        if (!isMoving)
        {
            if (dPressed)
            {
                moveDirection = new Vector3(1, 0, 0);
                StartMove();
            }
            else if (aPressed)
            {
                moveDirection = new Vector3(-1, 0, 0);
                StartMove();
            }
            else if (wPressed)
            {
                moveDirection = new Vector3(0, 0, 1);
                StartMove();
            }
            else if (sPressed)
            {
                moveDirection = new Vector3(0, 0, -1);
                StartMove();
            }
        }

        if (isMoving)
        {
            float step = moveSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);

            if (transform.position == targetPosition)
                isMoving = false;
        }
    }

    void StartMove()
    {
        targetPosition = transform.position + moveDirection * gridSize;
        isMoving = true;

        if (moveDirection.x > 0)
            transform.rotation = Quaternion.Euler(0, 90, 0);
        else if (moveDirection.x < 0)
            transform.rotation = Quaternion.Euler(0, -90, 0);
        else if (moveDirection.z > 0)
            transform.rotation = Quaternion.Euler(0, 0, 0);
        else if (moveDirection.z < 0)
            transform.rotation = Quaternion.Euler(0, 180, 0);
    }

    public bool CanMoveTo(Vector3 position)
    {
        return true;
    }

    public Vector3 GetGridPosition()
    {
        return new Vector3(
            Mathf.Round(transform.position.x),
            transform.position.y,
            Mathf.Round(transform.position.z)
        );
    }
}