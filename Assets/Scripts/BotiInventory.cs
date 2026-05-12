using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple inventory for tracking collected resources.
/// </summary>
public class BotiInventory : MonoBehaviour
{
    public int scrapCount = 0;
    public int crystalCount = 0;
    public int woodCount = 0;

    [Header("UI References")]
    public Text scrapText;
    public Text crystalText;
    public Text woodText;

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

    public void AddWood(int amount = 1)
    {
        woodCount += amount;
        UpdateUI();
    }

    public void SetCounts(int crystal, int scrap, int wood)
    {
        crystalCount = crystal;
        scrapCount = scrap;
        woodCount = wood;
        UpdateUI();
    }

    public bool HasResources(int crystal, int scrap, int wood)
    {
        return crystalCount >= crystal && scrapCount >= scrap && woodCount >= wood;
    }

    public bool SpendResources(int crystal, int scrap, int wood)
    {
        if (!HasResources(crystal, scrap, wood))
            return false;

        crystalCount -= crystal;
        scrapCount -= scrap;
        woodCount -= wood;
        UpdateUI();
        return true;
    }

    public bool HasInventorySpace()
    {
        return true;
    }

    public void UpdateUI()
    {
        if (crystalText != null)
            crystalText.text = "Crystal: " + crystalCount;

        if (scrapText != null)
            scrapText.text = "Scrap: " + scrapCount;

        if (woodText != null)
            woodText.text = "Wood: " + woodCount;
    }
}
