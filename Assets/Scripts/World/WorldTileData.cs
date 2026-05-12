using UnityEngine;

public enum WorldTileType
{
    Grass,
    DarkGrass,
    Stone,
    Path,
    Water
}

public enum WorldResourceType
{
    None,
    Crystal,
    Scrap
}

public enum WorldObstacleType
{
    None,
    Rock,
    Tree
}

public enum WorldStructureType
{
    None,
    Wall,
    StorageBox
}

/// <summary>
/// Data for one tile in the world.
/// This is the source of truth, not the visual GameObject.
/// </summary>
[System.Serializable]
public class WorldTileData
{
    public Vector2Int coordinates;
    public WorldTileType tileType;
    public bool walkable;
    public WorldResourceType resourceType;
    public WorldObstacleType obstacleType;
    public WorldStructureType structureType;
    public bool wasCollected;
    public GameObject worldObject;

    public WorldTileData(Vector2Int coordinates, WorldTileType tileType, bool walkable)
    {
        this.coordinates = coordinates;
        this.tileType = tileType;
        this.walkable = walkable;
        resourceType = WorldResourceType.None;
        obstacleType = WorldObstacleType.None;
        structureType = WorldStructureType.None;
        wasCollected = false;
    }

    public bool HasResource()
    {
        return resourceType != WorldResourceType.None && !wasCollected;
    }

    public bool HasObstacle()
    {
        return obstacleType != WorldObstacleType.None;
    }

    public bool HasStructure()
    {
        return structureType != WorldStructureType.None;
    }
}
