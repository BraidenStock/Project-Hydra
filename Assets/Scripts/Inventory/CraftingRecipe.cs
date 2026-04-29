using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]

public class Ingredient
{
    public ItemSO item;
    public int amount;
}
[CreateAssetMenu(fileName = "Recipe", menuName = "NewRecipe")]
public class CraftingRecipe : ScriptableObject
{
    public List<Ingredient> ingredients;
    public ItemSO result;
    public int resultAmount;
}