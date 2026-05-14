using UnityEngine;

[CreateAssetMenu(fileName = "PlankRecipe", menuName = "Boti/Craft Recipe")]
public class CraftRecipe : ScriptableObject
{
    public string recipeName;
    public CraftResult outputItem;
    public int outputAmount = 1;
    public CraftIngredient[] ingredients;
}

[System.Serializable]
public class CraftIngredient
{
    public CraftResourceType resource;
    public int amount;
}

public enum CraftResourceType { Wood, Scrap, Crystal }
public enum CraftResult { Plank, MetalPlate }