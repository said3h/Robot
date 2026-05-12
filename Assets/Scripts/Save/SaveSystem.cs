using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Saves and loads Boti's current prototype state as JSON.
/// Temporary controls: F5 saves, F9 loads.
/// </summary>
public class SaveSystem : MonoBehaviour
{
    public string fileName = "boti_save.json";

    public BotiPlayerController player;
    public BotiInventory inventory;
    public WorldState worldState;
    public GridManager gridManager;
    public WorldVisualSpawner visualSpawner;

    private string SavePath
    {
        get { return Path.Combine(Application.persistentDataPath, fileName); }
    }

    void Start()
    {
        FindReferences();
    }

    void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        if (keyboard.f5Key.wasPressedThisFrame)
            SaveGame();

        if (keyboard.f9Key.wasPressedThisFrame)
            LoadGame();
    }

    public void SaveGame()
    {
        FindReferences();

        if (player == null || inventory == null || worldState == null)
        {
            Debug.LogWarning("Cannot save: missing player, inventory, or world state.");
            return;
        }

        SaveData saveData = new SaveData();
        saveData.playerGridX = player.currentGridPosition.x;
        saveData.playerGridY = player.currentGridPosition.y;
        saveData.crystalCount = inventory.crystalCount;
        saveData.scrapCount = inventory.scrapCount;
        saveData.woodCount = inventory.woodCount;
        saveData.equippedTool = player.equippedTool;
        saveData.world = worldState.CreateSaveData();

        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(SavePath, json);
        Debug.Log("Game saved to: " + SavePath);
    }

    public void LoadGame()
    {
        FindReferences();

        if (!File.Exists(SavePath))
        {
            Debug.LogWarning("No save file found at: " + SavePath);
            return;
        }

        string json = File.ReadAllText(SavePath);
        SaveData saveData = JsonUtility.FromJson<SaveData>(json);

        if (saveData == null || saveData.world == null)
        {
            Debug.LogWarning("Save file is empty or invalid.");
            return;
        }

        worldState.LoadFromSaveData(saveData.world);

        if (gridManager != null)
            gridManager.ApplyWorldState(worldState);

        if (inventory != null)
            inventory.SetCounts(saveData.crystalCount, saveData.scrapCount, saveData.woodCount);

        if (visualSpawner != null)
            visualSpawner.RebuildVisuals(worldState, gridManager, inventory);

        if (player != null)
        {
            player.equippedTool = saveData.equippedTool;
            player.SetGridPosition(new Vector2Int(saveData.playerGridX, saveData.playerGridY));
            if (player.botiUI != null)
                player.botiUI.SetEquippedTool(player.equippedTool);
        }

        Debug.Log("Game loaded from: " + SavePath);
    }

    private void FindReferences()
    {
        if (player == null)
            player = FindObjectOfType<BotiPlayerController>();

        if (inventory == null)
            inventory = FindObjectOfType<BotiInventory>();

        if (worldState == null)
            worldState = FindObjectOfType<WorldState>();

        if (gridManager == null)
            gridManager = FindObjectOfType<GridManager>();

        if (visualSpawner == null)
            visualSpawner = FindObjectOfType<WorldVisualSpawner>();
    }
}
