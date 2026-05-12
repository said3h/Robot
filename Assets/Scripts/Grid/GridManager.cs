using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Handles grid tiles, world/grid conversion and map occupation.
/// </summary>
public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    public int width = 21;
    public int height = 21;
    public float tileSize = 1f;
    public Vector3 originPosition = new Vector3(-10, 0, -10);

    [Header("Legacy Settings")]
    public Vector2 gridSize = new Vector2(21, 21);

    [Header("Legacy Prefabs")]
    public GameObject tilePrefab;
    public GameObject obstaclePrefab;
    public GameObject goalPrefab;
    public GameObject robotPrefab;

    [Header("Legacy Materials")]
    public Material tileMaterial;
    public Material obstacleMaterial;
    public Material goalMaterial;

    private Dictionary<Vector2Int, GridTile> tiles = new Dictionary<Vector2Int, GridTile>();
    private Dictionary<Vector3, GameObject> legacyObstacles = new Dictionary<Vector3, GameObject>();
    private Vector3 goalPosition;

    void Awake()
    {
        CreateGrid();
    }

    public void CreateGrid()
    {
        width = Mathf.RoundToInt(gridSize.x);
        height = Mathf.RoundToInt(gridSize.y);
        tiles.Clear();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int coordinates = new Vector2Int(x, y);
                tiles.Add(coordinates, new GridTile(coordinates));
            }
        }
    }

    public Vector2Int WorldToGridPosition(Vector3 worldPosition)
    {
        int x = Mathf.RoundToInt((worldPosition.x - originPosition.x) / tileSize);
        int y = Mathf.RoundToInt((worldPosition.z - originPosition.z) / tileSize);
        return new Vector2Int(x, y);
    }

    public Vector3 GridToWorldPosition(Vector2Int gridPosition)
    {
        return new Vector3(
            originPosition.x + gridPosition.x * tileSize,
            originPosition.y,
            originPosition.z + gridPosition.y * tileSize
        );
    }

    public GridTile GetTile(Vector2Int gridPosition)
    {
        GridTile tile;
        tiles.TryGetValue(gridPosition, out tile);
        return tile;
    }

    public bool IsInsideGrid(Vector2Int gridPosition)
    {
        return tiles.ContainsKey(gridPosition);
    }

    public bool CanMoveToWorldPosition(Vector3 worldPosition)
    {
        Vector2Int gridPosition = WorldToGridPosition(worldPosition);
        return CanMoveToGridPosition(gridPosition);
    }

    public bool CanMoveToGridPosition(Vector2Int gridPosition)
    {
        GridTile tile = GetTile(gridPosition);
        return tile != null && tile.CanWalkHere();
    }

    public void ApplyWorldState(WorldState worldState)
    {
        if (worldState == null)
            return;

        width = worldState.width;
        height = worldState.height;
        tileSize = worldState.tileSize;
        originPosition = worldState.originPosition;
        gridSize = new Vector2(width, height);
        CreateGrid();

        foreach (WorldTileData worldTile in worldState.tiles)
        {
            GridTile gridTile = GetTile(worldTile.coordinates);
            if (gridTile == null)
                continue;

            gridTile.walkable = worldTile.walkable;
            gridTile.hasObstacle = worldTile.HasObstacle() || worldTile.HasStructure() || !worldTile.walkable;
        }
    }

    public void SetObstacleAtWorldPosition(Vector3 worldPosition, bool hasObstacle)
    {
        GridTile tile = GetTile(WorldToGridPosition(worldPosition));
        if (tile == null)
            return;

        tile.hasObstacle = hasObstacle;
        tile.walkable = !hasObstacle;
    }

    public void SetInteractableAtWorldPosition(Vector3 worldPosition, Interactable interactable)
    {
        GridTile tile = GetTile(WorldToGridPosition(worldPosition));
        if (tile == null)
            return;

        tile.hasInteractable = interactable != null;
        tile.interactable = interactable;
    }

    public void ClearInteractableAtWorldPosition(Vector3 worldPosition)
    {
        SetInteractableAtWorldPosition(worldPosition, null);
    }

    public void AddObstacle(Vector3 gridPosition)
    {
        SetObstacleAtWorldPosition(gridPosition, true);

        if (legacyObstacles.ContainsKey(gridPosition))
            return;

        GameObject obstacle = GameObject.CreatePrimitive(PrimitiveType.Quad);
        obstacle.name = $"Obstacle_{(int)gridPosition.x}_{(int)gridPosition.z}";
        obstacle.transform.position = gridPosition + new Vector3(0, 0.01f, 0);
        obstacle.transform.rotation = Quaternion.Euler(90, 0, 0);

        if (obstacleMaterial != null)
            obstacle.GetComponent<Renderer>().material = obstacleMaterial;

        legacyObstacles[gridPosition] = obstacle;
    }

    public void SetGoal(Vector3 position)
    {
        goalPosition = position;

        GameObject goal = GameObject.CreatePrimitive(PrimitiveType.Quad);
        goal.name = "Goal";
        goal.transform.position = position + new Vector3(0, 0.01f, 0);
        goal.transform.rotation = Quaternion.Euler(90, 0, 0);

        if (goalMaterial != null)
            goal.GetComponent<Renderer>().material = goalMaterial;
    }

    public RobotController SpawnRobot(Vector3 position)
    {
        GameObject robot = GameObject.CreatePrimitive(PrimitiveType.Cube);
        robot.name = "Robot";
        robot.transform.position = position;
        return robot.AddComponent<RobotController>();
    }

    public bool IsBlocked(Vector3 gridPosition)
    {
        GridTile tile = GetTile(WorldToGridPosition(gridPosition));
        return tile == null || !tile.CanWalkHere();
    }

    public Vector3 GetGoalPosition()
    {
        return goalPosition;
    }

    public void ClearGrid()
    {
        List<GameObject> toDestroy = new List<GameObject>();
        foreach (Transform child in transform)
            toDestroy.Add(child.gameObject);

        foreach (GameObject obj in toDestroy)
            Object.DestroyImmediate(obj);

        tiles.Clear();
        legacyObstacles.Clear();
    }

    public Vector3 WorldToGrid(Vector3 worldPos)
    {
        Vector2Int gridPosition = WorldToGridPosition(worldPos);
        return new Vector3(gridPosition.x, 0, gridPosition.y);
    }
}
