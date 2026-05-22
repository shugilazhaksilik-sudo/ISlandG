using UnityEngine;

[System.Serializable]
public class CraftingIngredient
{
    public ItemData item; // Какой предмет нужен
    public int amount;    // Сколько штук нужно
}

[CreateAssetMenu(fileName = "New Recipe", menuName = "Inventory/Crafting Recipe")]
public class CraftingRecipe : ScriptableObject
{
    public ItemData resultItem; // Что получаем в итоге
    public int resultAmount = 1; // Сколько штук получаем (например, 1 доска дает 4 палки)

    public CraftingIngredient[] ingredients; // Список того, что нужно для крафта
}