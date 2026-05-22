using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public GameObject dropPrefab; // Префаб, который появится на земле при выбрасывании

    // --- ДОБАВЬ ЭТУ СТРОКУ ---
    public bool canBeDropped = true; // Галочка: можно ли выбросить предмет клавишей Q или мышкой?
    // -------------------------

    [Header("Настройки строительства (Размещение)")]
    public bool isPlaceable;         // Галочка: можно ли это поставить на землю?
    public GameObject placeablePrefab; // ЧТО именно ставим (например, префаб FirePloughStation)

    // --- НОВЫЕ НАСТРОЙКИ ДЛЯ ТАЙМЕРА ---
    [Header("Настройки времени жизни")]
    public bool hasLifetime;        // Галочка: есть ли у предмета срок годности (для искры)?
    public float maxLifetime = 15f; // Сколько секунд он живет?

    // --- НОВЫЕ СТРОЧКИ ДЛЯ ИНСТРУМЕНТОВ ---
    [Header("Настройки прочности (Инструменты)")]
    public bool isTool;             // Галочка: это инструмент?
    public int maxDurability = 50;  // На сколько ударов его хватает в идеале?

    [Header("Гибридная Анимация Инструмента")]
    public Sprite sideOverlaySprite;
    public string animationNameUp;
    public string animationNameDown;
    // --- НОВЫЕ СТРОКИ ---
    public string animationNameLeft;  // Анимация пустой руки влево
    public string animationNameRight; // Анимация пустой руки вправо
}