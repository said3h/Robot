using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

/// <summary>
/// Manages the crafting panel UI and recipe execution.
/// Toggle with C key, craft with number keys 1-9.
/// </summary>
public class CraftingSystem : MonoBehaviour
{
    [Header("UI References")]
    public GameObject craftingPanel;
    public Text recipeListText;
    public Text statusText;

    [Header("Recipe Assets")]
    public CraftRecipe[] recipes;

    [Header("System References")]
    public BotiInventory inventory;
    public BotiUI botiUI;
    public BotiFeedback feedback;

    private bool isOpen = false;
    private float statusClearTime = 0f;
    private const float statusDuration = 2f;

    void Start()
    {
        if (inventory == null)
            inventory = FindObjectOfType<BotiInventory>();
        if (botiUI == null)
            botiUI = FindObjectOfType<BotiUI>();
        if (feedback == null)
            feedback = FindObjectOfType<BotiFeedback>();

        if (craftingPanel != null)
            craftingPanel.SetActive(false);
    }

    void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        if (keyboard.cKey.wasPressedThisFrame)
            ToggleCraftingPanel();

        if (!isOpen)
            return;

        // Number keys 1-9 map to recipe indices 0-8
        if (keyboard.digit1Key.wasPressedThisFrame) TryCraft(0);
        else if (keyboard.digit2Key.wasPressedThisFrame) TryCraft(1);
        else if (keyboard.digit3Key.wasPressedThisFrame) TryCraft(2);
        else if (keyboard.digit4Key.wasPressedThisFrame) TryCraft(3);
        else if (keyboard.digit5Key.wasPressedThisFrame) TryCraft(4);
        else if (keyboard.digit6Key.wasPressedThisFrame) TryCraft(5);
        else if (keyboard.digit7Key.wasPressedThisFrame) TryCraft(6);
        else if (keyboard.digit8Key.wasPressedThisFrame) TryCraft(7);
        else if (keyboard.digit9Key.wasPressedThisFrame) TryCraft(8);

        if (statusClearTime > 0f && Time.time >= statusClearTime)
        {
            ClearStatus();
            statusClearTime = 0f;
        }
    }

    public void ToggleCraftingPanel()
    {
        isOpen = !isOpen;

        if (craftingPanel != null)
            craftingPanel.SetActive(isOpen);

        if (isOpen)
            RefreshRecipeList();
    }

    private void RefreshRecipeList()
    {
        if (recipeListText == null || recipes == null)
            return;

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("=== CRAFTING ===");
        sb.AppendLine();

        for (int i = 0; i < recipes.Length; i++)
        {
            CraftRecipe recipe = recipes[i];
            if (recipe == null)
                continue;

            bool canCraft = CanCraft(recipe);
            string status = canCraft ? "[OK]" : "(missing)";

            sb.AppendFormat("{0}: {1} x{2} <- ", i + 1, recipe.outputItem, recipe.outputAmount);

            for (int j = 0; j < recipe.ingredients.Length; j++)
            {
                var ing = recipe.ingredients[j];
                if (j > 0)
                    sb.Append(", ");
                sb.AppendFormat("{0} x{1}", ing.resource, ing.amount);
            }

            sb.AppendLine();
            sb.AppendFormat("  {0}", status);
            sb.AppendLine();
        }

        sb.AppendLine();
        sb.AppendLine("Press 1-9 to craft");
        sb.AppendLine("Press C to close");

        recipeListText.text = sb.ToString();
    }

    public bool CanCraft(CraftRecipe recipe)
    {
        if (recipe == null || recipe.ingredients == null)
            return false;

        foreach (var ing in recipe.ingredients)
        {
            switch (ing.resource)
            {
                case CraftResourceType.Wood:
                    if (inventory == null || inventory.woodCount < ing.amount)
                        return false;
                    break;
                case CraftResourceType.Scrap:
                    if (inventory == null || inventory.scrapCount < ing.amount)
                        return false;
                    break;
                case CraftResourceType.Crystal:
                    if (inventory == null || inventory.crystalCount < ing.amount)
                        return false;
                    break;
            }
        }
        return true;
    }

    public void TryCraft(int recipeIndex)
    {
        if (recipeIndex < 0 || recipeIndex >= recipes.Length)
        {
            ShowStatus("Invalid recipe");
            return;
        }

        CraftRecipe recipe = recipes[recipeIndex];
        if (recipe == null)
            return;

        if (!CanCraft(recipe))
        {
            if (feedback != null)
                feedback.PlayError();
            ShowStatus("Not enough resources");
            return;
        }

        // Spend ingredients
        foreach (var ing in recipe.ingredients)
        {
            switch (ing.resource)
            {
                case CraftResourceType.Wood:
                    inventory.woodCount -= ing.amount;
                    break;
                case CraftResourceType.Scrap:
                    inventory.scrapCount -= ing.amount;
                    break;
                case CraftResourceType.Crystal:
                    inventory.crystalCount -= ing.amount;
                    break;
            }
        }

        // Add output
        switch (recipe.outputItem)
        {
            case CraftResult.Plank:
                inventory.plankCount += recipe.outputAmount;
                break;
            case CraftResult.MetalPlate:
                inventory.metalPlateCount += recipe.outputAmount;
                break;
        }

        inventory.UpdateUI();

        if (feedback != null)
            feedback.PlayCollect();

        ShowStatus("Crafted " + recipe.outputItem + " x" + recipe.outputAmount);
        RefreshRecipeList();
    }

    private void ShowStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;

        statusClearTime = Time.time + statusDuration;
    }

    private void ClearStatus()
    {
        if (statusText != null)
            statusText.text = "";
    }
}