using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Simple keyboard-based movement controller for Boti robot.
/// Provides smooth movement with WASD keys and world interaction with E.
/// </summary>
public class BotiPlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float gridSize = 1f;

    [Header("Interaction")]
    public float interactionRange = 3f;
    public Text interactionPromptText;

    [Header("References")]
    public Camera mainCamera;
    public BotiInventory inventory;

    // Movement state
    private Vector3 targetPosition;
    private bool isMoving = false;
    private Vector3 moveDirection = Vector3.zero;

    // Interaction state
    private Interactable nearbyInteractable;

    void Start()
    {
        targetPosition = transform.position;

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (inventory != null)
            inventory.UpdateUI();

        if (interactionPromptText != null)
            interactionPromptText.gameObject.SetActive(false);
    }

    void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        // Movement input
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

        // Smooth movement towards target
        if (isMoving)
        {
            float step = moveSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);

            if (transform.position == targetPosition)
                isMoving = false;
        }

        // Check for nearby interactables
        CheckForInteractables();

        // Interaction with E key
        if (keyboard.eKey.wasPressedThisFrame && nearbyInteractable != null)
        {
            nearbyInteractable.Interact();
            nearbyInteractable = null;
            if (interactionPromptText != null)
                interactionPromptText.gameObject.SetActive(false);
        }
    }

    private void CheckForInteractables()
    {
        nearbyInteractable = null;

        Collider[] hits = Physics.OverlapSphere(transform.position, interactionRange);
        float closestDist = interactionRange;

        foreach (Collider hit in hits)
        {
            Interactable interactable = hit.GetComponent<Interactable>();
            if (interactable != null)
            {
                float dist = Vector3.Distance(transform.position, hit.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    nearbyInteractable = interactable;
                }
            }
        }

        // Update UI
        if (interactionPromptText != null)
        {
            if (nearbyInteractable != null)
            {
                interactionPromptText.text = nearbyInteractable.GetInteractionMessage();
                interactionPromptText.gameObject.SetActive(true);
            }
            else
            {
                interactionPromptText.gameObject.SetActive(false);
            }
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