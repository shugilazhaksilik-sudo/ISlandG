using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CampfireState
{
    Unlit,        // Не подожжен (основа)
    Burning,      // Горит (анимация 4 кадров)
    Extinguished  // Потух (угольки, нельзя зажечь снова)
}

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class Campfire : MonoBehaviour
{
    [Header("Sprites")]
    [Tooltip("Спрайт основы костра (не подожжен)")]
    public Sprite unlitSprite;
    [Tooltip("Спрайт угольков (потухший костер)")]
    public Sprite extinguishedSprite;
    [Tooltip("4 кадра анимации горения костра")]
    public Sprite[] burningSprites;
    [Tooltip("Скорость смены кадров горения (в секундах)")]
    public float animationSpeed = 0.2f;

    [Header("Items")]
    [Tooltip("Предмет Искра (Spark) для поджигания")]
    public ItemData sparkItem;
    [Tooltip("Предмет Основа Костра (10Fireplace) для возврата при разборке")]
    public ItemData fireplaceItem;

    [Header("Shelter & Rain Settings")]
    [Tooltip("Радиус проверки укрытия под большой пальмой (BigPalm)")]
    public float shelterCheckRadius = 1.8f;
    [Tooltip("Задержка перед тушением костра под ливнем (в секундах)")]
    public float rainExtinguishDelay = 1.0f;

    [Header("Collider Settings")]
    [Tooltip("Размер коллайдера для основы костра (Unlit / Burning)")]
    public Vector2 normalColliderSize = new Vector2(1f, 1f);
    [Tooltip("Смещение коллайдера для основы костра")]
    public Vector2 normalColliderOffset = new Vector2(0f, 0f);

    [Tooltip("Размер коллайдера для потухшего костра (Extinguished)")]
    public Vector2 extinguishedColliderSize = new Vector2(1.5f, 1.5f);
    [Tooltip("Смещение коллайдера для потухшего костра")]
    public Vector2 extinguishedColliderOffset = new Vector2(0f, 0f);

    [Header("Damage Settings")]
    [Tooltip("Урон в секунду, если игрок наступает прямо на горящий костер")]
    public float burnDamage = 15f;
    [Tooltip("Максимальная дистанция от центра костра для нанесения урона")]
    public float burnRadius = 0.4f;

    [Header("Audio (Optional)")]
    public AudioSource audioSource;
    public AudioClip igniteSound;
    public AudioClip cracklingSound;
    public AudioClip cookingSound;
    public AudioClip extinguishSound;

    // Глобальный статический список всех активных костров в мире
    public static List<Campfire> activeCampfires = new List<Campfire>();

    private CampfireState currentState = CampfireState.Unlit;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;
    private float animTimer;
    private int currentFrameIndex;
    private float rainExposureTimer = 0f; // Таймер нахождения под дождем

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    private void OnEnable()
    {
        activeCampfires.Add(this);
    }

    private void OnDisable()
    {
        activeCampfires.Remove(this);
    }

    private void Start()
    {
        // Устанавливаем базовое состояние при установке костра в мире
        SetState(CampfireState.Unlit);
    }

    private void Update()
    {
        // Проигрываем 4-кадровую анимацию пламени без использования Animator
        if (currentState == CampfireState.Burning)
        {
            if (burningSprites != null && burningSprites.Length > 0)
            {
                animTimer += Time.deltaTime;
                if (animTimer >= animationSpeed)
                {
                    animTimer = 0f;
                    currentFrameIndex = (currentFrameIndex + 1) % burningSprites.Length;
                    spriteRenderer.sprite = burningSprites[currentFrameIndex];
                }
            }

            // Тушение дождем, если костер не под укрытием большой пальмы, с задержкой
            if (WeatherManager.instance != null && WeatherManager.instance.IsRaining() && !IsShelteredByBigPalm())
            {
                rainExposureTimer += Time.deltaTime;
                if (rainExposureTimer >= rainExtinguishDelay)
                {
                    Debug.Log("Костер потух под усилившимся дождем!");
                    Extinguish();
                    rainExposureTimer = 0f;
                }
            }
            else
            {
                rainExposureTimer = 0f; // Сбрасываем таймер, если дождь кончился или костер укрыли
            }

            // Нанесение урона игроку, если он встает прямо поверх огня (дистанция < burnRadius)
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                float dist = Vector2.Distance(transform.position, player.transform.position);
                if (dist < burnRadius)
                {
                    if (SurvivalSystem.instance != null)
                    {
                        SurvivalSystem.instance.TakeDamage(burnDamage * Time.deltaTime, DamageType.Fire);
                    }
                }
            }
        }
    }

    public void SetState(CampfireState newState)
    {
        currentState = newState;
        animTimer = 0f;
        currentFrameIndex = 0;
        rainExposureTimer = 0f;

        switch (currentState)
        {
            case CampfireState.Unlit:
                if (unlitSprite != null)
                    spriteRenderer.sprite = unlitSprite;
                if (boxCollider != null)
                {
                    boxCollider.size = normalColliderSize;
                    boxCollider.offset = normalColliderOffset;
                }
                if (audioSource != null)
                    audioSource.Stop();
                break;

            case CampfireState.Burning:
                if (boxCollider != null)
                {
                    boxCollider.size = normalColliderSize;
                    boxCollider.offset = normalColliderOffset;
                }
                if (audioSource != null && cracklingSound != null)
                {
                    audioSource.clip = cracklingSound;
                    audioSource.loop = true;
                    audioSource.Play();
                }
                if (audioSource != null && igniteSound != null)
                {
                    audioSource.PlayOneShot(igniteSound);
                }
                break;

            case CampfireState.Extinguished:
                if (extinguishedSprite != null)
                    spriteRenderer.sprite = extinguishedSprite;
                if (boxCollider != null)
                {
                    boxCollider.size = extinguishedColliderSize;
                    boxCollider.offset = extinguishedColliderOffset;
                }
                if (audioSource != null)
                {
                    audioSource.Stop();
                    if (extinguishSound != null)
                        audioSource.PlayOneShot(extinguishSound);
                }
                break;
        }
    }

    // Проверка, горит ли сейчас костер
    public bool IsBurning()
    {
        return currentState == CampfireState.Burning;
    }

    // Метод затухания костра
    public void Extinguish()
    {
        SetState(CampfireState.Extinguished);
    }

    // Проверка, находится ли костер под ветками Большой Пальмы (BigPalm)
    public bool IsShelteredByBigPalm()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, shelterCheckRadius);
        foreach (var col in colliders)
        {
            // Ищем ствол или ветки большой пальмы по ее названию
            if (col.gameObject.name.Contains("BigPalm"))
            {
                return true;
            }
        }
        return false;
    }

    private void OnMouseOver()
    {
        // Взаимодействие на правую кнопку мыши (ПКМ)
        if (Input.GetMouseButtonDown(1))
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;

            // Проверяем дистанцию между игроком и костром
            float distance = Vector2.Distance(transform.position, player.transform.position);
            if (distance > 2f)
            {
                Debug.Log("Слишком далеко от костра!");
                return;
            }

            Interact(player);
        }
    }

    private void Interact(GameObject player)
    {
        ItemData activeItem = InventoryManager.instance.GetActiveItem();
        InventorySlot activeSlot = InventoryManager.instance.allSlots[InventoryManager.instance.selectedSlotIndex];

        switch (currentState)
        {
            case CampfireState.Unlit:
                // Разжигаем костер Искрой
                if (activeItem == sparkItem)
                {
                    // Проверяем, идет ли дождь и защищен ли костер большой пальмой
                    if (WeatherManager.instance != null && WeatherManager.instance.IsRaining() && !IsShelteredByBigPalm())
                    {
                        Debug.Log("Нельзя разжечь костер под дождем без укрытия!");
                        return;
                    }

                    InventoryManager.instance.RemoveActiveItem(); // Потребляем 1 искру
                    SetState(CampfireState.Burning);
                    Debug.Log("Костер успешно разожжен с помощью Искры!");
                }
                else
                {
                    Debug.Log("Этот костер не горит. Чтобы поджечь его, выберите в руку Искру и кликните ПКМ.");
                }
                break;

            case CampfireState.Burning:
                // Жарим пищу
                if (activeItem != null)
                {
                    if (activeItem.isFood && activeItem.cookedVersion != null)
                    {
                        CookFood(activeSlot, activeItem.cookedVersion);
                    }
                    else
                    {
                        Debug.Log("Этот предмет нельзя пожарить.");
                    }
                }
                break;

            case CampfireState.Extinguished:
                // Убираем костер навсегда и возвращаем основу
                ClearExtinguishedCampfire();
                break;
        }
    }

    private void CookFood(InventorySlot slot, ItemData cookedFood)
    {
        // Воспроизводим звук шипения/готовки
        if (audioSource != null && cookingSound != null)
        {
            audioSource.PlayOneShot(cookingSound);
        }

        // Логика поштучной жарки еды из стака
        if (slot.amount == 1)
        {
            // Используем метод AddItem, чтобы полностью перерисовать иконку в руке!
            slot.AddItem(cookedFood, 1);
            Debug.Log($"Приготовлено: {cookedFood.itemName}! Теперь предмет в вашей руке.");
        }
        else if (slot.amount > 1)
        {
            // Если в стаке больше 1 предмета, уменьшаем сырые в руке на 1
            slot.amount--;
            slot.UpdateUI();

            // Добавляем 1 готовую еду в инвентарь
            bool success = InventoryManager.instance.TryAddItem(cookedFood);
            if (!success)
            {
                // Если в инвентаре нет свободного места, готовая еда падает на землю
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                Vector2 spawnPos = (player != null) ? (Vector2)player.transform.position : (Vector2)transform.position;
                if (cookedFood.dropPrefab != null)
                {
                    Vector2 randomDir = Random.insideUnitCircle.normalized * 0.8f;
                    Instantiate(cookedFood.dropPrefab, spawnPos + randomDir, Quaternion.identity);
                }
                Debug.Log($"Инвентарь полон! Приготовленный {cookedFood.itemName} выпал на землю.");
            }
            else
            {
                Debug.Log($"Приготовлено: {cookedFood.itemName}! 1 шт. добавлена в инвентарь. В руке осталось сырых: {slot.amount}.");
            }
        }
    }

    private void ClearExtinguishedCampfire()
    {
        Debug.Log("Разбираем потухший костер...");

        // Возвращаем основу костра (10Fireplace) игроку
        if (fireplaceItem != null)
        {
            bool success = InventoryManager.instance.TryAddItem(fireplaceItem);
            if (!success)
            {
                // Если инвентарь забит, выбрасываем префаб основы костра на землю
                if (fireplaceItem.dropPrefab != null)
                {
                    Vector2 randomDir = Random.insideUnitCircle.normalized * 0.8f;
                    Instantiate(fireplaceItem.dropPrefab, (Vector2)transform.position + randomDir, Quaternion.identity);
                }
                Debug.Log("Инвентарь полон! Основа костра выпала на землю.");
            }
            else
            {
                Debug.Log("Потухший костер убран! Основа костра возвращена в инвентарь.");
            }
        }

        // Удаляем объект костра навсегда
        Destroy(gameObject);
    }
}
