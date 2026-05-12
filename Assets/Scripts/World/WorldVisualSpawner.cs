using UnityEngine;

/// <summary>
/// Builds visual GameObjects from WorldState data.
/// WorldState stays as the source of truth.
/// </summary>
public class WorldVisualSpawner : MonoBehaviour
{
    public void RebuildVisuals(WorldState worldState, GridManager gridManager, BotiInventory inventory)
    {
        if (worldState == null)
            return;

        DestroyIfExists("Terrain");
        DestroyIfExists("WorldObjects");

        if (gridManager != null)
            gridManager.ApplyWorldState(worldState);

        SpawnWorldVisuals(worldState, gridManager, inventory);
    }

    private void SpawnWorldVisuals(WorldState worldState, GridManager gridManager, BotiInventory inventory)
    {
        Material grassMat = Mat(new Color(0.3f, 0.6f, 0.25f));
        Material stoneMat = Mat(new Color(0.5f, 0.5f, 0.55f));
        Material waterMat = Mat(new Color(0.2f, 0.4f, 0.7f));
        Material pathMat = Mat(new Color(0.65f, 0.5f, 0.35f));
        Material darkGrassMat = Mat(new Color(0.25f, 0.5f, 0.2f));
        Material rockMat = Mat(new Color(0.6f, 0.6f, 0.65f));
        Material treeMat = Mat(new Color(0.3f, 0.5f, 0.2f));
        Material trunkMat = Mat(new Color(0.4f, 0.25f, 0.15f));
        Material junkMat = Mat(new Color(0.5f, 0.5f, 0.6f));
        Material crystalMat = Mat(new Color(0.6f, 0.2f, 0.8f));
        Material wallMat = Mat(new Color(0.35f, 0.35f, 0.38f));
        Material storageMat = Mat(new Color(0.75f, 0.45f, 0.18f));

        GameObject terrainParent = new GameObject("Terrain");
        GameObject objectsParent = new GameObject("WorldObjects");
        BotiFeedback feedback = FindObjectOfType<BotiFeedback>();

        foreach (WorldTileData tileData in worldState.tiles)
        {
            Vector3 worldPosition = worldState.GridToWorldPosition(tileData.coordinates);
            CreateTileVisual(tileData, worldPosition, terrainParent.transform, grassMat, stoneMat, waterMat, pathMat, darkGrassMat);

            if (tileData.obstacleType == WorldObstacleType.Rock)
                SpawnRock(tileData, worldPosition, objectsParent.transform, rockMat, gridManager, worldState, inventory, feedback);
            else if (tileData.obstacleType == WorldObstacleType.Tree)
                SpawnTree(tileData, worldPosition, objectsParent.transform, treeMat, trunkMat, gridManager, worldState, inventory, feedback);

            if (tileData.HasResource())
                SpawnResource(tileData, worldPosition, objectsParent.transform, junkMat, crystalMat, gridManager, worldState, inventory, feedback);

            if (tileData.HasStructure())
                SpawnStructure(tileData, worldPosition, objectsParent.transform, wallMat, storageMat);
        }
    }

    private void CreateTileVisual(WorldTileData tileData, Vector3 worldPosition, Transform parent, Material grassMat, Material stoneMat, Material waterMat, Material pathMat, Material darkGrassMat)
    {
        GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tile.name = "Tile";
        tile.transform.parent = parent;
        tile.transform.position = worldPosition;
        tile.transform.localScale = new Vector3(0.95f, 0.15f, 0.95f);

        if (tileData.tileType == WorldTileType.Water)
        {
            Paint(tile, waterMat);
            tile.transform.localScale = new Vector3(0.95f, 0.3f, 0.95f);
        }
        else if (tileData.tileType == WorldTileType.Path)
        {
            Paint(tile, pathMat);
        }
        else if (tileData.tileType == WorldTileType.Stone)
        {
            Paint(tile, stoneMat);
        }
        else if (tileData.tileType == WorldTileType.DarkGrass)
        {
            Paint(tile, darkGrassMat);
        }
        else
        {
            Paint(tile, grassMat);
        }
    }

    private void SpawnRock(WorldTileData tileData, Vector3 worldPosition, Transform parent, Material rockMat, GridManager gridManager, WorldState worldState, BotiInventory inventory, BotiFeedback feedback)
    {
        GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        rock.name = "Rock";
        rock.transform.parent = parent;
        rock.transform.position = worldPosition + new Vector3(0, 0.35f, 0);
        rock.transform.localScale = new Vector3(0.8f, 0.5f, 0.8f);
        Paint(rock, rockMat);

        Interactable rockInteractable = rock.AddComponent<Interactable>();
        rockInteractable.type = InteractableType.Rock;
        rockInteractable.gridManager = gridManager;
        rockInteractable.worldState = worldState;
        rockInteractable.visualSpawner = this;
        rockInteractable.inventory = inventory;
        rockInteractable.feedback = feedback;

        tileData.worldObject = rock;

        if (gridManager != null)
            gridManager.SetInteractableAtWorldPosition(worldPosition, rockInteractable);
    }

