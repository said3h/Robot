using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

/// <summary>
/// Generador automático de escena Boti.
/// Menu: Boti → Build Scene
/// </summary>
public class BotiSceneBuilder
{
    [MenuItem("Boti/Build Scene", false, 1)]
    public static void BuildScene()
    {
        Debug.Log("=== Boti Scene Builder ===");

        // 1. Crear escena limpia
        Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // 2. Crear GameObjects básicos
        CreateMainCamera();
        CreateDirectionalLight();
        GameObject levelGen = CreateLevelGenerator();

        // 3. Configurar SpriteManager
        SpriteManager spriteManager = ConfigureSpriteManager();

        // 4. Generar el nivel
        LevelGenerator generator = levelGen.GetComponent<LevelGenerator>();
        if (generator != null)
        {
            generator.spriteManager = spriteManager;
            generator.GenerateSimpleLevel();
        }

        // 5. Configurar cámara para vista 2D
        SetupCamera();

        // 6. Guardar escena
        string scenePath = "Assets/Scenes/BotiGame.unity";
        EditorSceneManager.SaveScene(newScene, scenePath);

        Debug.Log("=== Scene Builder Complete ===");
        Debug.Log("Listo para Play!");

        EditorUtility.DisplayDialog(
            "Boti Scene Builder",
            "Escena creada exitosamente!\n\nPresiona Play para probar.",
            "OK"
        );
    }

    static void CreateMainCamera()
    {
        GameObject camObj = GameObject.Find("Main Camera");
        if (!camObj)
        {
            camObj = new GameObject("Main Camera");
            camObj.AddComponent<Camera>();
            camObj.tag = "MainCamera";
        }

        Camera cam = camObj.GetComponent<Camera>();
        cam.transform.position = new Vector3(0, 10, 0);
        cam.transform.rotation = Quaternion.Euler(90, 0, 0);
        cam.orthographic = true;
        cam.orthographicSize = 8;
        cam.backgroundColor = Color.black;
    }

    static void CreateDirectionalLight()
    {
        GameObject lightObj = GameObject.Find("Directional Light");
        if (!lightObj)
        {
            lightObj = new GameObject("Directional Light");
            lightObj.AddComponent<Light>();
        }

        Light light = lightObj.GetComponent<Light>();
        light.type = LightType.Directional;
        light.transform.rotation = Quaternion.Euler(50, -30, 0);
    }

    static GameObject CreateLevelGenerator()
    {
        // Crear GameObject
        GameObject levelGen = new GameObject("LevelGenerator");

        // Agregar LevelGenerator
        LevelGenerator generator = levelGen.AddComponent<LevelGenerator>();

        // Agregar SpriteManager
        SpriteManager spriteMgr = levelGen.AddComponent<SpriteManager>();

        // Configurar LevelGenerator
        generator.spriteManager = spriteMgr;
        generator.gridSize = new Vector2(7, 7);
        generator.robotStart = new Vector3(-3, 0, -3);
        generator.goalPosition = new Vector3(3, 0, 3);
        generator.obstacles = new Vector3[]
        {
            new Vector3(0, 0, 0),
            new Vector3(1, 0, 0),
            new Vector3(-1, 0, 1),
            new Vector3(2, 0, -1)
        };

        return levelGen;
    }

    static SpriteManager ConfigureSpriteManager()
    {
        SpriteManager[] managers = UnityEngine.Object.FindObjectsOfType<SpriteManager>();
        if (managers.Length > 0)
            return managers[0];

        GameObject spriteMgrObj = new GameObject("SpriteManager");
        return spriteMgrObj.AddComponent<SpriteManager>();
    }

    static void SetupCamera()
    {
        Camera cam = Camera.main;
        if (cam)
        {
            cam.transform.position = new Vector3(0, 12, 0);
            cam.transform.rotation = Quaternion.Euler(90, 0, 0);
            cam.orthographic = true;
            cam.orthographicSize = 6;
        }
    }

    [MenuItem("Boti/Test MCP Menu")]
    public static void TestMenu()
    {
        Debug.Log("Boti menu works");
    }

    [MenuItem("Boti/Clear Scene", false, 2)]
    public static void ClearScene()
    {
        if (!EditorUtility.DisplayDialog(
            "Clear Scene",
            "¿Limpiar la escena actual?",
            "Si", "Cancelar"))
            return;

        string[] objectNames = { "Grid", "Robot", "Goal", "LevelGenerator", "SpriteManager" };

        foreach (string name in objectNames)
        {
            GameObject obj = GameObject.Find(name);
            if (obj)
            {
                if (Application.isPlaying)
                    UnityEngine.Object.Destroy(obj);
                else
                    UnityEngine.Object.DestroyImmediate(obj);
            }
        }

        // Limpiar hijos del Grid
        GameObject grid = GameObject.Find("Grid");
        if (grid)
        {
            if (Application.isPlaying)
                UnityEngine.Object.Destroy(grid);
            else
                UnityEngine.Object.DestroyImmediate(grid);
        }

        Debug.Log("Escena limpiada");
    }
}