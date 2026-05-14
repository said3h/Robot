using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Crea el nivel completo con sprites.
/// </summary>
public class LevelGenerator : MonoBehaviour
{
    [Header("Configuración del Grid")]
    public Vector2 gridSize = new Vector2(7, 7);
    public float tileSpacing = 1f;

    [Header("Posiciones")]
    public Vector3 robotStart = new Vector3(-3, 0, -3);
    public Vector3 goalPosition = new Vector3(3, 0, 3);
    public Vector3[] obstacles;

    [Header("Referencias")]
    public SpriteManager spriteManager;

    private GameObject gridParent;
    private List<GameObject> levelObjects = new List<GameObject>();

    void Start()
    {
        if (spriteManager == null)
            spriteManager = SpriteManager.Instance;

        GenerateLevel();
    }

    public void GenerateLevel()
    {
        ClearLevel();

        // Crear grid de tiles
        CreateGrid();

        // Crear robot
        CreateRobot();

        // Crear meta
        CreateGoal();

        // Crear obstáculos
        CreateObstacles();
    }

    void CreateGrid()
    {
        gridParent = new GameObject("Grid");
        levelObjects.Add(gridParent);

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                Vector3 pos = new Vector3(
                    x - gridSize.x / 2,
                    0,
                    y - gridSize.y / 2
                );

                GameObject tile = spriteManager.CreateTile(pos, gridParent.transform);
                levelObjects.Add(tile);
            }
        }
    }

    void CreateRobot()
    {
        GameObject robot = spriteManager.CreateRobot(robotStart);
        levelObjects.Add(robot);
    }

    void CreateGoal()
    {
        GameObject goal = spriteManager.CreateGoal(goalPosition);
        levelObjects.Add(goal);
    }

    void CreateObstacles()
    {
        if (obstacles == null || obstacles.Length == 0) return;

        foreach (Vector3 pos in obstacles)
        {
            Vector3 worldPos = new Vector3(
                robotStart.x + pos.x,
                0,
                robotStart.z + pos.z
            );

            bool isRock = (Random.value > 0.5f);
            GameObject obstacle = spriteManager.CreateObstacle(worldPos, isRock, gridParent.transform);
            levelObjects.Add(obstacle);
        }
    }

    public void ClearLevel()
    {
        foreach (GameObject obj in levelObjects)
        {
            if (obj) Destroy(obj);
        }
        levelObjects.Clear();

        if (gridParent) Destroy(gridParent);
    }

    /// <summary>
    /// Genera un nivel simple predeterminado.
    /// </summary>
    public void GenerateSimpleLevel()
    {
        robotStart = new Vector3(-3, 0, -3);
        goalPosition = new Vector3(3, 0, 3);
        obstacles = new Vector3[]
        {
            new Vector3(0, 0, 0),
            new Vector3(1, 0, 0),
            new Vector3(-1, 0, 1),
            new Vector3(2, 0, -1)
        };

        GenerateLevel();
    }
}