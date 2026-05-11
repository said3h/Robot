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
        CreateBoti();
        CreateEventSystem();

        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            AssetDatabase.CreateFolder("Assets", "Scenes");

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/BotiGame.unity");

        EditorUtility.DisplayDialog("Boti", "Exploration sandbox created!", "OK");
    }

    static void CreateEventSystem()
    {
        // Remove any existing EventSystems first
        EventSystem[] existingEventSystems = Object.FindObjectsOfType<EventSystem>();
        foreach (EventSystem es in existingEventSystems)
        {
            Object.DestroyImmediate(es.gameObject);
        }
        
        GameObject eventSystemObj = new GameObject("EventSystem");
        eventSystemObj.AddComponent<EventSystem>();
        eventSystemObj.AddComponent<StandaloneInputModule>();
    }

    static void CreateCamera()
    {
        // Main camera with top-down view
        GameObject camObj = new GameObject("MainCamera");
        Camera cam = camObj.AddComponent<Camera>();
        camObj.tag = "MainCamera";
        cam.transform.position = new Vector3(0, 20, -5);
        cam.transform.rotation = Quaternion.Euler(60, 0, 0);
        cam.orthographic = true;
        cam.orthographicSize = 12;
        cam.backgroundColor = new Color(0.4f, 0.6f, 0.8f); // Sky blue
        
        // Add camera follow script
        CameraFollow camFollow = camObj.AddComponent<CameraFollow>();
        camFollow.offset = new Vector3(0, 20, -5);
        camFollow.smoothSpeed = 3f;
    }

    static void CreateLight()
    {
        GameObject lightObj = new GameObject("Directional Light");
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.0f;
        lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);
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
        // Materials for different terrain types
        Material grassMat = Mat(new Color(0.3f, 0.6f, 0.25f));      // Green grass
        Material stoneMat = Mat(new Color(0.5f, 0.5f, 0.55f));    // Gray stone
        Material waterMat = Mat(new Color(0.2f, 0.4f, 0.7f));     // Blue water
        Material pathMat = Mat(new Color(0.65f, 0.5f, 0.35f));   // Dirt path
        Material darkGrassMat = Mat(new Color(0.25f, 0.5f, 0.2f)); // Dark grass
        
        // Create larger world grid (20x20 tiles)
        int worldSize = 10;
        GameObject terrainParent = new GameObject("Terrain");
        
        for (int x = -worldSize; x <= worldSize; x++)
        {
            for (int z = -worldSize; z <= worldSize; z++)
            {
                GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tile.name = "Tile";
                tile.transform.parent = terrainParent.transform;
                tile.transform.position = new Vector3(x, 0, z);
                tile.transform.localScale = new Vector3(0.95f, 0.15f, 0.95f);
                
                // Add variety to terrain
                float noise = Mathf.PerlinNoise(x * 0.2f, z * 0.2f);
                
                if (noise < 0.2f)
                {
                    Paint(tile, waterMat);
                    tile.transform.localScale = new Vector3(0.95f, 0.3f, 0.95f);
                }
                else if (noise < 0.3f)
                {
                    Paint(tile, pathMat);
                }
                else if (noise < 0.5f)
                {
                    Paint(tile, stoneMat);
                }
                else if (noise < 0.7f)
                {
                    Paint(tile, darkGrassMat);
                }
                else
                {
                    Paint(tile, grassMat);
                }
            }
        }
        
        // Add decorative objects
        CreateDecorations();
    }

    static void CreateDecorations()
    {
        // Materials for decorations
        Material rockMat = Mat(new Color(0.6f, 0.6f, 0.65f));
        Material treeMat = Mat(new Color(0.3f, 0.5f, 0.2f));
        Material trunkMat = Mat(new Color(0.4f, 0.25f, 0.15f));
        Material junkMat = Mat(new Color(0.5f, 0.5f, 0.6f));
        Material crystalMat = Mat(new Color(0.6f, 0.2f, 0.8f)); // Purple crystal
        
        System.Random rng = new System.Random(42); // Deterministic placement
        
        // Scatter rocks
        for (int i = 0; i < 25; i++)
        {
            int x = rng.Next(-8, 9);
            int z = rng.Next(-8, 9);
            float scale = (float)(rng.NextDouble() * 0.5 + 0.5);
            
            GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rock.name = "Rock";
            rock.transform.position = new Vector3(x, scale * 0.4f, z);
            rock.transform.localScale = new Vector3(scale, scale * 0.6f, scale);
            Paint(rock, rockMat);
        }
        
        // Scatter trees
        for (int i = 0; i < 15; i++)
        {
            int x = rng.Next(-8, 9);
            int z = rng.Next(-8, 9);
            
            // Check if position is near center (spawn area)
            if (Mathf.Abs(x) < 2 && Mathf.Abs(z) < 2) continue;
            
            float height = (float)(rng.NextDouble() * 1.5 + 1.5);
            
            // Trunk
            GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "TreeTrunk";
            trunk.transform.position = new Vector3(x, height * 0.4f, z);
            trunk.transform.localScale = new Vector3(0.15f, height * 0.4f, 0.15f);
            Paint(trunk, trunkMat);
            
            // Foliage (sphere)
            GameObject foliage = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            foliage.name = "TreeFoliage";
            foliage.transform.position = new Vector3(x, height * 0.8f + 0.5f, z);
            foliage.transform.localScale = new Vector3(height * 0.8f, height * 0.6f, height * 0.8f);
            Paint(foliage, treeMat);
        }
        
        // Scatter robot junk/scrap
        for (int i = 0; i < 10; i++)
        {
            int x = rng.Next(-9, 10);
            int z = rng.Next(-9, 10);
            
            GameObject junk = GameObject.CreatePrimitive(PrimitiveType.Cube);
            junk.name = "RobotJunk";
            junk.transform.position = new Vector3(x, 0.2f, z);
            junk.transform.rotation = Quaternion.Euler(0, rng.Next(0, 360), 0);
            junk.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            Paint(junk, junkMat);
        }
        
        // Scatter crystals
        for (int i = 0; i < 8; i++)
        {
            int x = rng.Next(-8, 9);
            int z = rng.Next(-8, 9);
            
            GameObject crystal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            crystal.name = "Crystal";
            crystal.transform.position = new Vector3(x, 0.4f, z);
            crystal.transform.localScale = new Vector3(0.15f, 0.8f, 0.15f);
            Paint(crystal, crystalMat);
        }
        
        Debug.Log("=== WORLD DECORATIONS CREATED ===");
    }

    static void CreateBoti()
    {
        // Create Boti robot player
        GameObject boti = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        boti.name = "Boti";
        boti.transform.position = new Vector3(0, 0.6f, 0);
        
        // Material - bright cyan for visibility
        Material botiMat = Mat(new Color(0.2f, 0.9f, 0.9f));
        Paint(boti, botiMat);
        
        // Add player controller
        BotiPlayerController playerCtrl = boti.AddComponent<BotiPlayerController>();
        playerCtrl.moveSpeed = 6f;
        playerCtrl.gridSize = 1f;
        
        // Find camera and assign target
        Camera cam = Object.FindObjectOfType<Camera>();
        if (cam != null)
        {
            CameraFollow camFollow = cam.GetComponent<CameraFollow>();
            if (camFollow != null)
            {
                camFollow.SetTarget(boti.transform);
                camFollow.SnapToTarget();
            }
        }
        
        Debug.Log("=== BOTI ROBOT CREATED ===");
        Debug.Log("Use WASD or Arrow keys to move!");
    }
}
