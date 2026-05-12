using UnityEngine;

/// <summary>
/// Data for one cell in Boti's grid.
/// </summary>
[System.Serializable]
public class GridTile
{
    public Vector2Int coordinates;
    public bool walkable = true;
    public bool hasObstacle = false;
    public bool hasInteractable = false;
    public Interactable interactable;

    public GridTile(Vector2Int coordinates)
    {
        this.coordinates = coordinates;
    }

    public bool CanWalkHere()
    {
        return walkable && !hasObstacle;
    }
}
