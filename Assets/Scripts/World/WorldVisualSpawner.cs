using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Builds visual GameObjects from WorldState data.
/// WorldState stays as the source of truth. Visual improvements only.
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
        GameObject terrainParent = new GameObject("Terrain");
        GameObject objectsParent = new GameObject("WorldObjects");
        BotiFeedback feedback = FindObjectOfType<BotiFeedback>();

        foreach (WorldTileData tileData in worldState.tiles)
        {
            Vector3 worldPosition = worldState.GridToWorldPosition(tileData.coordinates);
            CreateTileVisual(tileData, worldPosition, terrainParent.transform, worldState);

            if (tileData.obstacleType == WorldObstacleType.Rock)
                SpawnRock(tileData, worldPosition, objectsParent.transform, gridManager, worldState, inventory, feedback);
            else if (tileData.obstacleType == WorldObstacleType.Tree)
                SpawnTree(tileData, worldPosition, objectsParent.transform, gridManager, worldState, inventory, feedback);

            if (tileData.HasResource())
                SpawnResource(tileData, worldPosition, objectsParent.transform, gridManager, worldState, inventory, feedback);

            if (tileData.HasStructure())
                SpawnStructure(tileData, worldPosition, objectsParent.transform);
        }
    }

    private void CreateTileVisual(WorldTileData tileData, Vector3 worldPosition, Transform parent, WorldState worldState)
    {
        GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tile.name = "Tile";
        tile.transform.parent = parent;
        tile.transform.position = worldPosition;
        tile.transform.localScale = new Vector3(0.95f, 0.15f, 0.95f);

        bool isEdge = IsEdgeTile(tileData, worldState);

        if (tileData.tileType == WorldTileType.Water)
        {
            tile.transform.localScale = new Vector3(0.95f, 0.3f, 0.95f);
            float noise = Mathf.PerlinNoise(tileData.coordinates.x * 0.3f, tileData.coordinates.y * 0.3f);
            Color waterColor = Color.Lerp(new Color(0.15f, 0.35f, 0.65f), new Color(0.25f, 0.5f, 0.8f), noise);
            Paint(tile, WaterMat(waterColor));
        }
        else if (tileData.tileType == WorldTileType.Path)
        {
            Paint(tile, PathMat(isEdge));
        }
        else if (tileData.tileType == WorldTileType.Stone)
        {
            Paint(tile, StoneMat(isEdge));
        }
        else if (tileData.tileType == WorldTileType.DarkGrass)
        {
            Paint(tile, DarkGrassMat(isEdge));
        }
        else
        {
            float noise = Mathf.PerlinNoise(tileData.coordinates.x * 0.25f + 3.7f, tileData.coordinates.y * 0.25f + 1.2f);
            Color grassColor = Color.Lerp(new Color(0.25f, 0.55f, 0.2f), new Color(0.38f, 0.68f, 0.28f), noise);
            Paint(tile, GrassMat(grassColor));
        }
    }

    private bool IsEdgeTile(WorldTileData tileData, WorldState worldState)
    {
        Vector2Int[] neighbors = {
            new Vector2Int(0, 1), new Vector2Int(0, -1),
            new Vector2Int(1, 0), new Vector2Int(-1, 0)
        };
        foreach (var n in neighbors)
        {
            var neighbor = worldState.GetTile(tileData.coordinates + n);
            if (neighbor != null && neighbor.tileType != tileData.tileType)
                return true;
        }
        return false;
    }

    private void SpawnRock(WorldTileData tileData, Vector3 worldPosition, Transform parent, GridManager gridManager, WorldState worldState, BotiInventory inventory, BotiFeedback feedback)
    {
        int parts = Random.Range(2, 4);
        GameObject rockRoot = new GameObject("Rock");
        rockRoot.transform.parent = parent;
        rockRoot.transform.position = worldPosition;

        GameObject mainRock = null;

        for (int i = 0; i < parts; i++)
        {
            GameObject rockPart = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rockPart.name = "RockPart" + i;
            rockPart.transform.parent = rockRoot.transform;

            float offsetX = Random.Range(-0.2f, 0.2f);
            float offsetZ = Random.Range(-0.2f, 0.2f);
            float yPos = Random.Range(0.15f, 0.4f);

            rockPart.transform.position = worldPosition + new Vector3(offsetX, yPos, offsetZ);

            float scaleX = Random.Range(0.35f, 0.7f);
            float scaleY = Random.Range(0.25f, 0.55f);
            float scaleZ = Random.Range(0.35f, 0.7f);
            rockPart.transform.localScale = new Vector3(scaleX, scaleY, scaleZ);

            rockPart.transform.rotation = Quaternion.Euler(
                Random.Range(-15f, 15f),
                Random.Range(0f, 360f),
                Random.Range(-15f, 15f)
            );

            Paint(rockPart, RockMat());

            if (i == 0) mainRock = rockPart;
        }

        if (mainRock == null) mainRock = rockRoot;

        Interactable rockInteractable = rockRoot.AddComponent<Interactable>();
        rockInteractable.type = InteractableType.Rock;
        rockInteractable.gridManager = gridManager;
        rockInteractable.worldState = worldState;
        rockInteractable.visualSpawner = this;
        rockInteractable.inventory = inventory;
        rockInteractable.feedback = feedback;

        tileData.worldObject = rockRoot;

        if (gridManager != null)
            gridManager.SetInteractableAtWorldPosition(worldPosition, rockInteractable);
    }

    private void SpawnTree(WorldTileData tileData, Vector3 worldPosition, Transform parent, GridManager gridManager, WorldState worldState, BotiInventory inventory, BotiFeedback feedback)
    {
        GameObject treeRoot = new GameObject("Tree");
        treeRoot.transform.parent = parent;
        treeRoot.transform.position = worldPosition;

        float trunkHeight = Random.Range(0.6f, 1.0f);
        float trunkRadius = Random.Range(0.1f, 0.16f);

        GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trunk.name = "Trunk";
        trunk.transform.parent = treeRoot.transform;
        trunk.transform.position = worldPosition + new Vector3(0, trunkHeight * 0.5f, 0);
        trunk.transform.localScale = new Vector3(trunkRadius * 2f, trunkHeight * 0.5f, trunkRadius * 2f);
        trunk.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), Random.Range(-4f, 4f));
        Paint(trunk, TrunkMat());

        Interactable treeInteractable = trunk.AddComponent<Interactable>();
        treeInteractable.type = InteractableType.Tree;
        treeInteractable.gridManager = gridManager;
        treeInteractable.worldState = worldState;
        treeInteractable.visualSpawner = this;
        treeInteractable.inventory = inventory;
        treeInteractable.feedback = feedback;

        int foliageCount = Random.Range(2, 5);
        float foliageNoise = Random.Range(0f, 1f);
        Color foliageColor = Color.Lerp(new Color(0.2f, 0.48f, 0.15f), new Color(0.35f, 0.58f, 0.22f), foliageNoise);

        for (int i = 0; i < foliageCount; i++)
        {
            GameObject foliage = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            foliage.name = "Foliage" + i;
            foliage.transform.parent = treeRoot.transform;

            float angle = Random.Range(0f, Mathf.PI * 2f);
            float radius = Random.Range(0.15f, 0.55f);
            float yOffset = Random.Range(1.4f, 2.3f);
            float scale = Random.Range(0.45f, 1.1f);

            foliage.transform.position = worldPosition + new Vector3(
                Mathf.Cos(angle) * radius,
                yOffset,
                Mathf.Sin(angle) * radius
            );
            foliage.transform.localScale = new Vector3(scale * 1.2f, scale * 0.9f, scale * 1.2f);
            Paint(foliage, FoliageMat(foliageColor));
        }

        tileData.worldObject = treeRoot;

        if (gridManager != null)
            gridManager.SetInteractableAtWorldPosition(worldPosition, treeInteractable);
    }

    private void SpawnResource(WorldTileData tileData, Vector3 worldPosition, Transform parent, GridManager gridManager, WorldState worldState, BotiInventory inventory, BotiFeedback feedback)
    {
        if (tileData.resourceType == WorldResourceType.Crystal)
        {
            GameObject crystalRoot = new GameObject("Crystal");
            crystalRoot.transform.parent = parent;
            crystalRoot.transform.position = worldPosition + new Vector3(0, 0.05f, 0);

            int count = Random.Range(2, 4);

            for (int i = 0; i < count; i++)
            {
                GameObject crystal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                crystal.name = "CrystalShard" + i;
                crystal.transform.parent = crystalRoot.transform;

                float angle = (i / (float)count) * Mathf.PI * 2f + Random.Range(-0.3f, 0.3f);
                float radius = Random.Range(0.04f, 0.13f);
                float height = Random.Range(0.4f, 0.9f);

                crystal.transform.position = worldPosition + new Vector3(
                    Mathf.Cos(angle) * radius,
                    Random.Range(0.25f, 0.45f),
                    Mathf.Sin(angle) * radius
                );
                crystal.transform.localScale = new Vector3(
                    Random.Range(0.06f, 0.1f),
                    height * 0.5f,
                    Random.Range(0.06f, 0.1f)
                );
                crystal.transform.rotation = Quaternion.Euler(0, angle * Mathf.Rad2Deg, Random.Range(-8f, 8f));

                Paint(crystal, CrystalMat());
            }

            Interactable interactable = crystalRoot.AddComponent<Interactable>();
            interactable.type = InteractableType.Crystal;
            interactable.gridManager = gridManager;
            interactable.worldState = worldState;
            interactable.visualSpawner = this;
            interactable.inventory = inventory;
            interactable.feedback = feedback;

            tileData.worldObject = crystalRoot;

            if (gridManager != null)
                gridManager.SetInteractableAtWorldPosition(worldPosition, interactable);
        }
        else
        {
            GameObject scrapRoot = new GameObject("RobotJunk");
            scrapRoot.transform.parent = parent;
            scrapRoot.transform.position = worldPosition + new Vector3(0, 0.08f, 0);

            int count = Random.Range(3, 6);

            for (int i = 0; i < count; i++)
            {
                PrimitiveType type = Random.value > 0.5f ? PrimitiveType.Cube : PrimitiveType.Sphere;
                GameObject piece = GameObject.CreatePrimitive(type);
                piece.name = "ScrapPiece" + i;
                piece.transform.parent = scrapRoot.transform;

                piece.transform.position = new Vector3(
                    Random.Range(-0.12f, 0.12f),
                    Random.Range(0f, 0.18f),
                    Random.Range(-0.12f, 0.12f)
                );

                piece.transform.rotation = Quaternion.Euler(
                    Random.Range(0f, 360f),
                    Random.Range(0f, 360f),
                    Random.Range(0f, 360f)
                );

                float scale = Random.Range(0.07f, 0.13f);
                piece.transform.localScale = new Vector3(
                    scale,
                    scale * Random.Range(0.5f, 1.4f),
                    scale
                );

                Paint(piece, ScrapMat());
            }

            Interactable interactable = scrapRoot.AddComponent<Interactable>();
            interactable.type = InteractableType.Scrap;
            interactable.gridManager = gridManager;
            interactable.worldState = worldState;
            interactable.visualSpawner = this;
            interactable.inventory = inventory;
            interactable.feedback = feedback;

            tileData.worldObject = scrapRoot;

            if (gridManager != null)
                gridManager.SetInteractableAtWorldPosition(worldPosition, interactable);
        }
    }

    private void SpawnStructure(WorldTileData tileData, Vector3 worldPosition, Transform parent)
    {
        GameObject structure = GameObject.CreatePrimitive(PrimitiveType.Cube);
        structure.transform.parent = parent;

        if (tileData.structureType == WorldStructureType.Wall)
        {
            structure.name = "Wall";
            structure.transform.position = worldPosition + new Vector3(0, 0.5f, 0);
            structure.transform.localScale = new Vector3(0.9f, 1f, 0.9f);
            Paint(structure, WallMat());
        }
        else if (tileData.structureType == WorldStructureType.StorageBox)
        {
            structure.name = "StorageBox";
            structure.transform.position = worldPosition + new Vector3(0, 0.35f, 0);
            structure.transform.localScale = new Vector3(0.75f, 0.7f, 0.75f);
            Paint(structure, StorageMat());
        }

        tileData.worldObject = structure;
    }

    // ─── Material helpers ───────────────────────────────────────────────

    private Material Mat(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null) shader = Shader.Find("Sprites/Default");

        Material mat = new Material(shader);
        mat.color = color;
        return mat;
    }

    private Material GrassMat(Color color)
    {
        Material mat = Mat(color);
        mat.SetFloat("_Smoothness", 0.35f);
        return mat;
    }

    private Material WaterMat(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null) shader = Shader.Find("Sprites/Default");

        Material mat = new Material(shader);
        mat.color = color;
        mat.SetFloat("_Surface", 1f);
        mat.SetFloat("_Blend", 0f);
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        mat.renderQueue = 3000;
        return mat;
    }

    private Material PathMat(bool isEdge)
    {
        Color color = isEdge
            ? new Color(0.5f, 0.38f, 0.25f)
            : new Color(0.65f, 0.5f, 0.35f);
        return Mat(color);
    }

    private Material StoneMat(bool isEdge)
    {
        Color color = isEdge
            ? new Color(0.38f, 0.38f, 0.42f)
            : new Color(0.5f, 0.5f, 0.55f);
        return Mat(color);
    }

    private Material DarkGrassMat(bool isEdge)
    {
        Color color = isEdge
            ? new Color(0.14f, 0.28f, 0.1f)
            : new Color(0.2f, 0.4f, 0.15f);
        return Mat(color);
    }

    private Material RockMat()
    {
        return Mat(new Color(0.55f, 0.55f, 0.6f));
    }

    private Material TrunkMat()
    {
        return Mat(new Color(0.38f, 0.23f, 0.13f));
    }

    private Material FoliageMat(Color color)
    {
        Material mat = Mat(color);
        mat.SetFloat("_Smoothness", 0.1f);
        return mat;
    }

    private Material CrystalMat()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null) shader = Shader.Find("Sprites/Default");

        Material mat = new Material(shader);
        mat.color = new Color(0.85f, 0.35f, 1f);
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", new Color(0.5f, 0.15f, 0.7f));
        return mat;
    }

    private Material ScrapMat()
    {
        Material mat = Mat(new Color(0.55f, 0.55f, 0.65f));
        mat.SetFloat("_Smoothness", 0.55f);
        return mat;
    }

    private Material WallMat()
    {
        return Mat(new Color(0.4f, 0.4f, 0.44f));
    }

    private Material StorageMat()
    {
        return Mat(new Color(0.7f, 0.42f, 0.18f));
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