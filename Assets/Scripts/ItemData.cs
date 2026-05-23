using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public GameObject dropPrefab; // Префаб выброшенного предмета

    public bool canBeDropped = true; // Можно ли выбросить

    [Header("Строительство и размещение")]
    public bool isPlaceable;         // Можно ли разместить на сцене
    public GameObject placeablePrefab; // Префаб для размещения (например, костер)

    [Header("Время жизни")]
    public bool hasLifetime;        // Есть ли время жизни предмета
    public float maxLifetime = 15f; // Максимальное время жизни

    [Header("Инструмент и прочность")]
    public bool isTool;             // Является ли инструментом
    public int maxDurability = 50;  // Максимальная прочность

    [Header("Анимации инструмента")]
    public Sprite sideOverlaySprite;
    public string animationNameUp;
    public string animationNameDown;
    public string animationNameLeft;
    public string animationNameRight;

    [Header("Свойства еды")]
    public bool isFood;               // Является ли пищей (сырой)
    public ItemData cookedVersion;    // Во что превращается после готовки
}
