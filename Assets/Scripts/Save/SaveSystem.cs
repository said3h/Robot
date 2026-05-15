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
        {
            Debug.LogWarning("SaveSystem: Keyboard.current is null — check Input System package");
            return;
        }

        if (keyboard.kKey.wasPressedThisFrame)
        {
            Debug.Log("[SaveSystem] K pressed — calling SaveGame()");
            SaveGame();
        }

        if (keyboard.lKey.wasPressedThisFrame)
        {
            Debug.Log("[SaveSystem] L pressed — calling LoadGame()");
            LoadGame();
        }
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
        saveData.plankCount = inventory.plankCount;
        saveData.metalPlateCount = inventory.metalPlateCount;
        saveData.equippedTool = player.equippedTool;
        saveData.world = worldState.CreateSaveData();

        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(SavePath, json);
        Debug.Log("[SaveGame] Saved to: " + SavePath + " — JSON size: " + json.Length + " chars");
    }

    public void LoadGame()
    {
        Debug.Log("[LoadGame] Started");
        FindReferences();

        Debug.Log("[LoadGame] SavePath: " + SavePath);
        Debug.Log("[LoadGame] File exists: " + File.Exists(SavePath));

        if (!File.Exists(SavePath))
        {
            Debug.LogWarning("[LoadGame] No save file found.");
            return;
        }

        string json = File.ReadAllText(SavePath);
        Debug.Log("[LoadGame] JSON read, length: " + json.Length);

        if (string.IsNullOrEmpty(json))
        {
            Debug.LogWarning("[LoadGame] JSON is empty.");
            return;
        }

        SaveData saveData = JsonUtility.FromJson<SaveData>(json);
        Debug.Log("[LoadGame] saveData parsed, world null?: " + (saveData == null || saveData.world == null));

        if (saveData == null || saveData.world == null)
        {
            Debug.LogWarning("[LoadGame] Save file is empty or invalid.");
            return;
        }

        Debug.Log("[LoadGame] Restoring WorldState...");
        worldState.LoadFromSaveData(saveData.world);
        Debug.Log("[LoadGame] WorldState restored. Tiles loaded: " + (saveData.world != null ? saveData.world.tiles.Count : 0));

        if (gridManager != null)
        {
            Debug.Log("[LoadGame] Applying WorldState to GridManager...");
            gridManager.ApplyWorldState(worldState);
            Debug.Log("[LoadGame] GridManager applied.");
        }
        else
        {
            Debug.LogWarning("[LoadGame] GridManager is null!");
        }

        if (inventory != null)
        {
            Debug.Log("[LoadGame] Restoring inventory: crystal=" + saveData.crystalCount + " scrap=" + saveData.scrapCount + " wood=" + saveData.woodCount + " plank=" + saveData.plankCount + " metalPlate=" + saveData.metalPlateCount);
            inventory.SetCounts(saveData.crystalCount, saveData.scrapCount, saveData.woodCount,
                saveData.plankCount, saveData.metalPlateCount);
            Debug.Log("[LoadGame] Inventory restored.");
        }
        else
        {
            Debug.LogWarning("[LoadGame] Inventory is null!");
        }

        if (visualSpawner != null)
        {
            Debug.Log("[LoadGame] Rebuilding visuals...");
            visualSpawner.RebuildVisuals(worldState, gridManager, inventory);
            Debug.Log("[LoadGame] Visuals rebuilt.");
        }
        else
        {
            Debug.LogWarning("[LoadGame] VisualSpawner is null!");
        }

        if (player != null)
        {
            Debug.Log("[LoadGame] Restoring player to grid (" + saveData.playerGridX + "," + saveData.playerGridY + ")");
            player.equippedTool = saveData.equippedTool;
            player.SetGridPosition(new Vector2Int(saveData.playerGridX, saveData.playerGridY));
            if (player.botiUI != null)
                player.botiUI.SetEquippedTool(player.equippedTool);
            Debug.Log("[LoadGame] Player restored. Current world pos: " + player.transform.position);
        }
        else
        {
            Debug.LogWarning("[LoadGame] Player is null!");
        }

        Debug.Log("[LoadGame] Completed successfully.");
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
