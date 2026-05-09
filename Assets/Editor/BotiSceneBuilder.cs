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
        BotiRobotController rc = robot.AddComponent<BotiRobotController>();

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

        // Find robot to get reference for buttons
        BotiRobotController robotController = GameObject.Find("Robot")?.GetComponent<BotiRobotController>();

        if (robotController == null)
        {
            Debug.LogWarning("Robot not found - UI buttons will not be linked");
            return;
        }

        // Create button style
        GameObject buttonPrefab = CreateButtonPrefab();

        // Create Up button
        CreateButton(canvasObj.transform, buttonPrefab, "Up", new Vector2(0, -80),
            () => robotController.MoveNorth());

        // Create Down button
        CreateButton(canvasObj.transform, buttonPrefab, "Down", new Vector2(0, 80),
            () => robotController.MoveSouth());

        // Create Left button
        CreateButton(canvasObj.transform, buttonPrefab, "Left", new Vector2(-100, 0),
            () => robotController.MoveWest());

        // Create Right button
        CreateButton(canvasObj.transform, buttonPrefab, "Right", new Vector2(100, 0),
            () => robotController.MoveEast());
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
        textObj.GetComponent<Text>().fontSize = 24;
        textObj.GetComponent<Text>().fontStyle = FontStyle.Bold;

        // Set Text to fill parent
        textObj.GetComponent<RectTransform>().anchorMin = Vector2.zero;
        textObj.GetComponent<RectTransform>().anchorMax = Vector2.one;
        textObj.GetComponent<RectTransform>().sizeDelta = Vector2.zero;

        // Add Image for background
        Image img = buttonObj.AddComponent<Image>();
        img.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);

        // Add Button component
        Button btn = buttonObj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.highlightedColor = new Color(0.3f, 0.3f, 0.3f);
        colors.pressedColor = new Color(0.5f, 0.5f, 0.5f);
        btn.colors = colors;

        return buttonObj;
    }

    static void CreateButton(Transform parent, GameObject prefab, string label, Vector2 position, UnityEngine.Events.UnityAction callback)
    {
        GameObject button = Object.Instantiate(prefab);
        button.SetActive(true);
        button.name = label + "Button";
        button.transform.parent = parent;

        // Set label text
        button.transform.GetChild(0).GetComponent<Text>().text = label;

        // Set position and size
        RectTransform rect = button.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(100, 60);
        rect.anchoredPosition = position;

        // Add click listener
        button.GetComponent<Button>().onClick.AddListener(callback);
    }
}