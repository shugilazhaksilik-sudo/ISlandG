using UnityEngine;

public class TinderDrying : MonoBehaviour
{
    [Header("Настройки стадий (Fiber)")]
    public Sprite greenSprite;      // Спрайт зеленой травы (1-я стадия)
    public Sprite yellowSprite;     // Спрайт желто-зеленой травы (2-я стадия)
    public Sprite drySprite;        // Спрайт сухой травы (3-я стадия)

    [Tooltip("Время сушки в секундах. Для тестов поставь по 5 секунд.")]
    public float timeToYellow = 5f;
    public float timeToDry = 5f;

    [Header("Данные для инвентаря")]
    public ItemData dryFiberData;

    [Header("Текущее состояние")]
    public int currentStage = 0; // 0 - зеленая, 1 - желто-зеленая, 2 - сухая
    private float timer = 0f;

    private SpriteRenderer spriteRenderer;
    private Item itemComponent;
    private Collider2D fetchCollider;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        itemComponent = GetComponent<Item>();
        fetchCollider = GetComponent<Collider2D>(); // Твой триггер для подбора

        // БЛОКИРОВКА: Пока трава сырая, подбирать её нельзя!
        if (itemComponent != null) itemComponent.enabled = false;

        UpdateVisuals();
    }

    void Update()
    {
        if (currentStage >= 2) return;

        timer += Time.deltaTime;

        // Переход от зеленой к желто-зеленой
        if (currentStage == 0 && timer >= timeToYellow)
        {
            currentStage = 1;
            timer = 0f;
            UpdateVisuals();
        }
        // Переход от желто-зеленой к сухой (ГОТОВО)
        else if (currentStage == 1 && timer >= timeToDry)
        {
            currentStage = 2;
            timer = 0f;
            UpdateVisuals();

            // РАЗБЛОКИРОВКА: Трава высохла, теперь её можно поднять!
            if (itemComponent != null && dryFiberData != null)
            {
                itemComponent.itemData = dryFiberData;
                itemComponent.enabled = true; // Включаем скрипт подбора
                Debug.Log("[Сушка] Волокно высохло! Скрипт Item активирован, можно подбирать.");
            }
        }
    }

    private void UpdateVisuals()
    {
        if (currentStage == 0) spriteRenderer.sprite = greenSprite;
        else if (currentStage == 1) spriteRenderer.sprite = yellowSprite;
        else if (currentStage == 2) spriteRenderer.sprite = drySprite;
    }

    public bool CanBePickedUp()
    {
        return currentStage == 2;
    }
}