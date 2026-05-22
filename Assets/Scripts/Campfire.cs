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
public class Campfire : MonoBehaviour
{
    [System.Serializable]
    public class CookingRecipe
    {
        public ItemData rawFood;    // Сырой предмет (например, RawShrimp)
        public ItemData cookedFood; // Готовый предмет (например, FriedShrimp)
    }

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

    [Header("Cooking Settings")]
    [Tooltip("Список рецептов готовки на костре")]
    public List<CookingRecipe> cookingRecipes = new List<CookingRecipe>();

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
    private float animTimer;
    private int currentFrameIndex;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
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
        if (currentState == CampfireState.Burning && burningSprites != null && burningSprites.Length > 0)
        {
            animTimer += Time.deltaTime;
            if (animTimer >= animationSpeed)
            {
                animTimer = 0f;
                currentFrameIndex = (currentFrameIndex + 1) % burningSprites.Length;
                spriteRenderer.sprite = burningSprites[currentFrameIndex];
            }
        }
    }

    public void SetState(CampfireState newState)
    {
        currentState = newState;
        animTimer = 0f;
        currentFrameIndex = 0;

        switch (currentState)
        {
            case CampfireState.Unlit:
                if (unlitSprite != null)
                    spriteRenderer.sprite = unlitSprite;
                if (audioSource != null)
                    audioSource.Stop();
                break;

            case CampfireState.Burning:
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
                    CookingRecipe match = FindRecipe(activeItem);
                    if (match != null)
                    {
                        CookFood(activeSlot, match);
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

    private CookingRecipe FindRecipe(ItemData raw)
    {
        foreach (var recipe in cookingRecipes)
        {
            if (recipe.rawFood == raw)
                return recipe;
        }
        return null;
    }

    private void CookFood(InventorySlot slot, CookingRecipe recipe)
    {
        // Воспроизводим звук шипения/готовки
        if (audioSource != null && cookingSound != null)
        {
            audioSource.PlayOneShot(cookingSound);
        }

        // Логика поштучной жарки еды из стака
        if (slot.amount == 1)
        {
            // Если в активной руке ровно 1 предмет, заменяем его готовым в том же слоте
            slot.item = recipe.cookedFood;
            slot.amount = 1;
            slot.UpdateUI();
            Debug.Log($"Приготовлено: {recipe.cookedFood.itemName}! Теперь предмет в вашей руке.");
        }
        else if (slot.amount > 1)
        {
            // Если в стаке больше 1 предмета, уменьшаем сырые в руке на 1
            slot.amount--;
            slot.UpdateUI();

            // Добавляем 1 готовую еду в инвентарь
            bool success = InventoryManager.instance.TryAddItem(recipe.cookedFood);
            if (!success)
            {
                // Если в инвентаре нет свободного места, готовая еда падает на землю
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                Vector2 spawnPos = (player != null) ? (Vector2)player.transform.position : (Vector2)transform.position;
                if (recipe.cookedFood.dropPrefab != null)
                {
                    Vector2 randomDir = Random.insideUnitCircle.normalized * 0.8f;
                    Instantiate(recipe.cookedFood.dropPrefab, spawnPos + randomDir, Quaternion.identity);
                }
                Debug.Log($"Инвентарь полон! Приготовленный {recipe.cookedFood.itemName} выпал на землю.");
            }
            else
            {
                Debug.Log($"Приготовлено: {recipe.cookedFood.itemName}! 1 шт. добавлена в инвентарь. В руке осталось сырых: {slot.amount}.");
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
