using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Administrador de sprites para el juego Boti.
/// Asigna sprites automáticamente a los GameObjects.
/// </summary>
public class SpriteManager : MonoBehaviour
{
    [Header("Sprites del Robot")]
    public Sprite robotUp;
    public Sprite robotDown;
    public Sprite robotLeft;
    public Sprite robotRight;

    [Header("Sprites del Nivel")]
    public Sprite tileNormal;
    public Sprite battery;
    public Sprite rock;
    public Sprite tree;

    [Header("Configuración")]
    public float spriteScale = 1f;

    private static SpriteManager instance;
    private Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();

    void Awake()
    {
        instance = this;
        LoadSpritesFromResources();
    }

    public static SpriteManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("SpriteManager");
                instance = go.AddComponent<SpriteManager>();
            }
            return instance;
        }
    }

    /// <summary>
    /// Carga sprites desde la carpeta Sprites (Assets/Sprites).
    /// </summary>
    void LoadSpritesFromResources()
    {
        // Intentar cargar desde Resources/Sprites
        Sprite[] allSprites = Resources.LoadAll<Sprite>("Sprites");

        // Si no hay sprites en Resources, cargar directamente desde Assets/Sprites
        if (allSprites == null || allSprites.Length == 0)
        {
            LoadSpritesFromAssets();
            return;
        }

        foreach (Sprite sprite in allSprites)
        {
            spriteCache[sprite.name] = sprite;
        }

        AssignSprites();
    }

    /// <summary>
    /// Carga sprites directamente desde Assets/Sprites usando AssetDatabase.
    /// </summary>
    void LoadSpritesFromAssets()
    {
#if UNITY_EDITOR
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:Sprite", new[] { "Assets/Sprites" });

        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            Sprite sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);

            if (sprite != null)
            {
                spriteCache[sprite.name] = sprite;
            }
        }
#endif

        AssignSprites();
    }

    void AssignSprites()
    {
        // Asignar sprites del robot
        robotUp = GetSpriteOrFallback("robot_up", null);
        robotDown = GetSpriteOrFallback("robot_down", null);
        robotLeft = GetSpriteOrFallback("robot_left", null);
        robotRight = GetSpriteOrFallback("robot_right", null);

        // Asignar sprites del nivel
        tileNormal = GetSpriteOrFallback("tile_normal", null);
        battery = GetSpriteOrFallback("battery", null);
        rock = GetSpriteOrFallback("rock", null);
        tree = GetSpriteOrFallback("tree", null);

        Debug.Log($"SpriteManager: {spriteCache.Count} sprites cargados");
    }

    Sprite GetSpriteOrFallback(string name, Sprite fallback)
    {
        if (spriteCache.ContainsKey(name))
            return spriteCache[name];

        Debug.LogWarning($"SpriteManager: Sprite '{name}' no encontrado, usando fallback");
        return fallback;
    }

    /// <summary>
    /// Obtiene el sprite del robot según la dirección.
    /// </summary>
    public Sprite GetRobotSprite(string direction)
    {
        return direction.ToLower() switch
        {
            "up" => robotUp,
            "down" => robotDown,
            "left" => robotLeft,
            "right" => robotRight,
            _ => robotRight
        };
    }

    /// <summary>
    /// Aplica sprite a un GameObject con SpriteRenderer.
    /// </summary>
    public void ApplySprite(GameObject target, Sprite sprite, float scale = 1f)
    {
        if (!target) return;

        SpriteRenderer sr = target.GetComponent<SpriteRenderer>();
        if (!sr)
            sr = target.AddComponent<SpriteRenderer>();

        sr.sprite = sprite;
        target.transform.localScale = Vector3.one * scale;
    }

    /// <summary>
    /// Crea un tile con sprite.
    /// </summary>
    public GameObject CreateTile(Vector3 position, Transform parent = null)
    {
        GameObject tile = new GameObject("Tile");
        tile.transform.position = position;
        if (parent) tile.transform.parent = parent;

        ApplySprite(tile, tileNormal, spriteScale);

        return tile;
    }

    /// <summary>
    /// Crea la meta (battery) con sprite.
    /// </summary>
    public GameObject CreateGoal(Vector3 position, Transform parent = null)
    {
        GameObject goal = new GameObject("Goal");
        goal.transform.position = position;
        if (parent) goal.transform.parent = parent;

        ApplySprite(goal, battery, spriteScale);

        return goal;
    }

    /// <summary>
    /// Crea un obstáculo con sprite.
    /// </summary>
    public GameObject CreateObstacle(Vector3 position, bool isRock = true, Transform parent = null)
    {
        GameObject obstacle = new GameObject(isRock ? "Rock" : "Tree");
        obstacle.transform.position = position;
        if (parent) obstacle.transform.parent = parent;

        Sprite sprite = isRock ? rock : tree;
        ApplySprite(obstacle, sprite, spriteScale);

        BoxCollider2D col = obstacle.AddComponent<BoxCollider2D>();

        return obstacle;
    }

    /// <summary>
    /// Crea el robot con sprites direccionales.
    /// </summary>
    public GameObject CreateRobot(Vector3 position, Transform parent = null)
    {
        GameObject robot = new GameObject("Robot");
        robot.transform.position = position;
        if (parent) robot.transform.parent = parent;

        ApplySprite(robot, robotRight, spriteScale);

        // Agregar componente de dirección
        RobotDirection rd = robot.AddComponent<RobotDirection>();
        rd.Initialize(this);

        return robot;
    }

    /// <summary>
    /// Obtiene sprite placeholder.
    /// </summary>
    public Sprite GetPlaceholder()
    {
        // Crear textura simple de color
        Texture2D tex = new Texture2D(64, 64);
        Color[] colors = new Color[64 * 64];
        for (int i = 0; i < colors.Length; i++)
            colors[i] = Color.gray;
        tex.SetPixels(colors);
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 64);
    }
}