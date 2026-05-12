using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Stores the logical state of Boti's world.
/// Visual objects are spawned from this data.
/// </summary>
public class WorldState : MonoBehaviour
{
    public int width = 21;
    public int height = 21;
    public float tileSize = 1f;
    public Vector3 originPosition = new Vector3(-10, 0, -10);

    public List<WorldTileData> tiles = new List<WorldTileData>();

    private Dictionary<Vector2Int, WorldTileData> tilesByPosition = new Dictionary<Vector2Int, WorldTileData>();

    void Awake()
    {
        RebuildLookup();
    }

    public void CreateEmptyWorld(int width, int height, float tileSize, Vector3 originPosition)
    {
        this.width = width;
        this.height = height;
        this.tileSize = tileSize;
        this.originPosition = originPosition;

        tiles.Clear();
        tilesByPosition.Clear();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                WorldTileData tile = new WorldTileData(new Vector2Int(x, y), WorldTileType.Grass, true);
                tiles.Add(tile);
                tilesByPosition.Add(tile.coordinates, tile);
            }
        }
    }

    public void RebuildLookup()
    {
        tilesByPosition.Clear();

        foreach (WorldTileData tile in tiles)
        {
            if (tile != null && !tilesByPosition.ContainsKey(tile.coordinates))
                tilesByPosition.Add(tile.coordinates, tile);
        }
    }

    public WorldTileData GetTile(Vector2Int coordinates)
    {
        WorldTileData tile;
        tilesByPosition.TryGetValue(coordinates, out tile);
        return tile;
    }

    public WorldTileData GetTileAtWorldPosition(Vector3 worldPosition)
    {
        return GetTile(WorldToGridPosition(worldPosition));
    }

    public Vector2Int WorldToGridPosition(Vector3 worldPosition)
    {
        int x = Mathf.RoundToInt((worldPosition.x - originPosition.x) / tileSize);
        int y = Mathf.RoundToInt((worldPosition.z - originPosition.z) / tileSize);
        return new Vector2Int(x, y);
    }

    public Vector3 GridToWorldPosition(Vector2Int coordinates)
    {
        return new Vector3(
            originPosition.x + coordinates.x * tileSize,
            originPosition.y,
            originPosition.z + coordinates.y * tileSize
        );
    }

    public void SetTileType(Vector2Int coordinates, WorldTileType tileType, bool walkable)
    {
        WorldTileData tile = GetTile(coordinates);
        if (tile == null)
            return;

        tile.tileType = tileType;
        tile.walkable = walkable;
    }

    public void SetObstacle(Vector2Int coordinates, WorldObstacleType obstacleType)
    {
        WorldTileData tile = GetTile(coordinates);
        if (tile == null)
            return;

        tile.obstacleType = obstacleType;
        tile.walkable = obstacleType == WorldObstacleType.None && tile.tileType != WorldTileType.Water;
    }

    public void SetResource(Vector2Int coordinates, WorldResourceType resourceType)
    {
        WorldTileData tile = GetTile(coordinates);
        if (tile == null || tile.HasObstacle() || tile.HasStructure() || !tile.walkable)
            return;

        tile.resourceType = resourceType;
        tile.wasCollected = false;
    }

    public void CollectResourceAtWorldPosition(Vector3 worldPosition)
    {
        WorldTileData tile = GetTileAtWorldPosition(worldPosition);
        if (tile == null)
            return;

        tile.wasCollected = true;
        tile.resourceType = WorldResourceType.None;
        tile.worldObject = null;
    }

    public void RemoveObstacleAtWorldPosition(Vector3 worldPosition)
    {
        WorldTileData tile = GetTileAtWorldPosition(worldPosition);
        if (tile == null)
            return;

        tile.obstacleType = WorldObstacleType.None;
        tile.walkable = tile.tileType != WorldTileType.Water && !tile.HasStructure();
        tile.worldObject = null;
    }

    public bool CanPlaceStructure(Vector2Int coordinates)
    {
        WorldTileData tile = GetTile(coordinates);
        if (tile == null)
            return false;

        return tile.walkable && !tile.HasObstacle() && !tile.HasResource() && !tile.HasStructure();
    }

    public bool PlaceStructure(Vector2Int coordinates, WorldStructureType structureType)
    {
        if (!CanPlaceStructure(coordinates))
            return false;

        WorldTileData tile = GetTile(coordinates);
        tile.structureType = structureType;
        tile.walkable = false;
        return true;
    }

    public WorldSaveData CreateSaveData()
    {
        WorldSaveData saveData = new WorldSaveData();
        saveData.width = width;
        saveData.height = height;
        saveData.tileSize = tileSize;
        saveData.originPosition = originPosition;

        foreach (WorldTileData tile in tiles)
        {
            WorldTileSaveData tileSaveData = new WorldTileSaveData();
            tileSaveData.x = tile.coordinates.x;
            tileSaveData.y = tile.coordinates.y;
            tileSaveData.tileType = tile.tileType;
            tileSaveData.walkable = tile.walkable;
            tileSaveData.resourceType = tile.resourceType;
            tileSaveData.obstacleType = tile.obstacleType;
            tileSaveData.structureType = tile.structureType;
            tileSaveData.wasCollected = tile.wasCollected;
            saveData.tiles.Add(tileSaveData);
        }

        return saveData;
    }

    public void LoadFromSaveData(WorldSaveData saveData)
    {
        if (saveData == null)
            return;

        width = saveData.width;
        height = saveData.height;
        tileSize = saveData.tileSize;
        originPosition = saveData.originPosition;

        tiles.Clear();

        foreach (WorldTileSaveData tileSaveData in saveData.tiles)
        {
            WorldTileData tile = new WorldTileData(
                new Vector2Int(tileSaveData.x, tileSaveData.y),
                tileSaveData.tileType,
                tileSaveData.walkable
            );

            tile.resourceType = tileSaveData.resourceType;
            tile.obstacleType = tileSaveData.obstacleType;
            tile.structureType = tileSaveData.structureType;
            tile.wasCollected = tileSaveData.wasCollected;
            tile.worldObject = null;
            tiles.Add(tile);
        }

        RebuildLookup();
    }
}
