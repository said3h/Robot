using UnityEngine;

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
    
    // Input
    private float horizontal;
    private float vertical;
    
    void Start()
    {
        // Initialize position
        targetPosition = transform.position;
        
        // Find camera if not assigned
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }
    
    void Update()
    {
        // Read input
        horizontal = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right arrows
        vertical = Input.GetAxisRaw("Vertical");     // W/S or Up/Down arrows
        
        // Handle movement input
        if (!isMoving)
        {
            // Prioritize one direction at a time
            if (horizontal != 0 && vertical == 0)
            {
                moveDirection = new Vector3(horizontal > 0 ? 1 : -1, 0, 0);
                StartMove();
            }
            else if (vertical != 0 && horizontal == 0)
            {
                moveDirection = new Vector3(0, 0, vertical > 0 ? 1 : -1);
                StartMove();
            }
            else if (vertical != 0 && horizontal != 0)
            {
                // Prefer vertical when both pressed
                moveDirection = new Vector3(0, 0, vertical > 0 ? 1 : -1);
                StartMove();
            }
        }
        
        // Smooth movement towards target
        if (isMoving)
        {
            float step = moveSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);
            
            // Check if reached target
            if (transform.position == targetPosition)
            {
                isMoving = false;
            }
        }
    }
    
    void StartMove()
    {
        targetPosition = transform.position + moveDirection * gridSize;
        isMoving = true;
        
        // Rotate robot to face movement direction
        if (moveDirection.x > 0)
            transform.rotation = Quaternion.Euler(0, 90, 0);
        else if (moveDirection.x < 0)
            transform.rotation = Quaternion.Euler(0, -90, 0);
        else if (moveDirection.z > 0)
            transform.rotation = Quaternion.Euler(0, 0, 0);
        else if (moveDirection.z < 0)
            transform.rotation = Quaternion.Euler(0, 180, 0);
    }
    
    /// <summary>
    /// Check if robot can move to a position (for future collision detection)
    /// </summary>
    public bool CanMoveTo(Vector3 position)
    {
        // TODO: Add obstacle collision detection here
        return true;
    }
    
    /// <summary>
    /// Get current grid position
    /// </summary>
    public Vector3 GetGridPosition()
    {
        return new Vector3(
            Mathf.Round(transform.position.x),
            transform.position.y,
            Mathf.Round(transform.position.z)
        );
    }
}
