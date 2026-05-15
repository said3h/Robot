using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public static class BotiSceneBuilder
{
    [MenuItem("Boti/Build Scene")]
    public static void BuildScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        CreateCamera();
        CreateLight();
        CreateWorld();
        CreateUI();
        CreateBoti();
        CreateEventSystem();

        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            AssetDatabase.CreateFolder("Assets", "Scenes");

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/BotiGame.unity");

        EditorUtility.DisplayDialog("Boti", "Exploration sandbox created!", "OK");
    }

    static void CreateEventSystem()
    {
        EventSystem[] existingEventSystems = Object.FindObjectsOfType<EventSystem>();
        foreach (EventSystem es in existingEventSystems)
        {
            Object.DestroyImmediate(es.gameObject);
        }

        GameObject eventSystemObj = new GameObject("EventSystem");
        eventSystemObj.AddComponent<EventSystem>();
        eventSystemObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
    }

    static void CreateCamera()
    {
        RemoveExistingCameras();

        GameObject camObj = new GameObject("MainCamera");
        Camera cam = camObj.AddComponent<Camera>();
        camObj.AddComponent<AudioListener>();
        camObj.tag = "MainCamera";
        camObj.transform.parent = null;
        cam.transform.position = new Vector3(0, 26, -18);
        cam.transform.rotation = Quaternion.Euler(60, 0, 0);
        cam.orthographic = true;
        cam.orthographicSize = 15;
        cam.backgroundColor = new Color(0.55f, 0.7f, 0.85f);

        CameraFollow camFollow = camObj.AddComponent<CameraFollow>();
        camFollow.fixedPosition = new Vector3(0, 26, -18);
        camFollow.fixedRotation = new Vector3(60, 0, 0);
        camFollow.orthographicSize = 15;
        camFollow.ApplyFixedCamera();
    }

    static void RemoveExistingCameras()
    {
        Camera[] cameras = Object.FindObjectsOfType<Camera>();
        foreach (Camera camera in cameras)
        {
            if (camera != null)
                Object.DestroyImmediate(camera.gameObject);
        }
    }

    static void CreateLight()
    {
        GameObject lightObj = new GameObject("Directional Light");
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.2f;
        light.color = new Color(1f, 0.95f, 0.85f);
        // light.shadowIntensity = 0.4f;
        // light.shadowDistance = 30f;
        lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.3f, 0.35f, 0.4f);
    }

    static Material Mat(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null) shader = Shader.Find("Sprites/Default");
        Material mat = new Material(shader);
        mat.color = color;
        return mat;
    }

    static void Paint(GameObject obj, Material mat)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null) renderer.sharedMaterial = mat;
    }

    static void CreateWorld()
    {
        int worldSize = 10;
        WorldState worldState = CreateWorldState(worldSize);
        GridManager gridManager = CreateGridManager(worldSize);

        GenerateWorldData(worldState);
        gridManager.ApplyWorldState(worldState);
        SpawnWorldVisuals(worldState, gridManager);
    }

    static WorldState CreateWorldState(int worldSize)
    {
        GameObject worldStateObj = new GameObject("WorldState");
        WorldState worldState = worldStateObj.AddComponent<WorldState>();
        worldStateObj.AddComponent<WorldVisualSpawner>();
        int size = worldSize * 2 + 1;
        worldState.CreateEmptyWorld(size, size, 1f, new Vector3(-worldSize, 0, -worldSize));
        return worldState;
    }

    static GridManager CreateGridManager(int worldSize)
    {
        GameObject gridObj = new GameObject("GridManager");
        GridManager gridManager = gridObj.AddComponent<GridManager>();
        gridManager.width = worldSize * 2 + 1;
        gridManager.height = worldSize * 2 + 1;
        gridManager.gridSize = new Vector2(gridManager.width, gridManager.height);
        gridManager.tileSize = 1f;
        gridManager.originPosition = new Vector3(-worldSize, 0, -worldSize);
        gridManager.CreateGrid();
        return gridManager;
    }

    static void GenerateWorldData(WorldState worldState)
    {
        System.Random rng = new System.Random(42);

        foreach (WorldTileData tile in worldState.tiles)
        {
            Vector3 worldPosition = worldState.GridToWorldPosition(tile.coordinates);
            float noise = Mathf.PerlinNoise(worldPosition.x * 0.2f, worldPosition.z * 0.2f);

            if (noise < 0.2f)
                worldState.SetTileType(tile.coordinates, WorldTileType.Water, false);
            else if (noise < 0.3f)
                worldState.SetTileType(tile.coordinates, WorldTileType.Path, true);
            else if (noise < 0.5f)
                worldState.SetTileType(tile.coordinates, WorldTileType.Stone, true);
            else if (noise < 0.7f)
                worldState.SetTileType(tile.coordinates, WorldTileType.DarkGrass, true);
            else
                worldState.SetTileType(tile.coordinates, WorldTileType.Grass, true);
        }

        AddObstacleData(worldState, rng);
        AddResourceData(worldState, rng);
    }

    static void AddObstacleData(WorldState worldState, System.Random rng)
    {
        for (int i = 0; i < 25; i++)
        {
            Vector2Int position = RandomWorldPosition(worldState, rng, -8, 9);
            if (IsNearStart(position, worldState))
                continue;

            worldState.SetObstacle(position, WorldObstacleType.Rock);
        }

        for (int i = 0; i < 15; i++)
        {
            Vector2Int position = RandomWorldPosition(worldState, rng, -8, 9);
            if (IsNearStart(position, worldState))
                continue;

            worldState.SetObstacle(position, WorldObstacleType.Tree);
        }
    }

    static void AddResourceData(WorldState worldState, System.Random rng)
    {
        for (int i = 0; i < 10; i++)
            worldState.SetResource(RandomWorldPosition(worldState, rng, -9, 10), WorldResourceType.Scrap);

        for (int i = 0; i < 8; i++)
            worldState.SetResource(RandomWorldPosition(worldState, rng, -8, 9), WorldResourceType.Crystal);
    }

    static Vector2Int RandomWorldPosition(WorldState worldState, System.Random rng, int min, int max)
    {
        int x = rng.Next(min, max);
        int z = rng.Next(min, max);
        return worldState.WorldToGridPosition(new Vector3(x, 0, z));
    }

    static bool IsNearStart(Vector2Int position, WorldState worldState)
    {
        Vector2Int start = worldState.WorldToGridPosition(Vector3.zero);
        return Mathf.Abs(position.x - start.x) < 2 && Mathf.Abs(position.y - start.y) < 2;
    }

    static void SpawnWorldVisuals(WorldState worldState, GridManager gridManager)
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

        GameObject terrainParent = new GameObject("Terrain");
        GameObject objectsParent = new GameObject("WorldObjects");

        foreach (WorldTileData tileData in worldState.tiles)
        {
            Vector3 worldPosition = worldState.GridToWorldPosition(tileData.coordinates);
            CreateTileVisual(tileData, worldPosition, terrainParent.transform, grassMat, stoneMat, waterMat, pathMat, darkGrassMat);

            if (gridManager != null && !tileData.walkable)
                gridManager.SetObstacleAtWorldPosition(worldPosition, true);

            if (tileData.obstacleType == WorldObstacleType.Rock)
                SpawnRock(tileData, worldPosition, objectsParent.transform, rockMat, gridManager, worldState);
            else if (tileData.obstacleType == WorldObstacleType.Tree)
                SpawnTree(tileData, worldPosition, objectsParent.transform, treeMat, trunkMat, gridManager, worldState);

            if (tileData.HasResource())
                SpawnResource(tileData, worldPosition, objectsParent.transform, junkMat, crystalMat, gridManager, worldState);
        }
    }

    static void CreateTileVisual(WorldTileData tileData, Vector3 worldPosition, Transform parent, Material grassMat, Material stoneMat, Material waterMat, Material pathMat, Material darkGrassMat)
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

    static void SpawnRock(WorldTileData tileData, Vector3 worldPosition, Transform parent, Material rockMat, GridManager gridManager, WorldState worldState)
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

        tileData.worldObject = rock;

        if (gridManager != null)
        {
            gridManager.SetObstacleAtWorldPosition(worldPosition, true);
            gridManager.SetInteractableAtWorldPosition(worldPosition, rockInteractable);
        }
    }

    static void SpawnTree(WorldTileData tileData, Vector3 worldPosition, Transform parent, Material treeMat, Material trunkMat, GridManager gridManager, WorldState worldState)
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

        GameObject foliage = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        foliage.name = "TreeFoliage";
        foliage.transform.parent = treeRoot.transform;
        foliage.transform.position = worldPosition + new Vector3(0, 2f, 0);
        foliage.transform.localScale = new Vector3(1.8f, 1.4f, 1.8f);
        Paint(foliage, treeMat);

        tileData.worldObject = treeRoot;

        if (gridManager != null)
        {
            gridManager.SetObstacleAtWorldPosition(worldPosition, true);
            gridManager.SetInteractableAtWorldPosition(worldPosition, treeInteractable);
        }
    }

    static void SpawnResource(WorldTileData tileData, Vector3 worldPosition, Transform parent, Material junkMat, Material crystalMat, GridManager gridManager, WorldState worldState)
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
        tileData.worldObject = resource;

        if (gridManager != null)
            gridManager.SetInteractableAtWorldPosition(worldPosition, interactable);
    }

    static void CreateUI()
    {
        // Inventory Canvas
        GameObject inventoryCanvasObj = new GameObject("InventoryCanvas");
        Canvas inventoryCanvas = inventoryCanvasObj.AddComponent<Canvas>();
        inventoryCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        inventoryCanvas.sortingOrder = 100;
        inventoryCanvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        inventoryCanvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // Crystal text
        GameObject crystalTextObj = new GameObject("CrystalText");
        crystalTextObj.transform.SetParent(inventoryCanvasObj.transform);
        RectTransform crystalRect = crystalTextObj.AddComponent<RectTransform>();
        crystalRect.anchorMin = new Vector2(0f, 1f);
        crystalRect.anchorMax = new Vector2(0f, 1f);
        crystalRect.pivot = new Vector2(0f, 1f);
        crystalRect.anchoredPosition = new Vector2(20, -20);
        crystalRect.sizeDelta = new Vector2(200, 40);

        Text crystalText = crystalTextObj.AddComponent<Text>();
        crystalText.text = "Crystal: 0";
        crystalText.fontSize = 24;
        crystalText.color = Color.white;

        // Scrap text
        GameObject scrapTextObj = new GameObject("ScrapText");
        scrapTextObj.transform.SetParent(inventoryCanvasObj.transform);
        RectTransform scrapRect = scrapTextObj.AddComponent<RectTransform>();
        scrapRect.anchorMin = new Vector2(0f, 1f);
        scrapRect.anchorMax = new Vector2(0f, 1f);
        scrapRect.pivot = new Vector2(0f, 1f);
        scrapRect.anchoredPosition = new Vector2(20, -60);
        scrapRect.sizeDelta = new Vector2(200, 40);

        Text scrapText = scrapTextObj.AddComponent<Text>();
        scrapText.text = "Scrap: 0";
        scrapText.fontSize = 24;
        scrapText.color = Color.white;

        // Wood text
        GameObject woodTextObj = new GameObject("WoodText");
        woodTextObj.transform.SetParent(inventoryCanvasObj.transform);
        RectTransform woodRect = woodTextObj.AddComponent<RectTransform>();
        woodRect.anchorMin = new Vector2(0f, 1f);
        woodRect.anchorMax = new Vector2(0f, 1f);
        woodRect.pivot = new Vector2(0f, 1f);
        woodRect.anchoredPosition = new Vector2(20, -100);
        woodRect.sizeDelta = new Vector2(200, 40);

        Text woodText = woodTextObj.AddComponent<Text>();
        woodText.text = "Wood: 0";
        woodText.fontSize = 24;
        woodText.color = Color.white;

        // Interaction Canvas
        GameObject canvasObj = new GameObject("InteractionCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // Interaction Prompt Text
        GameObject textObj = new GameObject("InteractionPrompt");
        textObj.transform.SetParent(canvasObj.transform);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0f);
        textRect.anchorMax = new Vector2(0.5f, 0f);
        textRect.pivot = new Vector2(0.5f, 0f);
        textRect.anchoredPosition = new Vector2(0, 40);
        textRect.sizeDelta = new Vector2(800, 80);

        UnityEngine.UI.Text promptText = textObj.AddComponent<UnityEngine.UI.Text>();
        promptText.text = "";
        promptText.fontSize = 32;
        promptText.color = Color.white;
        promptText.alignment = TextAnchor.MiddleCenter;

        UnityEngine.UI.Outline outline = textObj.AddComponent<UnityEngine.UI.Outline>();
        outline.effectColor = new Color(0, 0, 0, 0.5f);
        outline.effectDistance = new Vector2(2, -2);

        BotiUI botiUI = canvasObj.AddComponent<BotiUI>();
        botiUI.interactionText = promptText;

        GameObject buildTextObj = new GameObject("BuildModeText");
        buildTextObj.transform.SetParent(canvasObj.transform);

        RectTransform buildRect = buildTextObj.AddComponent<RectTransform>();
        buildRect.anchorMin = new Vector2(1f, 1f);
        buildRect.anchorMax = new Vector2(1f, 1f);
        buildRect.pivot = new Vector2(1f, 1f);
        buildRect.anchoredPosition = new Vector2(-20, -20);
        buildRect.sizeDelta = new Vector2(280, 80);

        UnityEngine.UI.Text buildText = buildTextObj.AddComponent<UnityEngine.UI.Text>();
        buildText.text = "Build Mode: OFF\nSelected: Wall";
        buildText.fontSize = 22;
        buildText.color = Color.white;
        buildText.alignment = TextAnchor.UpperRight;

        UnityEngine.UI.Outline buildOutline = buildTextObj.AddComponent<UnityEngine.UI.Outline>();
        buildOutline.effectColor = new Color(0, 0, 0, 0.5f);
        buildOutline.effectDistance = new Vector2(2, -2);

        botiUI.buildText = buildText;

        GameObject toolTextObj = new GameObject("EquippedToolText");
        toolTextObj.transform.SetParent(canvasObj.transform);

        RectTransform toolRect = toolTextObj.AddComponent<RectTransform>();
        toolRect.anchorMin = new Vector2(1f, 1f);
        toolRect.anchorMax = new Vector2(1f, 1f);
        toolRect.pivot = new Vector2(1f, 1f);
        toolRect.anchoredPosition = new Vector2(-20, -105);
        toolRect.sizeDelta = new Vector2(280, 40);

        UnityEngine.UI.Text toolText = toolTextObj.AddComponent<UnityEngine.UI.Text>();
        toolText.text = "Equipped Tool: Axe";
        toolText.fontSize = 22;
        toolText.color = Color.white;
        toolText.alignment = TextAnchor.UpperRight;

        UnityEngine.UI.Outline toolOutline = toolTextObj.AddComponent<UnityEngine.UI.Outline>();
        toolOutline.effectColor = new Color(0, 0, 0, 0.5f);
        toolOutline.effectDistance = new Vector2(2, -2);

        botiUI.toolText = toolText;
    }

    static void CreateBoti()
    {
        Material bodyMat = Mat(new Color(0.72f, 0.75f, 0.8f));
        Material headMat = Mat(new Color(0.6f, 0.63f, 0.68f));
        Material eyeMat = Mat(new Color(0.2f, 0.9f, 0.9f));
        Material accentMat = Mat(new Color(0.9f, 0.3f, 0.2f));

        GameObject botiRoot = new GameObject("Boti");
        botiRoot.transform.position = new Vector3(0, 0, 0);
        botiRoot.transform.rotation = Quaternion.Euler(0, 0, 0);

        // Torso — cylinder (pivot at center, half-height 0.2)
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        body.name = "Body";
        body.transform.parent = botiRoot.transform;
        body.transform.position = new Vector3(0, 0.2f, 0);
        body.transform.localScale = new Vector3(0.4f, 0.2f, 0.4f);
        Paint(body, bodyMat);

        // Head — cube (bottom sits on top of torso)
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Cube);
        head.name = "Head";
        head.transform.parent = botiRoot.transform;
        head.transform.position = new Vector3(0, 0.55f, 0);
        head.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);
        Paint(head, headMat);

        // Eyes — two small spheres
        for (int i = 0; i < 2; i++)
        {
            GameObject eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            eye.name = "Eye" + i;
            eye.transform.parent = head.transform;
            eye.transform.localPosition = new Vector3(i == 0 ? -0.1f : 0.1f, 0, 0.18f);
            eye.transform.localScale = new Vector3(0.08f, 0.08f, 0.05f);
            Paint(eye, eyeMat);
        }

        // Antenna stalk + tip
        GameObject antenna = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        antenna.name = "Antenna";
        antenna.transform.parent = head.transform;
        antenna.transform.localPosition = new Vector3(0, 0.22f, 0);
        antenna.transform.localScale = new Vector3(0.03f, 0.15f, 0.03f);
        Paint(antenna, bodyMat);

        GameObject antennaTip = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        antennaTip.name = "AntennaTip";
        antennaTip.transform.parent = antenna.transform;
        antennaTip.transform.localPosition = new Vector3(0, 0.13f, 0);
        antennaTip.transform.localScale = new Vector3(0.07f, 0.07f, 0.07f);
        Paint(antennaTip, accentMat);

        // Legs — two capsules
        for (int i = 0; i < 2; i++)
        {
            GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            leg.name = "Leg" + i;
            leg.transform.parent = botiRoot.transform;
            leg.transform.position = new Vector3(i == 0 ? -0.13f : 0.13f, 0.06f, 0);
            leg.transform.localScale = new Vector3(0.12f, 0.06f, 0.12f);
            Paint(leg, bodyMat);
        }

        // Arms — two cylinders with slight angle
        for (int i = 0; i < 2; i++)
        {
            GameObject arm = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            arm.name = "Arm" + i;
            arm.transform.parent = botiRoot.transform;
            arm.transform.position = new Vector3(i == 0 ? -0.32f : 0.32f, 0.24f, 0);
            arm.transform.localScale = new Vector3(0.08f, 0.22f, 0.08f);
            arm.transform.rotation = Quaternion.Euler(0, 0, i == 0 ? 18f : -18f);
            Paint(arm, bodyMat);
        }

        BotiPlayerController playerCtrl = botiRoot.AddComponent<BotiPlayerController>();
        BotiFeedback feedback = botiRoot.AddComponent<BotiFeedback>();
        playerCtrl.moveSpeed = 6f;
        playerCtrl.gridSize = 1f;
        playerCtrl.interactionRange = 3f;
        playerCtrl.gridManager = Object.FindObjectOfType<GridManager>();
        playerCtrl.worldState = Object.FindObjectOfType<WorldState>();
        playerCtrl.visualSpawner = Object.FindObjectOfType<WorldVisualSpawner>();
        playerCtrl.feedback = feedback;

        // Add inventory
        BotiInventory inventory = botiRoot.AddComponent<BotiInventory>();

        // Wire up inventory UI texts
        Text scrapText = GameObject.Find("ScrapText").GetComponent<Text>();
        Text crystalText = GameObject.Find("CrystalText").GetComponent<Text>();
        Text woodText = GameObject.Find("WoodText").GetComponent<Text>();
        inventory.scrapText = scrapText;
        inventory.crystalText = crystalText;
        inventory.woodText = woodText;
        inventory.UpdateUI();

        // Wire up interaction prompt
        UnityEngine.UI.Text promptText = GameObject.Find("InteractionPrompt").GetComponent<UnityEngine.UI.Text>();
        BotiUI botiUI = Object.FindObjectOfType<BotiUI>();
        playerCtrl.interactionPromptText = promptText;
        playerCtrl.botiUI = botiUI;
        playerCtrl.inventory = inventory;

        // Wire up inventory reference to all Interactables
        Interactable[] interactables = Object.FindObjectsOfType<Interactable>();
        GridManager gridManager = Object.FindObjectOfType<GridManager>();
        WorldState worldState = Object.FindObjectOfType<WorldState>();
        WorldVisualSpawner visualSpawner = Object.FindObjectOfType<WorldVisualSpawner>();
        foreach (Interactable inter in interactables)
        {
            inter.inventory = inventory;
            if (inter.gridManager == null)
                inter.gridManager = gridManager;
            if (inter.worldState == null)
                inter.worldState = worldState;
            if (inter.visualSpawner == null)
                inter.visualSpawner = visualSpawner;
            if (inter.feedback == null)
                inter.feedback = feedback;
        }

        Camera cam = Object.FindObjectOfType<Camera>();
        if (cam != null)
            ConfigureFixedCamera(cam);

        CreateSaveSystem(playerCtrl, inventory, gridManager, worldState);
    }

    static void ConfigureFixedCamera(Camera cam)
    {
        cam.transform.SetParent(null);
        cam.gameObject.name = "MainCamera";
        cam.tag = "MainCamera";
        cam.orthographic = true;
        cam.orthographicSize = 15;
        cam.transform.position = new Vector3(0, 26, -18);
        cam.transform.rotation = Quaternion.Euler(60, 0, 0);

        if (cam.GetComponent<AudioListener>() == null)
            cam.gameObject.AddComponent<AudioListener>();

        CameraFollow camFollow = cam.GetComponent<CameraFollow>();
        if (camFollow == null)
            camFollow = cam.gameObject.AddComponent<CameraFollow>();

        if (camFollow != null)
        {
            camFollow.fixedPosition = new Vector3(0, 26, -18);
            camFollow.fixedRotation = new Vector3(60, 0, 0);
            camFollow.orthographicSize = 15;
            camFollow.ApplyFixedCamera();
        }
    }

    static void CreateSaveSystem(BotiPlayerController playerCtrl, BotiInventory inventory, GridManager gridManager, WorldState worldState)
    {
        GameObject saveObj = new GameObject("SaveSystem");
        SaveSystem saveSystem = saveObj.AddComponent<SaveSystem>();
        saveSystem.player = playerCtrl;
        saveSystem.inventory = inventory;
        saveSystem.gridManager = gridManager;
        saveSystem.worldState = worldState;
        saveSystem.visualSpawner = Object.FindObjectOfType<WorldVisualSpawner>();
    }
}
