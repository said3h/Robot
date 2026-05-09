using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public static class BotiSceneBuilder
{
    [MenuItem("Boti/Build Scene")]
    public static void BuildScene()
    {
        Debug.Log("BOTI BUILDER EJECUTADO");

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        CreateCamera();
        CreateLight();
        CreateTestGrid();
        CreateControlPanel();

        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            AssetDatabase.CreateFolder("Assets", "Scenes");

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/BotiGame.unity");

        EditorUtility.DisplayDialog("Boti", "Escena creada correctamente.", "OK");
    }

    static void CreateCamera()
    {
        GameObject camObj = new GameObject("Main Camera");
        Camera cam = camObj.AddComponent<Camera>();
        camObj.tag = "MainCamera";

        cam.transform.position = new Vector3(0, 12, 0);
        cam.transform.rotation = Quaternion.Euler(90, 0, 0);
        cam.orthographic = true;
        cam.orthographicSize = 8;
        cam.backgroundColor = new Color(0.12f, 0.12f, 0.12f);
    }

    static void CreateLight()
    {
        GameObject lightObj = new GameObject("Directional Light");
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.2f;
        lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);
    }

    static Material Mat(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");

        if (shader == null)
            shader = Shader.Find("Universal Render Pipeline/Unlit");

        if (shader == null)
            shader = Shader.Find("Sprites/Default");

        Material mat = new Material(shader);
        mat.color = color;
        return mat;
    }

    static void Paint(GameObject obj, Material mat)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
            renderer.sharedMaterial = mat;
    }

    static void CreateTestGrid()
    {
        Material tileMat = Mat(new Color(0.45f, 0.55f, 0.65f));
        Material robotMat = Mat(Color.cyan);
        Material goalMat = Mat(Color.green);
        Material obstacleMat = Mat(Color.red);

        GameObject gridParent = new GameObject("Grid");

        for (int x = -3; x <= 3; x++)
        {
            for (int z = -3; z <= 3; z++)
            {
                GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tile.name = "Tile";
                tile.transform.parent = gridParent.transform;
                tile.transform.position = new Vector3(x, 0, z);
                tile.transform.localScale = new Vector3(0.9f, 0.1f, 0.9f);
                Paint(tile, tileMat);
            }
        }

        GameObject robot = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        robot.name = "Robot";
        robot.transform.position = new Vector3(-3, 0.8f, -3);
        Paint(robot, robotMat);

        // Add BotiRobotController for grid-based movement
        robot.AddComponent<BotiRobotController>();

        GameObject goal = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        goal.name = "Goal";
        goal.transform.position = new Vector3(3, 0.6f, 3);
        Paint(goal, goalMat);

        Vector3[] obstacles =
        {
            new Vector3(0, 0.6f, 0),
            new Vector3(1, 0.6f, 0),
            new Vector3(-1, 0.6f, 1),
            new Vector3(2, 0.6f, -1)
        };

        foreach (Vector3 pos in obstacles)
        {
            GameObject obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obstacle.name = "Obstacle";
            obstacle.transform.position = pos;
            obstacle.transform.localScale = new Vector3(0.8f, 1f, 0.8f);
            Paint(obstacle, obstacleMat);
        }

        // Create CommandQueue object
        GameObject commandQueueObj = new GameObject("CommandQueue");
        BotiCommandQueue cmdQueue = commandQueueObj.AddComponent<BotiCommandQueue>();
        cmdQueue.robotController = robot.GetComponent<BotiRobotController>();
    }

    static void CreateControlPanel()
    {
        // Create Canvas for UI
        GameObject canvasObj = new GameObject("ControlPanel");
        canvasObj.AddComponent<Canvas>();
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        // Make Canvas render in Screen Space Overlay
        Canvas canvas = canvasObj.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.pixelPerfect = true;

        // Find CommandQueue
        BotiCommandQueue cmdQueue = GameObject.Find("CommandQueue")?.GetComponent<BotiCommandQueue>();

        if (cmdQueue == null)
        {
            Debug.LogWarning("CommandQueue not found - UI buttons will not be linked");
            return;
        }

        // Create button style
        GameObject buttonPrefab = CreateButtonPrefab();

        // === Bottom Panel Background ===
        GameObject bottomPanel = new GameObject("BottomPanel");
        bottomPanel.transform.parent = canvasObj.transform;
        Image panelBg = bottomPanel.AddComponent<Image>();
        panelBg.color = new Color(0.08f, 0.08f, 0.12f, 0.98f);
        RectTransform panelRect = bottomPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 0);
        panelRect.anchorMax = new Vector2(1, 0);
        panelRect.sizeDelta = new Vector2(0, 260);
        panelRect.anchoredPosition = new Vector2(0, 130);

        // === Command Queue Display ===
        GameObject queueTextObj = new GameObject("QueueDisplay");
        queueTextObj.transform.parent = bottomPanel.transform;
        Text queueText = queueTextObj.AddComponent<Text>();
        queueText.text = "Commands: (empty)";
        queueText.fontSize = 22;
        queueText.color = new Color(0.7f, 0.9f, 0.7f);
        queueText.alignment = TextAnchor.MiddleCenter;
        queueText.fontStyle = FontStyle.Bold;
        RectTransform queueRect = queueTextObj.GetComponent<RectTransform>();
        queueRect.anchorMin = new Vector2(0.1f, 0.75f);
        queueRect.anchorMax = new Vector2(0.9f, 1f);
        queueRect.sizeDelta = new Vector2(0, 40);
        queueRect.anchoredPosition = new Vector2(0, 0);

        // Subscribe to command changes to update display
        cmdQueue.OnCommandsChanged += () =>
        {
            string display = cmdQueue.GetCommandsDisplay();
            queueText.text = "Commands: " + (display == "(empty)" ? "(empty)" : display);
        };

        // === Direction Buttons - D-Pad Layout ===
        float dpadSize = 70;

        // Up button (center top)
        CreateButton(bottomPanel.transform, buttonPrefab, "Up", 0.25f, 0.45f, dpadSize, dpadSize,
            () => cmdQueue.AddMoveUp());

        // Left button (left middle)
        CreateButton(bottomPanel.transform, buttonPrefab, "Left", 0.17f, 0.2f, dpadSize, dpadSize,
            () => cmdQueue.AddMoveLeft());

        // Down button (center bottom)
        CreateButton(bottomPanel.transform, buttonPrefab, "Down", 0.25f, 0.2f, dpadSize, dpadSize,
            () => cmdQueue.AddMoveDown());

        // Right button (right middle)
        CreateButton(bottomPanel.transform, buttonPrefab, "Right", 0.33f, 0.2f, dpadSize, dpadSize,
            () => cmdQueue.AddMoveRight());

        // === Action Buttons (right side) ===
        CreateButton(bottomPanel.transform, buttonPrefab, "Run", 0.68f, 0.45f, 90, 50,
            () => cmdQueue.RunCommands());

        CreateButton(bottomPanel.transform, buttonPrefab, "Clear", 0.83f, 0.45f, 90, 50,
            () => cmdQueue.ClearCommands());

        CreateButton(bottomPanel.transform, buttonPrefab, "Reset", 0.68f, 0.2f, 90, 50,
            () => cmdQueue.ResetLevel());
    }

    static GameObject CreateButtonPrefab()
    {
        // Create a simple button using Unity UI
        GameObject buttonObj = new GameObject("Button");
        buttonObj.SetActive(false);

        // Add Text for label
        GameObject textObj = new GameObject("Text");
        textObj.transform.parent = buttonObj.transform;
        textObj.AddComponent<Text>().text = "Button";
        textObj.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
        textObj.GetComponent<Text>().color = Color.white;
        textObj.GetComponent<Text>().fontSize = 22;
        textObj.GetComponent<Text>().fontStyle = FontStyle.Bold;

        // Set Text to fill parent
        textObj.GetComponent<RectTransform>().anchorMin = Vector2.zero;
        textObj.GetComponent<RectTransform>().anchorMax = Vector2.one;
        textObj.GetComponent<RectTransform>().sizeDelta = Vector2.zero;

        // Add Image for background
        Image img = buttonObj.AddComponent<Image>();
        img.color = new Color(0.25f, 0.35f, 0.45f, 0.95f);

        // Add Button component
        Button btn = buttonObj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.highlightedColor = new Color(0.35f, 0.5f, 0.65f);
        colors.pressedColor = new Color(0.15f, 0.25f, 0.35f);
        btn.colors = colors;

        return buttonObj;
    }

    static void CreateButton(Transform parent, GameObject prefab, string label, Vector2 anchorMin, Vector2 anchorMax, float width, float height, UnityEngine.Events.UnityAction callback)
    {
        GameObject button = Object.Instantiate(prefab);
        button.SetActive(true);
        button.name = label + "Button";
        button.transform.parent = parent;

        // Set label text
        button.transform.GetChild(0).GetComponent<Text>().text = label;

        // Set position and size using anchor-based positioning
        RectTransform rect = button.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(width, height);
        rect.anchoredPosition = Vector2.zero;

        // Add click listener
        button.GetComponent<Button>().onClick.AddListener(callback);
    }

    // New overload: x, y as separate floats for anchorMin/anchorMax
    static void CreateButton(Transform parent, GameObject prefab, string label, float anchorX, float anchorY, float width, float height, UnityEngine.Events.UnityAction callback)
    {
        CreateButton(parent, prefab, label, new Vector2(anchorX, anchorY), new Vector2(anchorX, anchorY), width, height, callback);
    }

    // Keep old signature for compatibility
    static void CreateButton(Transform parent, GameObject prefab, string label, Vector2 anchoredPosition, UnityEngine.Events.UnityAction callback)
    {
        CreateButton(parent, prefab, label, anchoredPosition, anchoredPosition, 70, 50, callback);
    }
}