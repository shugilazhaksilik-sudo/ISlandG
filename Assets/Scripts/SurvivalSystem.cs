using UnityEngine;
using UnityEngine.UI;

public class SurvivalSystem : MonoBehaviour
{
    public static SurvivalSystem instance;

    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float starvationDamageRate = 1.0f; // Урон в сек при голоде 0
    public float thirstDamageRate = 1.5f;     // Урон в сек при жажде 0
    public float coldDamageRate = 2.0f;       // Урон в сек при замерзании 100

    [Header("Hunger Settings")]
    public float maxHunger = 100f;
    public float currentHunger;
    public float hungerDecreaseRate = 0.3f; // Снижение в секунду

    [Header("Thirst Settings")]
    public float maxThirst = 100f;
    public float currentThirst;
    public float thirstDecreaseRate = 0.5f; // Снижение в секунду

    [Header("Cold Settings")]
    public float maxCold = 100f;
    public float currentCold;
    public float coldIncreaseRateNight = 1.0f;      // Замерзание ночью
    public float coldIncreaseRateRain = 1.5f;       // Замерзание под дождем
    public float coldDecreaseRateNearFire = 3.0f;   // Отогрев у костра
    public float warmAreaRadius = 3.0f;             // Радиус тепла костра

    [Header("UI Sliders (Optional)")]
    public Slider healthSlider;
    public Slider hungerSlider;
    public Slider thirstSlider;
    public Slider coldSlider;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip eatingSound; // Звук хруста/жевания

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        // Инициализируем показатели на 100% при первом запуске
        currentHealth = maxHealth;
        currentHunger = maxHunger;
        currentThirst = maxThirst;
        currentCold = 0f; // 0 - тепло, 100 - окоченение

        // Настраиваем слайдеры
        if (healthSlider != null) { healthSlider.minValue = 0; healthSlider.maxValue = maxHealth; }
        if (hungerSlider != null) { hungerSlider.minValue = 0; hungerSlider.maxValue = maxHunger; }
        if (thirstSlider != null) { thirstSlider.minValue = 0; thirstSlider.maxValue = maxThirst; }
        if (coldSlider != null)   { coldSlider.minValue = 0;   coldSlider.maxValue = maxCold; }
    }

    private void Update()
    {
        // 1. Падение голода и жажды со временем
        currentHunger = Mathf.Max(0f, currentHunger - hungerDecreaseRate * Time.deltaTime);
        currentThirst = Mathf.Max(0f, currentThirst - thirstDecreaseRate * Time.deltaTime);

        // 2. Логика замерзания
        UpdateColdLogic();

        // 3. Получение урона при критических показателях
        UpdateDamageLogic();

        // 4. Поедание пищи по нажатию клавиши F
        if (Input.GetKeyDown(KeyCode.F))
        {
            TryConsumeActiveItem();
        }

        // 5. Обновление интерфейса
        UpdateUI();
    }

    private void UpdateColdLogic()
    {
        bool isNearFire = IsNearBurningCampfire();

        if (isNearFire)
        {
            // Игрок греется у костра
            currentCold = Mathf.Max(0f, currentCold - coldDecreaseRateNearFire * Time.deltaTime);
        }
        else
        {
            bool isRaining = (WeatherManager.instance != null && WeatherManager.instance.IsRaining());
            bool isNight = IsNight();

            if (isRaining)
            {
                // Замерзает под дождем быстро
                currentCold = Mathf.Min(maxCold, currentCold + coldIncreaseRateRain * Time.deltaTime);
            }
            else if (isNight)
            {
                // Замерзает ночью нормально
                currentCold = Mathf.Min(maxCold, currentCold + coldIncreaseRateNight * Time.deltaTime);
            }
            else
            {
                // Днем на солнце медленно согревается сам по себе
                currentCold = Mathf.Max(0f, currentCold - 0.5f * Time.deltaTime);
            }
        }
    }

    private void UpdateDamageLogic()
    {
        if (currentHunger <= 0f)
        {
            currentHealth = Mathf.Max(0f, currentHealth - starvationDamageRate * Time.deltaTime);
        }
        if (currentThirst <= 0f)
        {
            currentHealth = Mathf.Max(0f, currentHealth - thirstDamageRate * Time.deltaTime);
        }
        if (currentCold >= maxCold)
        {
            currentHealth = Mathf.Max(0f, currentHealth - coldDamageRate * Time.deltaTime);
        }
    }

    private void TryConsumeActiveItem()
    {
        ItemData activeItem = InventoryManager.instance.GetActiveItem();

        if (activeItem != null && activeItem.isFood)
        {
            // Восстанавливаем показатели
            currentHunger = Mathf.Min(maxHunger, currentHunger + activeItem.hungerRestoreValue);
            currentThirst = Mathf.Min(maxThirst, currentThirst + activeItem.thirstRestoreValue);
            currentHealth = Mathf.Min(maxHealth, currentHealth + activeItem.healthRestoreValue);

            // Воспроизводим звук жевания
            if (audioSource != null && eatingSound != null)
            {
                audioSource.PlayOneShot(eatingSound);
            }

            // Тратим 1 единицу предмета из рук
            InventoryManager.instance.RemoveActiveItem();

            Debug.Log($"Игрок съел {activeItem.itemName}. Восстановлено: Голод +{activeItem.hungerRestoreValue}, Жажда +{activeItem.thirstRestoreValue}, Здоровье +{activeItem.healthRestoreValue}");
        }
        else
        {
            Debug.Log("В руках нет еды!");
        }
    }

    private void UpdateUI()
    {
        if (healthSlider != null) healthSlider.value = currentHealth;
        if (hungerSlider != null) hungerSlider.value = currentHunger;
        if (thirstSlider != null) thirstSlider.value = currentThirst;
        if (coldSlider != null)   coldSlider.value = currentCold;
    }

    // Получение урона (вызывается извне, например, при стоянии на костре)
    public void TakeDamage(float amount)
    {
        currentHealth = Mathf.Max(0f, currentHealth - amount);
    }

    public bool IsNearBurningCampfire()
    {
        foreach (Campfire fire in Campfire.activeCampfires)
        {
            if (fire.IsBurning())
            {
                float dist = Vector2.Distance(transform.position, fire.transform.position);
                if (dist <= warmAreaRadius) return true;
            }
        }
        return false;
    }

    public bool IsNight()
    {
        if (TimeManager.instance == null) return false;
        float hour = TimeManager.instance.currentTimeOfDay;
        return (hour >= 20f || hour < 6f);
    }
}
