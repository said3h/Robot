using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

/// <summary>
/// Crea una escena 2D simple para Boti.
/// Menu: Click derecho en Hierarchy → Create Boti 2D Scene
/// </summary>
public class CreateBoti2DScene : MonoBehaviour
{
    [MenuItem("GameObject/Create Boti 2D Scene", false, 10)]
    public static void Create2DScene()
    {
        ClearScene();
        CreateGrid();
        CreateBoti();
        CreateGoal();
        CreateObstacle();
        SetupCamera();
        SaveScene();
    }

    static void ClearScene()
    {
        string[] names = { "Grid", "Boti", "Goal", "Obstacle" };
        foreach (string name in names)
        {
            GameObject obj = GameObject.Find(name);
            if (obj) Object.DestroyImmediate(obj);
        }
    }

    static void CreateGrid()
    {
        GameObject gridParent = new GameObject("Grid");

        for (int x = -2; x <= 2; x++)
        {
            for (int y = -2; y <= 2; y++)
            {
                GameObject tile = new GameObject("Tile_" + x + "_" + y);
                tile.transform.parent = gridParent.transform;
                tile.transform.position = new Vector3(x, y, 0);

                SpriteRenderer sr = tile.AddComponent<SpriteRenderer>();
                sr.sprite = CreateWhiteSquare();
                sr.color = new Color(0.4f, 0.4f, 0.4f);
                sr.sortingOrder = 0;
            }
        }
    }

    static void CreateBoti()
    {
        GameObject boti = new GameObject("Boti");
        boti.transform.position = new Vector3(-1, -1, 0);

        SpriteRenderer sr = boti.AddComponent<SpriteRenderer>();
        sr.sprite = CreateWhiteSquare();
        sr.color = new Color(0.2f, 0.4f, 1f);
        sr.sortingOrder = 1;

        boti.AddComponent<Boti2DGrid>();
    }

    static void CreateGoal()
    {
        GameObject goal = new GameObject("Goal");
        goal.transform.position = new Vector3(2, 2, 0);

        SpriteRenderer sr = goal.AddComponent<SpriteRenderer>();
        sr.sprite = CreateWhiteSquare();
        sr.color = new Color(0.2f, 1f, 0.3f);
        sr.sortingOrder = 1;
    }

    static void CreateObstacle()
    {
        GameObject obstacle = new GameObject("Obstacle");
        obstacle.transform.position = new Vector3(0, 0, 0);

        SpriteRenderer sr = obstacle.AddComponent<SpriteRenderer>();
        sr.sprite = CreateWhiteSquare();
        sr.color = new Color(1f, 0.2f, 0.2f);
        sr.sortingOrder = 1;

        BoxCollider2D col = obstacle.AddComponent<BoxCollider2D>();
        obstacle.tag = "Obstacle";
    }

    static void SetupCamera()
    {
        Camera cam = Camera.main;
        if (!cam) return;

        cam.transform.position = new Vector3(0, 0, -10);
        cam.orthographic = true;
        cam.orthographicSize = 5f;
    }

    static void SaveScene()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            AssetDatabase.CreateFolder("Assets", "Scenes");

        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/Scenes/Boti2D.unity");
    }

    static Sprite CreateWhiteSquare()
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
    }
}
