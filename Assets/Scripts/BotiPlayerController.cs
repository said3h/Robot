using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Tile-based movement controller for Boti robot.
/// The grid position is the game logic; transform.position is only the visual position.
/// </summary>
public class BotiPlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float gridSize = 1f;
    public Vector2Int currentGridPosition;
    public Vector2Int targetGridPosition;

    [Header("Interaction")]
    public float interactionRange = 3f;
    public Text interactionPromptText;
    public BotiUI botiUI;

    [Header("References")]
    public Camera mainCamera;
    public BotiInventory inventory;
    public GridManager gridManager;
    public WorldState worldState;
    public WorldVisualSpawner visualSpawner;
    public BotiFeedback feedback;

    [Header("Build Mode")]
    public bool buildMode = false;
    public WorldStructureType selectedStructure = WorldStructureType.Wall;

    [Header("Tools")]
    public BotiTool equippedTool = BotiTool.Axe;

    [Header("Debug")]
    public bool debugMovementLogs = false;

    // Movement state
    private Vector3 targetPosition;
    private bool isMoving = false;
    private Vector2Int moveDirection = Vector2Int.zero;
    private Vector2Int facingDirection = Vector2Int.up;

    // Interaction state
    private Interactable nearbyInteractable;
    private bool loggedMissingKeyboard = false;

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (gridManager == null)
            gridManager = FindObjectOfType<GridManager>();

        if (worldState == null)
            worldState = FindObjectOfType<WorldState>();

        if (visualSpawner == null)
            visualSpawner = FindObjectOfType<WorldVisualSpawner>();

        if (feedback == null)
            feedback = GetComponent<BotiFeedback>();

        SetupGridPosition();
        EnsurePlayerStartsOnWalkableTile();
        LogMovement("BotiPlayerController active. BuildMode=" + buildMode + ", currentGrid=" + currentGridPosition + ", world=" + transform.position + ", gridManager=" + (gridManager != null));

        if (inventory != null)
            inventory.UpdateUI();

        if (botiUI == null)
            botiUI = FindObjectOfType<BotiUI>();

        if (botiUI != null)
        {
            botiUI.HidePrompt();
            botiUI.SetBuildInfo(buildMode, selectedStructure);
            botiUI.SetEquippedTool(equippedTool);
        }
        else if (interactionPromptText != null)
            interactionPromptText.gameObject.SetActive(false);
    }

    void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            if (!loggedMissingKeyboard)
            {
                LogMovement("No Keyboard.current found. Check Unity Input System package/settings.");
                loggedMissingKeyboard = true;
            }
            return;
        }

        // Movement input
        bool wPressed = keyboard.wKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame;
        bool sPressed = keyboard.sKey.wasPressedThisFrame || keyboard.downArrowKey.wasPressedThisFrame;
        bool aPressed = keyboard.aKey.wasPressedThisFrame || keyboard.leftArrowKey.wasPressedThisFrame;
        bool dPressed = keyboard.dKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame;
        bool movementPressed = wPressed || sPressed || aPressed || dPressed;

        HandleModeAndToolInput(keyboard);

        if (movementPressed && isMoving)
            LogMovement("Movement input ignored because Boti is already moving. currentGrid=" + currentGridPosition + ", targetGrid=" + targetGridPosition);

        if (!isMoving)
        {
            if (dPressed)
                TryStartMove(Vector2Int.right, "D/Right");
            else if (aPressed)
                TryStartMove(Vector2Int.left, "A/Left");
            else if (wPressed)
                TryStartMove(Vector2Int.up, "W/Up");
            else if (sPressed)
                TryStartMove(Vector2Int.down, "S/Down");
        }

        if (isMoving)
            MoveVisuallyToTarget();

        if (!isMoving)
            CheckForInteractables();

        if (!isMoving)
            UpdateTileFeedback();

        if (!isMoving && keyboard.eKey.wasPressedThisFrame && buildMode)
        {
            TryPlaceSelectedStructure();
            return;
        }

        if (!isMoving && keyboard.eKey.wasPressedThisFrame && nearbyInteractable != null)
        {
            if (nearbyInteractable.inventory == null)
                nearbyInteractable.inventory = inventory;

            if (nearbyInteractable.visualSpawner == null)
                nearbyInteractable.visualSpawner = visualSpawner;

            if (nearbyInteractable.feedback == null)
                nearbyInteractable.feedback = feedback;

            string message = nearbyInteractable.Interact(equippedTool);
            nearbyInteractable = null;

            if (botiUI != null && !string.IsNullOrEmpty(message))
            {
                botiUI.ShowMessage(message);
                if (message == "Inventory Full" && feedback != null)
                    feedback.PlayError();
            }
            else if (botiUI != null)
                botiUI.HidePrompt();
            else if (interactionPromptText != null)
                interactionPromptText.gameObject.SetActive(false);
        }
    }

    private void HandleModeAndToolInput(Keyboard keyboard)
    {
        bool changed = false;
        bool toolChanged = false;

        if (keyboard.cKey.wasPressedThisFrame)
        {
            CraftingSystem craftingSystem = FindObjectOfType<CraftingSystem>();
            if (craftingSystem != null)
                craftingSystem.ToggleCraftingPanel();
        }

        if (keyboard.bKey.wasPressedThisFrame)
        {
            buildMode = !buildMode;
            changed = true;
        }

        bool onePressed = keyboard.digit1Key.wasPressedThisFrame || keyboard.numpad1Key.wasPressedThisFrame;
        bool twoPressed = keyboard.digit2Key.wasPressedThisFrame || keyboard.numpad2Key.wasPressedThisFrame;

        if (buildMode && onePressed)
        {
            selectedStructure = WorldStructureType.Wall;
            changed = true;
        }
        else if (buildMode && twoPressed)
        {
            selectedStructure = WorldStructureType.StorageBox;
            changed = true;
        }
        else if (!buildMode && onePressed)
        {
            equippedTool = BotiTool.Axe;
            toolChanged = true;
        }
        else if (!buildMode && twoPressed)
        {
            equippedTool = BotiTool.Pickaxe;
            toolChanged = true;
        }

        if (changed && botiUI != null)
            botiUI.SetBuildInfo(buildMode, selectedStructure);

        if (toolChanged && botiUI != null)
            botiUI.SetEquippedTool(equippedTool);
    }

    private void TryPlaceSelectedStructure()
    {
        if (worldState == null || inventory == null)
            return;

        Vector2Int buildPosition = currentGridPosition + facingDirection;

        if (!worldState.CanPlaceStructure(buildPosition))
        {
            ShowBuildMessage("Cannot build here");
            return;
        }

        if (!SpendBuildCost())
        {
            ShowBuildMessage("Not enough resources");
            return;
        }

        if (!worldState.PlaceStructure(buildPosition, selectedStructure))
        {
            ShowBuildMessage("Cannot build here");
            return;
        }

        if (gridManager != null)
            gridManager.ApplyWorldState(worldState);

        if (visualSpawner != null)
            visualSpawner.RebuildVisuals(worldState, gridManager, inventory);

        if (feedback != null)
            feedback.PlayBuild();

        ShowBuildMessage("Built " + selectedStructure);
    }

    private bool SpendBuildCost()
    {
        if (selectedStructure == WorldStructureType.Wall)
            return inventory.SpendResources(0, 1, 0);

        if (selectedStructure == WorldStructureType.StorageBox)
            return inventory.SpendResources(1, 2, 0);

        return false;
    }

    private void ShowBuildMessage(string message)
    {
        if (botiUI != null)
            botiUI.ShowMessage(message);
        else
            Debug.Log(message);

        if (message == "Cannot build here" || message == "Not enough resources" || message == "Inventory Full")
        {
            if (feedback != null)
                feedback.PlayError();
        }
    }

    private void UpdateTileFeedback()
    {
        if (feedback == null || worldState == null)
            return;

        Vector2Int frontGridPosition = currentGridPosition + facingDirection;
        Vector3 frontWorldPosition = worldState.GridToWorldPosition(frontGridPosition);
        bool isValid = IsFrontTileActionValid(frontGridPosition);

        feedback.ShowTileHighlight(frontWorldPosition, isValid);

        if (buildMode)
            feedback.ShowBuildPreview(frontWorldPosition, selectedStructure, isValid);
        else
            feedback.HideBuildPreview();
    }

    private bool IsFrontTileActionValid(Vector2Int frontGridPosition)
    {
        if (buildMode)
            return worldState.CanPlaceStructure(frontGridPosition);

        WorldTileData tile = worldState.GetTile(frontGridPosition);
        if (tile == null)
            return false;

        if (tile.resourceType == WorldResourceType.Scrap)
            return true;

        if (tile.resourceType == WorldResourceType.Crystal)
            return equippedTool == BotiTool.Pickaxe;

        if (tile.obstacleType == WorldObstacleType.Tree)
            return equippedTool == BotiTool.Axe;

        if (tile.obstacleType == WorldObstacleType.Rock)
            return equippedTool == BotiTool.Pickaxe;

        return false;
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
        if (botiUI != null && botiUI.IsShowingMessage())
            return;

        if (buildMode && botiUI != null)
        {
            botiUI.ShowPrompt("Press E to place " + selectedStructure);
            return;
        }

        if (botiUI != null || interactionPromptText != null)
        {
            if (nearbyInteractable != null)
            {
                if (botiUI != null)
                    botiUI.ShowPrompt(nearbyInteractable.GetInteractionMessage());
                else
                {
                    interactionPromptText.text = nearbyInteractable.GetInteractionMessage();
                    interactionPromptText.gameObject.SetActive(true);
                }
            }
            else
            {
                if (botiUI != null)
                    botiUI.HidePrompt();
                else
                    interactionPromptText.gameObject.SetActive(false);
            }
        }
    }

    private void TryStartMove(Vector2Int direction, string inputName)
    {
        LogMovement("Input " + inputName + " pressed. BuildMode=" + buildMode + ", currentGrid=" + currentGridPosition + ", direction=" + direction);
        moveDirection = direction;
        facingDirection = direction;
        StartMove();
    }

    void StartMove()
    {
        Vector2Int nextGridPosition = currentGridPosition + moveDirection;
        LogMovement("Trying move. currentGrid=" + currentGridPosition + ", targetGrid=" + nextGridPosition);

        if (!CanMoveTo(nextGridPosition))
        {
            LogMovement("Move blocked by GridManager. targetGrid=" + nextGridPosition + ", reason=" + GetMoveBlockReason(nextGridPosition));
            return;
        }

        if (moveDirection == Vector2Int.right)
            transform.rotation = Quaternion.Euler(0, 90, 0);
        else if (moveDirection == Vector2Int.left)
            transform.rotation = Quaternion.Euler(0, -90, 0);
        else if (moveDirection == Vector2Int.up)
            transform.rotation = Quaternion.Euler(0, 0, 0);
        else if (moveDirection == Vector2Int.down)
            transform.rotation = Quaternion.Euler(0, 180, 0);

        targetGridPosition = nextGridPosition;
        targetPosition = GridToPlayerWorldPosition(targetGridPosition);
        isMoving = true;
        LogMovement("Move accepted. currentGrid=" + currentGridPosition + ", targetGrid=" + targetGridPosition + ", targetWorld=" + targetPosition);
    }

    private void SetupGridPosition()
    {
        if (gridManager != null)
            currentGridPosition = gridManager.WorldToGridPosition(transform.position);
        else
            currentGridPosition = new Vector2Int(Mathf.RoundToInt(transform.position.x / gridSize), Mathf.RoundToInt(transform.position.z / gridSize));

        targetGridPosition = currentGridPosition;
        targetPosition = GridToPlayerWorldPosition(currentGridPosition);
        transform.position = targetPosition;
    }

    private void EnsurePlayerStartsOnWalkableTile()
    {
        if (gridManager == null || CanMoveTo(currentGridPosition))
            return;

        Vector2Int originalPosition = currentGridPosition;
        Vector2Int safePosition;
        if (TryFindNearestWalkableTile(originalPosition, out safePosition))
        {
            currentGridPosition = safePosition;
            targetGridPosition = safePosition;
            targetPosition = GridToPlayerWorldPosition(safePosition);
            transform.position = targetPosition;
            LogMovement("Start tile was blocked. Moved Boti from grid " + originalPosition + " to nearest walkable grid " + safePosition);
        }
        else
        {
            LogMovement("Start tile is blocked and no walkable tile was found. " + GetMoveBlockReason(originalPosition));
        }
    }

    private bool TryFindNearestWalkableTile(Vector2Int startPosition, out Vector2Int safePosition)
    {
        int maxRadius = Mathf.Max(gridManager.width, gridManager.height);

        for (int radius = 1; radius <= maxRadius; radius++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    if (Mathf.Abs(x) != radius && Mathf.Abs(y) != radius)
                        continue;

                    Vector2Int candidate = startPosition + new Vector2Int(x, y);
                    if (CanMoveTo(candidate))
                    {
                        safePosition = candidate;
                        return true;
                    }
                }
            }
        }

        safePosition = startPosition;
        return false;
    }

    private void MoveVisuallyToTarget()
    {
        float step = moveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);

        if (transform.position == targetPosition)
        {
            currentGridPosition = targetGridPosition;
            isMoving = false;
            LogMovement("Move finished. currentGrid=" + currentGridPosition + ", world=" + transform.position);
        }
    }

    private Vector3 GridToPlayerWorldPosition(Vector2Int gridPosition)
    {
        Vector3 worldPosition;

        if (gridManager != null)
            worldPosition = gridManager.GridToWorldPosition(gridPosition);
        else
            worldPosition = new Vector3(gridPosition.x * gridSize, transform.position.y, gridPosition.y * gridSize);

        worldPosition.y = transform.position.y;
        return worldPosition;
    }

    public bool CanMoveTo(Vector2Int gridPosition)
    {
        if (gridManager == null)
        {
            LogMovement("GridManager missing. Allowing movement to " + gridPosition);
            return true;
        }

        return gridManager.CanMoveToGridPosition(gridPosition);
    }

    public bool CanMoveTo(Vector3 position)
    {
        if (gridManager == null)
            return true;

        return gridManager.CanMoveToWorldPosition(position);
    }

    public Vector3 GetGridPosition()
    {
        return new Vector3(currentGridPosition.x, 0, currentGridPosition.y);
    }

    public void SetGridPosition(Vector2Int gridPosition)
    {
        currentGridPosition = gridPosition;
        targetGridPosition = gridPosition;
        targetPosition = GridToPlayerWorldPosition(gridPosition);
        transform.position = targetPosition;
        isMoving = false;
        LogMovement("SetGridPosition called. currentGrid=" + currentGridPosition + ", world=" + transform.position);
    }

    private string GetMoveBlockReason(Vector2Int gridPosition)
    {
        if (gridManager == null)
            return "No GridManager";

        GridTile tile = gridManager.GetTile(gridPosition);
        if (tile == null)
            return "Outside grid";

        if (!tile.walkable)
            return "Tile not walkable";

        if (tile.hasObstacle)
            return "Tile has obstacle";

        return "Unknown";
    }

    private void LogMovement(string message)
    {
        if (!debugMovementLogs)
            return;

        Debug.Log("[BotiMovement] " + message);
    }
}
