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
                Debug.Log("Collected scrap");
                break;
            case InteractableType.Crystal:
                Debug.Log("Collected crystal");
                break;
            case InteractableType.Tree:
                Debug.Log("This tree looks old");
                break;
        }
    }
}