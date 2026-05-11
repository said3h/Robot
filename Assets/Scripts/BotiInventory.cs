using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple inventory for tracking collected resources.
/// </summary>
public class BotiInventory : MonoBehaviour
{
    public int scrapCount = 0;
    public int crystalCount = 0;

    [Header("UI References")]
    public Text scrapText;
    public Text crystalText;

    public void AddScrap(int amount = 1)
    {
        scrapCount += amount;
        UpdateUI();
    }

    public void AddCrystal(int amount = 1)
    {
        crystalCount += amount;
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (scrapText != null)
            scrapText.text = "Scrap: " + scrapCount;

        if (crystalText != null)
            crystalText.text = "Crystal: " + crystalCount;
    }
}