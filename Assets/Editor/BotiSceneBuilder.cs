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
        GameObject camObj = new GameObject("MainCamera");
        Camera cam = camObj.AddComponent<Camera>();
        camObj.tag = "MainCamera";
        cam.transform.position = new Vector3(0, 20, -5);
        cam.transform.rotation = Quaternion.Euler(60, 0, 0);
        cam.orthographic = true;
        cam.orthographicSize = 12;
        cam.backgroundColor = new Color(0.4f, 0.6f, 0.8f);

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
        Material grassMat = Mat(new Color(0.3f, 0.6f, 0.25f));
        Material stoneMat = Mat(new Color(0.5f, 0.5f, 0.55f));
        Material waterMat = Mat(new Color(0.2f, 0.4f, 0.7f));
        Material pathMat = Mat(new Color(0.65f, 0.5f, 0.35f));
        Material darkGrassMat = Mat(new Color(0.25f, 0.5f, 0.2f));

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

        CreateDecorations();
    }

    static void CreateDecorations()
    {
        Material rockMat = Mat(new Color(0.6f, 0.6f, 0.65f));
        Material treeMat = Mat(new Color(0.3f, 0.5f, 0.2f));
        Material trunkMat = Mat(new Color(0.4f, 0.25f, 0.15f));
        Material junkMat = Mat(new Color(0.5f, 0.5f, 0.6f));
        Material crystalMat = Mat(new Color(0.6f, 0.2f, 0.8f));

        System.Random rng = new System.Random(42);

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

        for (int i = 0; i < 15; i++)
        {
            int x = rng.Next(-8, 9);
            int z = rng.Next(-8, 9);

            if (Mathf.Abs(x) < 2 && Mathf.Abs(z) < 2) continue;

            float height = (float)(rng.NextDouble() * 1.5 + 1.5);

            GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "TreeTrunk";
            trunk.transform.position = new Vector3(x, height * 0.4f, z);
            trunk.transform.localScale = new Vector3(0.15f, height * 0.4f, 0.15f);
            Paint(trunk, trunkMat);

            Interactable treeInteractable = trunk.AddComponent<Interactable>();
            treeInteractable.type = InteractableType.Tree;

            GameObject foliage = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            foliage.name = "TreeFoliage";
            foliage.transform.position = new Vector3(x, height * 0.8f + 0.5f, z);
            foliage.transform.localScale = new Vector3(height * 0.8f, height * 0.6f, height * 0.8f);
            Paint(foliage, treeMat);

            Interactable foliageInteractable = foliage.AddComponent<Interactable>();
            foliageInteractable.type = InteractableType.Tree;
        }

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

            Interactable scrapInteractable = junk.AddComponent<Interactable>();
            scrapInteractable.type = InteractableType.Scrap;
        }

        for (int i = 0; i < 8; i++)
        {
            int x = rng.Next(-8, 9);
            int z = rng.Next(-8, 9);

            GameObject crystal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            crystal.name = "Crystal";
            crystal.transform.position = new Vector3(x, 0.4f, z);
            crystal.transform.localScale = new Vector3(0.15f, 0.8f, 0.15f);
            Paint(crystal, crystalMat);

            Interactable crystalInteractable = crystal.AddComponent<Interactable>();
            crystalInteractable.type = InteractableType.Crystal;
        }
    }

    static void CreateUI()
    {
        GameObject canvasObj = new GameObject("InteractionCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

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
    }

    static void CreateBoti()
    {
        GameObject boti = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        boti.name = "Boti";
        boti.transform.position = new Vector3(0, 0.6f, 0);

        Material botiMat = Mat(new Color(0.2f, 0.9f, 0.9f));
        Paint(boti, botiMat);

        BotiPlayerController playerCtrl = boti.AddComponent<BotiPlayerController>();
        playerCtrl.moveSpeed = 6f;
        playerCtrl.gridSize = 1f;
        playerCtrl.interactionRange = 3f;

        UnityEngine.UI.Text promptText = Object.FindObjectOfType<UnityEngine.UI.Text>();
        if (promptText != null)
        {
            playerCtrl.interactionPromptText = promptText;
        }

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
    }
}