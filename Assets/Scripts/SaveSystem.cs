using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem instance;

    [Header("Database")]
    [Tooltip("Список всех ItemData предметов в игре для сопоставления при загрузке")]
    public List<ItemData> itemDatabase = new List<ItemData>();

    private string saveFilePath;

    private void Awake()
    {
        instance = this;
        // Путь к файлу сохранения в зависимости от ОС (для Windows: AppData/LocalLow/...)
        saveFilePath = Path.Combine(Application.persistentDataPath, "savegame.json");
    }

    private void Update()
    {
        // Возможность ручного сохранения на F5 и загрузки на F9
        if (Input.GetKeyDown(KeyCode.F5))
        {
            SaveGame();
        }
        if (Input.GetKeyDown(KeyCode.F9))
        {
            LoadGame();
        }
    }

    // Метод сохранения
    public void SaveGame()
    {
        GameSaveData saveData = new GameSaveData();

        // 1. Сохраняем время суток и день
        if (TimeManager.instance != null)
        {
            saveData.currentTimeOfDay = TimeManager.instance.currentTimeOfDay;
        }

        // 2. Сохраняем координаты игрока
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            saveData.playerX = player.transform.position.x;
            saveData.playerY = player.transform.position.y;
        }

        // 3. Сохраняем показатели выживания
        if (SurvivalSystem.instance != null)
        {
            saveData.health = SurvivalSystem.instance.currentHealth;
            saveData.hunger = SurvivalSystem.instance.currentHunger;
            saveData.thirst = SurvivalSystem.instance.currentThirst;
            saveData.cold = SurvivalSystem.instance.currentCold;
        }

        // 4. Сохраняем инвентарь (каждую ячейку)
        if (InventoryManager.instance != null)
        {
            foreach (InventorySlot slot in InventoryManager.instance.allSlots)
            {
                SlotSaveData slotData = new SlotSaveData();
                if (slot.item != null)
                {
                    slotData.itemName = slot.item.itemName; // Сохраняем по уникальному имени
                    slotData.amount = slot.amount;
                    slotData.currentLifetime = slot.currentLifetime;
                    slotData.currentDurability = slot.currentDurability;
                }
                else
                {
                    slotData.itemName = ""; // Слот пустой
                }
                saveData.inventorySlots.Add(slotData);
            }
        }

        // Преобразуем в JSON с красивым форматированием
        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(saveFilePath, json);
        Debug.Log("Игра успешно сохранена в: " + saveFilePath);
    }

    // Метод загрузки
    public void LoadGame()
    {
        if (!File.Exists(saveFilePath))
        {
            Debug.LogWarning("Файл сохранения не найден!");
            return;
        }

        // Читаем из JSON
        string json = File.ReadAllText(saveFilePath);
        GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(json);

        // 1. Загружаем время суток
        if (TimeManager.instance != null)
        {
            TimeManager.instance.currentTimeOfDay = saveData.currentTimeOfDay;
        }

        // 2. Загружаем координаты игрока
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = new Vector3(saveData.playerX, saveData.playerY, player.transform.position.z);
        }

        // 3. Загружаем показатели выживания
        if (SurvivalSystem.instance != null)
        {
            SurvivalSystem.instance.currentHealth = saveData.health;
            SurvivalSystem.instance.currentHunger = saveData.hunger;
            SurvivalSystem.instance.currentThirst = saveData.thirst;
            SurvivalSystem.instance.currentCold = saveData.cold;
        }

        // 4. Загружаем инвентарь
        if (InventoryManager.instance != null)
        {
            // Сначала полностью очищаем все слоты инвентаря
            for (int i = 0; i < InventoryManager.instance.allSlots.Length; i++)
            {
                InventoryManager.instance.allSlots[i].ClearSlot();
            }

            // Восстанавливаем сохраненные предметы
            int slotsCount = Mathf.Min(InventoryManager.instance.allSlots.Length, saveData.inventorySlots.Count);
            for (int i = 0; i < slotsCount; i++)
            {
                SlotSaveData slotData = saveData.inventorySlots[i];
                if (!string.IsNullOrEmpty(slotData.itemName))
                {
                    ItemData matchedItem = FindItemInDatabase(slotData.itemName);
                    if (matchedItem != null)
                    {
                        InventoryManager.instance.allSlots[i].AddItem(
                            matchedItem, 
                            slotData.amount, 
                            slotData.currentLifetime, 
                            slotData.currentDurability
                        );
                    }
                }
            }
        }

        Debug.Log("Игра успешно загружена!");
    }

    // Вспомогательный метод поиска ItemData в базе по имени предмета
    private ItemData FindItemInDatabase(string nameToFind)
    {
        foreach (ItemData item in itemDatabase)
        {
            if (item.itemName == nameToFind)
                return item;
        }
        return null;
    }
}

[System.Serializable]
public class SlotSaveData
{
    public string itemName;
    public int amount;
    public float currentLifetime;
    public int currentDurability;
}

[System.Serializable]
public class GameSaveData
{
    public int currentDay;
    public float currentTimeOfDay;
    public float playerX;
    public float playerY;
    public float health;
    public float hunger;
    public float thirst;
    public float cold;
    public List<SlotSaveData> inventorySlots = new List<SlotSaveData>();
}
