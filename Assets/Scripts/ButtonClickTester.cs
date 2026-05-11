using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Debug component to verify pointer click events reach UI buttons.
/// Attach to button GameObjects to test if pointer events are received.
/// </summary>
public class ButtonClickTester : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("POINTER CLICK RECEIVED BY: " + gameObject.name);
    }
}