    private void SpawnTree(WorldTileData tileData, Vector3 worldPosition, Transform parent, Material treeMat, Material trunkMat, GridManager gridManager, WorldState worldState, BotiInventory inventory, BotiFeedback feedback)
    {
        GameObject treeRoot = new GameObject("Tree");
        treeRoot.transform.parent = parent;
        treeRoot.transform.position = worldPosition;

        GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trunk.name = "TreeTrunk";
        trunk.transform.parent = treeRoot.transform;
        trunk.transform.position = worldPosition + new Vector3(0, 0.8f, 0);
        trunk.transform.localScale = new Vector3(0.15f, 0.8f, 0.15f);
        Paint(trunk, trunkMat);

        Interactable treeInteractable = trunk.AddComponent<Interactable>();
        treeInteractable.type = InteractableType.Tree;
        treeInteractable.gridManager = gridManager;
        treeInteractable.worldState = worldState;
        treeInteractable.visualSpawner = this;
        treeInteractable.inventory = inventory;
        treeInteractable.feedback = feedback;

        GameObject foliage = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        foliage.name = "TreeFoliage";
        foliage.transform.parent = treeRoot.transform;
        foliage.transform.position = worldPosition + new Vector3(0, 2f, 0);
        foliage.transform.localScale = new Vector3(1.8f, 1.4f, 1.8f);
        Paint(foliage, treeMat);

        tileData.worldObject = treeRoot;

        if (gridManager != null)
            gridManager.SetInteractableAtWorldPosition(worldPosition, treeInteractable);
    }

    private void SpawnResource(WorldTileData tileData, Vector3 worldPosition, Transform parent, Material junkMat, Material crystalMat, GridManager gridManager, WorldState worldState, BotiInventory inventory, BotiFeedback feedback)
    {
        GameObject resource;
        InteractableType interactableType;

        if (tileData.resourceType == WorldResourceType.Crystal)
        {
            resource = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            resource.name = "Crystal";
            resource.transform.position = worldPosition + new Vector3(0, 0.4f, 0);
            resource.transform.localScale = new Vector3(0.15f, 0.8f, 0.15f);
            Paint(resource, crystalMat);
            interactableType = InteractableType.Crystal;
        }
        else
        {
            resource = GameObject.CreatePrimitive(PrimitiveType.Cube);
            resource.name = "RobotJunk";
            resource.transform.position = worldPosition + new Vector3(0, 0.2f, 0);
            resource.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            Paint(resource, junkMat);
            interactableType = InteractableType.Scrap;
        }

        resource.transform.parent = parent;
        Interactable interactable = resource.AddComponent<Interactable>();
        interactable.type = interactableType;
        interactable.gridManager = gridManager;
        interactable.worldState = worldState;
        interactable.visualSpawner = this;
        interactable.inventory = inventory;
        interactable.feedback = feedback;
        tileData.worldObject = resource;

        if (gridManager != null)
            gridManager.SetInteractableAtWorldPosition(worldPosition, interactable);
    }

    private void SpawnStructure(WorldTileData tileData, Vector3 worldPosition, Transform parent, Material wallMat, Material storageMat)
    {
        GameObject structure = GameObject.CreatePrimitive(PrimitiveType.Cube);
        structure.transform.parent = parent;

        if (tileData.structureType == WorldStructureType.Wall)
        {
            structure.name = "Wall";
            structure.transform.position = worldPosition + new Vector3(0, 0.5f, 0);
            structure.transform.localScale = new Vector3(0.9f, 1f, 0.9f);
            Paint(structure, wallMat);
        }
        else if (tileData.structureType == WorldStructureType.StorageBox)
        {
            structure.name = "StorageBox";
            structure.transform.position = worldPosition + new Vector3(0, 0.35f, 0);
            structure.transform.localScale = new Vector3(0.75f, 0.7f, 0.75f);
            Paint(structure, storageMat);
        }

        tileData.worldObject = structure;
    }

    private Material Mat(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null) shader = Shader.Find("Sprites/Default");

        Material mat = new Material(shader);
        mat.color = color;
        return mat;
    }

    private void Paint(GameObject obj, Material mat)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
            renderer.sharedMaterial = mat;
    }

    private void DestroyIfExists(string objectName)
    {
        GameObject existing = GameObject.Find(objectName);
        if (existing == null)
            return;

        if (Application.isPlaying)
            Destroy(existing);
        else
            DestroyImmediate(existing);
    }
}
