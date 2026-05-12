using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple UI helper for inventory text and interaction messages.
/// </summary>
public class BotiUI : MonoBehaviour
{
    public Text interactionText;
    public Text buildText;
    public Text toolText;
    public float messageTime = 1.5f;

    private float hideMessageAt;

    public bool IsShowingMessage()
    {
        return hideMessageAt > 0f;
    }

    void Update()
    {
        if (interactionText != null && hideMessageAt > 0f && Time.time >= hideMessageAt)
        {
            interactionText.gameObject.SetActive(false);
            hideMessageAt = 0f;
        }
    }

    public void ShowPrompt(string message)
    {
        if (interactionText == null)
            return;

        interactionText.text = message;
        interactionText.gameObject.SetActive(true);
        hideMessageAt = 0f;
    }

    public void ShowMessage(string message)
    {
        if (interactionText == null)
            return;

        interactionText.text = message;
        interactionText.gameObject.SetActive(true);
        hideMessageAt = Time.time + messageTime;
    }

    public void HidePrompt()
    {
        if (interactionText != null)
            interactionText.gameObject.SetActive(false);

        hideMessageAt = 0f;
    }

    public void SetBuildInfo(bool buildMode, WorldStructureType selectedStructure)
    {
        if (buildText == null)
            return;

        string modeText = buildMode ? "Build Mode: ON" : "Build Mode: OFF";
        buildText.text = modeText + "\nSelected: " + selectedStructure;
    }

    public void SetEquippedTool(BotiTool equippedTool)
    {
        if (toolText == null)
            return;

        toolText.text = "Equipped Tool: " + equippedTool;
    }
}
