using UnityEngine;

public class ResourceNode : MonoBehaviour
{
    [Header("Настройки добычи")]
    [Tooltip("Сколько ударов нужно для разрушения этого объекта")]
    public int hitsToDestroy = 3;
    [Tooltip("Задержка между ударами (в секундах)")]
    public float hitCooldown = 0.4f;
    [Tooltip("Список инструментов, которыми можно это сломать (например: Топор для дерева, Кирка для камня)")]
    public ItemData[] allowedTools;

    [Header("1. ОСНОВНОЙ ДРОП (Гарантированный)")]
    [Tooltip("Префаб предмета, который выпадает всегда (Дерево, Камень, Волокно травы)")]
    public GameObject mainDropPrefab;
    public int minMainAmount = 2;
    public int maxMainAmount = 4;

    [Header("2. ДОПОЛНИТЕЛЬНЫЙ ДРОП (Шансовый)")]
    [Tooltip("Префаб редкого/бонусного предмета (Палки, Кремний, Семена)")]
    public GameObject bonusDropPrefab;
    [Range(0, 100)]
    [Tooltip("Шанс выпадения бонусного предмета в процентах (0-100)")]
    public int bonusDropChance = 30;
    public int minBonusAmount = 1;
    public int maxBonusAmount = 2;

    [Header("Настройки аудио")]
    [Tooltip("Звуки разрушения (один будет выбран случайно при уничтожении)")]
    public AudioClip[] destroySounds;
    [Tooltip("Звуки при каждом обычном ударе (один будет выбран случайно)")]
    public AudioClip[] hitSounds;

    private int currentHits;
    private bool isPlayerNear = false;
    private float lastHitTime = 0f;

    void Start()
    {
        currentHits = hitsToDestroy;
    }

    void Update()
    {
        if (isPlayerNear && Input.GetMouseButtonDown(0))
        {
            if (Time.time >= lastHitTime + hitCooldown)
            {
                lastHitTime = Time.time;
                TryHarvest();
            }
        }
    }

    private void TryHarvest()
    {
        ItemData activeItem = InventoryManager.instance.GetActiveItem();

        // 1. Проверяем, держит ли Лео хоть какой-то инструмент
        if (activeItem == null || !activeItem.isTool)
        {
            Debug.Log("Это не добыть голыми руками! Нужен инструмент.");
            return;
        }

        // 2. Проверяем, подходит ли этот инструмент конкретно для этого ресурса
        bool isCorrectTool = false;
        foreach (ItemData tool in allowedTools)
        {
            if (activeItem == tool)
            {
                isCorrectTool = true;
                break;
            }
        }

        if (!isCorrectTool)
        {
            Debug.Log("Этот инструмент не подходит для этого ресурса!");
            return;
        }

        // 3. Удар прошел успешно!
        currentHits--;
        InventoryManager.instance.DamageActiveTool();

        if (currentHits <= 0)
        {
            BreakNode();
        }
        else
        {
            // Воспроизводим звук удара (если объект еще не сломан окончательно)
            if (hitSounds != null && hitSounds.Length > 0)
            {
                AudioClip clip = hitSounds[Random.Range(0, hitSounds.Length)];
                if (clip != null)
                {
                    AudioManager.PlaySFX(clip, transform.position);
                }
            }
        }
    }

    private void BreakNode()
    {
        // Воспроизводим случайный звук разрушения, если они заданы
        if (destroySounds != null && destroySounds.Length > 0)
        {
            AudioClip clip = destroySounds[Random.Range(0, destroySounds.Length)];
            if (clip != null)
            {
                AudioManager.PlaySFX(clip, transform.position);
            }
        }

        // --- 1. Спавн гарантированного основного дропа ---
        int mainCount = Random.Range(minMainAmount, maxMainAmount + 1);

        for (int i = 0; i < mainCount; i++)
        {
            SpawnItem(mainDropPrefab);
        }

        // --- 2. Спавн бонусного дропа по шансу ---
        int randomChance = Random.Range(1, 101);

        if (randomChance <= bonusDropChance)
        {
            int bonusCount = Random.Range(minBonusAmount, maxBonusAmount + 1);

            for (int i = 0; i < bonusCount; i++)
            {
                SpawnItem(bonusDropPrefab);
            }
        }

        // 3. Уничтожаем сам объект ресурса
        Destroy(gameObject);
    }

    // Вспомогательный метод для спавна с разлетом, чтобы не дублировать код
    private void SpawnItem(GameObject prefab)
    {
        if (prefab != null)
        {
            // Небольшое случайное смещение при появлении
            Vector3 spawnPosition = transform.position + new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(0.1f, 0.2f), 0f);
            GameObject droppedItem = Instantiate(prefab, spawnPosition, Quaternion.identity);

            // Запуск плавного разлета SmoothDrop
            if (droppedItem.TryGetComponent<SmoothDrop>(out var smoothDrop))
            {
                smoothDrop.SetupDrop();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) isPlayerNear = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) isPlayerNear = false;
    }
}
