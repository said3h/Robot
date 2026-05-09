using UnityEngine;
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
        robot.transform.rotation = Quaternion.Euler(0, 90f, 0); // Start facing East
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
}