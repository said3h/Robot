using UnityEngine;

/// <summary>
/// Types of interactable objects in the world.
/// </summary>
public enum InteractableType
{
    Scrap,
    Crystal,
    Tree
}

/// <summary>
/// Marks an object as interactable in the world.
/// </summary>
public class Interactable : MonoBehaviour
{
    public InteractableType type;
    public string interactionPrompt = "Press E";
    public BotiInventory inventory;

    public string GetInteractionMessage()
    {
        switch (type)
        {
            case InteractableType.Scrap:
                return "Press E to collect scrap";
            case InteractableType.Crystal:
                return "Press E to collect crystal";
            case InteractableType.Tree:
                return "Press E to inspect tree";
            default:
                return "Press E to interact";
        }
    }

    public void Interact()
    {
        switch (type)
        {
            case InteractableType.Scrap:
                if (inventory != null)
                    inventory.AddScrap(1);
                Debug.Log("Collected scrap");
                Destroy(gameObject);
                break;
            case InteractableType.Crystal:
                if (inventory != null)
                    inventory.AddCrystal(1);
                Debug.Log("Collected crystal");
                Destroy(gameObject);
                break;
            case InteractableType.Tree:
                Debug.Log("This tree looks old");
                break;
        }
    }
}