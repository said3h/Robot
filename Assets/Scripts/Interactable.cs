using System.Collections;
using UnityEngine;

/// <summary>
/// Types of interactable objects in the world.
/// </summary>
public enum InteractableType
{
    Scrap,
    Crystal,
    Tree,
    Rock
}

/// <summary>
/// Marks an object as interactable in the world.
/// </summary>
public class Interactable : MonoBehaviour
{
    public InteractableType type;
    public string interactionPrompt = "Press E";
    public BotiInventory inventory;
    public GridManager gridManager;
    public WorldState worldState;
    public WorldVisualSpawner visualSpawner;
    public BotiFeedback feedback;
    public float treeLogCooldown = 1f;

    private float lastTreeLogTime = -100f;
    private bool isBeingCollected = false;

    public string GetInteractionMessage()
    {
        switch (type)
        {
            case InteractableType.Scrap:
                return "Press E to collect scrap";
            case InteractableType.Crystal:
                return "Press E to collect crystal";
            case InteractableType.Tree:
                return "Press E to chop tree";
            case InteractableType.Rock:
                return "Press E to mine rock";
            default:
                return "Press E to interact";
        }
    }

    public string Interact(BotiTool equippedTool)
    {
        if (isBeingCollected)
            return "";

        switch (type)
        {
            case InteractableType.Scrap:
                if (inventory != null && !inventory.HasInventorySpace())
                    return "Inventory Full";
                if (inventory != null)
                    inventory.AddScrap(1);
                if (worldState != null)
                    worldState.CollectResourceAtWorldPosition(transform.position);
                if (gridManager != null)
                    gridManager.ClearInteractableAtWorldPosition(transform.position);
                PlayCollectFeedback("+1 Scrap");
                Debug.Log("Collected scrap");
                StartCoroutine(CollectAndDestroy(false));
                return "";
            case InteractableType.Crystal:
                if (equippedTool != BotiTool.Pickaxe)
                {
                    PlayErrorFeedback();
                    return "Need Pickaxe";
                }
                if (inventory != null && !inventory.HasInventorySpace())
                    return "Inventory Full";
                if (inventory != null)
                    inventory.AddCrystal(1);
                if (worldState != null)
                    worldState.CollectResourceAtWorldPosition(transform.position);
                if (gridManager != null)
                    gridManager.ClearInteractableAtWorldPosition(transform.position);
                PlayCollectFeedback("+1 Crystal");
                Debug.Log("Collected crystal");
                StartCoroutine(CollectAndDestroy(false));
                return "";
            case InteractableType.Tree:
                if (equippedTool != BotiTool.Axe)
                {
                    PlayErrorFeedback();
                    return "Need Axe";
                }
                if (inventory != null && !inventory.HasInventorySpace())
                    return "Inventory Full";
                if (inventory != null)
                    inventory.AddWood(1);
                PlayCollectFeedback("+1 Wood");
                RemoveObstacleFromWorld();
                return "";
            case InteractableType.Rock:
                if (equippedTool != BotiTool.Pickaxe)
                {
                    PlayErrorFeedback();
                    return "Need Pickaxe";
                }
                if (inventory != null && !inventory.HasInventorySpace())
                    return "Inventory Full";
                if (inventory != null)
                    inventory.AddScrap(1);
                PlayCollectFeedback("+1 Scrap");
                RemoveObstacleFromWorld();
                return "";
        }

        return "";
    }

    public string Interact()
    {
        return Interact(BotiTool.Axe);
    }

    private void RemoveObstacleFromWorld()
    {
        isBeingCollected = true;

        if (worldState != null)
            worldState.RemoveObstacleAtWorldPosition(transform.position);

        if (gridManager != null && worldState != null)
            gridManager.ApplyWorldState(worldState);

        StartCoroutine(CollectAndDestroy(true));
    }

    private IEnumerator CollectAndDestroy(bool rebuildWorldVisuals)
    {
        isBeingCollected = true;

        GameObject target = gameObject;
        if (transform.parent != null && transform.parent.name == "Tree")
            target = transform.parent.gameObject;

        Vector3 startScale = target.transform.localScale;
        Vector3 popScale = startScale * 1.15f;
        float duration = 0.18f;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            target.transform.localScale = Vector3.Lerp(startScale, popScale, t);
            yield return null;
        }

        timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            target.transform.localScale = Vector3.Lerp(popScale, Vector3.zero, t);
            yield return null;
        }

        if (rebuildWorldVisuals && visualSpawner != null && worldState != null)
            visualSpawner.RebuildVisuals(worldState, gridManager, inventory);
        else
            Destroy(target);
    }

    private void PlayCollectFeedback(string message)
    {
        if (feedback == null)
            feedback = FindObjectOfType<BotiFeedback>();

        if (feedback == null)
            return;

        feedback.ShowFloatingText(message, transform.position);
        feedback.PlayCollect();
    }

    private void PlayErrorFeedback()
    {
        if (feedback == null)
            feedback = FindObjectOfType<BotiFeedback>();

        if (feedback != null)
            feedback.PlayError();
    }
}
