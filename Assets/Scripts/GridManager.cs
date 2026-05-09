using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Maneja el grid y detecta obstaculos.
/// </summary>
public class GridManager : MonoBehaviour
{
    [Header("Configuración")]
    public Vector2 gridSize = new Vector2(7, 7);
    public float tileSize = 1f;

    [Header("Prefabs")]
    public GameObject tilePrefab;
    public GameObject obstaclePrefab;
    public GameObject goalPrefab;
    public GameObject robotPrefab;

    [Header("Materiales")]
    public Material tileMaterial;
    public Material obstacleMaterial;
    public Material goalMaterial;

    private Dictionary<Vector3, GameObject> obstacles = new Dictionary<Vector3, GameObject>();
    private Vector3 goalPosition;

    /// <summary>
    /// Crea el grid completo del nivel.
    /// </summary>
    public void CreateGrid()
    {
        // Limpiar grid anterior
        ClearGrid();

        // Crear tiles
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                Vector3 pos = new Vector3(x - gridSize.x / 2, 0, y - gridSize.y / 2);
                CreateTile(pos);
            }
        }
    }

    void CreateTile(Vector3 position)
    {
        GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Quad);
        tile.name = $"Tile_{(int)position.x}_{(int)position.z}";
        tile.transform.position = position;
        tile.transform.rotation = Quaternion.Euler(90, 0, 0);
        tile.transform.parent = transform;

        if (tileMaterial)
            tile.GetComponent<Renderer>().material = tileMaterial;
    }

    /// <summary>
    /// Agrega un obstáculo en la posición del grid.
    /// </summary>
    public void AddObstacle(Vector3 gridPosition)
    {
        if (obstacles.ContainsKey(gridPosition)) return;

        GameObject obstacle = GameObject.CreatePrimitive(PrimitiveType.Quad);
        obstacle.name = $"Obstacle_{(int)gridPosition.x}_{(int)gridPosition.z}";
        obstacle.transform.position = gridPosition + new Vector3(0, 0.01f, 0);
        obstacle.transform.rotation = Quaternion.Euler(90, 0, 0);

        if (obstacleMaterial)
            obstacle.GetComponent<Renderer>().material = obstacleMaterial;

        obstacles[gridPosition] = obstacle;
    }

    /// <summary>
    /// Crea la meta.
    /// </summary>
    public void SetGoal(Vector3 position)
    {
        goalPosition = position;

        GameObject goal = GameObject.CreatePrimitive(PrimitiveType.Quad);
        goal.name = "Goal";
        goal.transform.position = position + new Vector3(0, 0.01f, 0);
        goal.transform.rotation = Quaternion.Euler(90, 0, 0);

        if (goalMaterial)
            goal.GetComponent<Renderer>().material = goalMaterial;
    }

    /// <summary>
    /// Crea el robot en la posición inicial.
    /// </summary>
    public RobotController SpawnRobot(Vector3 position)
    {
        GameObject robot = GameObject.CreatePrimitive(PrimitiveType.Cube);
        robot.name = "Robot";
        robot.transform.position = position;

        RobotController controller = robot.AddComponent<RobotController>();
        return controller;
    }

    /// <summary>
    /// Verifica si una posición tiene obstáculo.
    /// </summary>
    public bool IsBlocked(Vector3 gridPosition)
    {
        return obstacles.ContainsKey(gridPosition);
    }

    /// <summary>
    /// Obtiene la posición de la meta.
    /// </summary>
    public Vector3 GetGoalPosition()
    {
        return goalPosition;
    }

    /// <summary>
    /// Limpia el grid.
    /// </summary>
    public void ClearGrid()
    {
        // Limpiar hijos
        List<GameObject> toDestroy = new List<GameObject>();
        foreach (Transform child in transform)
            toDestroy.Add(child.gameObject);

        foreach (GameObject obj in toDestroy)
            Object.DestroyImmediate(obj);

        // Limpiar obstáculos
        obstacles.Clear();
    }

    /// <summary>
    /// Obtiene posición del grid desde posición del mundo.
    /// </summary>
    public Vector3 WorldToGrid(Vector3 worldPos)
    {
        return new Vector3(
            Mathf.RoundToInt(worldPos.x),
            0,
            Mathf.RoundToInt(worldPos.z)
        );
    }
}