using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Plain data that can be converted to JSON.
/// It contains no GameObject references.
/// </summary>
[System.Serializable]
public class SaveData
{
    public int playerGridX;
    public int playerGridY;

    public int crystalCount;
    public int scrapCount;
    public int woodCount;
    public int plankCount;
    public int metalPlateCount;
    public BotiTool equippedTool;

    public WorldSaveData world;
}

[System.Serializable]
public class WorldSaveData
{
    public int width;
    public int height;
    public float tileSize;
    public Vector3 originPosition;
    public List<WorldTileSaveData> tiles = new List<WorldTileSaveData>();
}

[System.Serializable]
public class WorldTileSaveData
{
    public int x;
    public int y;
    public WorldTileType tileType;
    public bool walkable;
    public WorldResourceType resourceType;
    public WorldObstacleType obstacleType;
    public WorldStructureType structureType;
    public bool wasCollected;
}
