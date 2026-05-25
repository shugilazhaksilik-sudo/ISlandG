using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;

    [Header("UI Windows")]
    public GameObject mainWindow;
    public GameObject inventoryPage;
    public GameObject craftingPage;
    [Tooltip("Панель быстрого доступа (Hotbar)")]
    public GameObject hotbarPanel;

    public InventorySlot[] allSlots;
    public int selectedSlotIndex = 0;

    [Header("Drag and Drop Settings")]
    public Image dragIcon;
    [HideInInspector] public ItemData draggedItem;
    [HideInInspector] public int draggedAmount;
    [HideInInspector] public InventorySlot originalSlot;

    [HideInInspector] public float draggedLifetime;
    // --- НОВАЯ ПАМЯТЬ МЫШКИ ---
    [HideInInspector] public int draggedDurability = -1;

    public void OpenInventoryTab()
    {
        if (inventoryPage != null) inventoryPage.SetActive(true);
        if (craftingPage != null) craftingPage.SetActive(false);
    }

    public void OpenCraftingTab()
    {
        if (inventoryPage != null) inventoryPage.SetActive(false);
        if (craftingPage != null) craftingPage.SetActive(true);
    }

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        ToggleInventory(false);
        ChangeSelectedSlot(0);

        // Принудительно выбираем самый первый слот при старте игры
        selectedSlotIndex = 0;
    }

    public void ToggleInventory(bool active)
    {
        if (mainWindow != null) mainWindow.SetActive(active);

        // 1. Скрываем/показываем хотбар
        if (hotbarPanel != null) hotbarPanel.SetActive(!active);

        // 2. Останавливаем/возобновляем время во всем мире (физика, таймеры, анимации игрового мира)
        Time.timeScale = active ? 0f : 1f;

        // 3. Останавливаем/возобновляем все звуки в игре
        AudioListener.pause = active;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (mainWindow != null)
            {
                ToggleInventory(!mainWindow.activeSelf);
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (mainWindow != null && mainWindow.activeSelf)
            {
                ToggleInventory(false);
            }
        }

        if (Input.GetKeyDown(KeyCode.Q)) DropSingleItem(allSlots[selectedSlotIndex]);

        if (Input.GetKeyDown(KeyCode.Alpha1)) ChangeSelectedSlot(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) ChangeSelectedSlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) ChangeSelectedSlot(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) ChangeSelectedSlot(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) ChangeSelectedSlot(4);
        if (Input.GetKeyDown(KeyCode.Alpha6)) ChangeSelectedSlot(5);
        if (Input.GetKeyDown(KeyCode.Alpha7)) ChangeSelectedSlot(6);
        if (Input.GetKeyDown(KeyCode.Alpha8)) ChangeSelectedSlot(7);
        if (Input.GetKeyDown(KeyCode.Alpha9)) ChangeSelectedSlot(8);

        if (draggedItem != null)
        {
            dragIcon.transform.position = Input.mousePosition;

            if (Input.GetMouseButtonDown(0))
            {
                if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                {
                    DropDraggedItemsToGround();
                    dragIcon.enabled = false;
                }
            }
        }
    }

    // --- ОБНОВЛЕНО ДЛЯ ПРОЧНОСТИ ---
    public bool TryAddItem(ItemData newItem, int durability = -1)
    {
        if (newItem == null) return false;

        foreach (InventorySlot slot in allSlots)
        {
            // ЗАЩИТА: Инструменты (!newItem.isTool) больше не складываются в одну кучу!
            if (slot.item == newItem && slot.amount < 5 && !newItem.hasLifetime && !newItem.isTool)
            {
                slot.amount++;
                slot.UpdateUI();
                return true;
            }
        }

        foreach (InventorySlot slot in allSlots)
        {
            if (slot.item == null)
            {
                // Передаем прочность в ячейку
                slot.AddItem(newItem, 1, -1f, durability);
                return true;
            }
        }

        return false;
    }

    public void ChangeSelectedSlot(int newValue)
    {
        if (selectedSlotIndex >= 0 && selectedSlotIndex < allSlots.Length)
        {
            allSlots[selectedSlotIndex].SetHighlight(false);
        }

        selectedSlotIndex = newValue;
        allSlots[selectedSlotIndex].SetHighlight(true);
    }

    public void SelectSlotByReference(InventorySlot slot)
    {
        for (int i = 0; i < allSlots.Length; i++)
        {
            if (allSlots[i] == slot || (allSlots[i].linkedHotbarSlot != null && allSlots[i].linkedHotbarSlot == slot))
            {
                ChangeSelectedSlot(i);
                return;
            }
        }
    }

    // --- ОБНОВЛЕНЫ МЕТОДЫ ВЫБРОСА ПРЕДМЕТОВ ---
    public void DropSingleItem(InventorySlot slot)
    {
        if (slot == null || slot.item == null || slot.item.dropPrefab == null) return;
        if (!slot.item.canBeDropped) return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        GameObject dropped = SpawnPhysicalItem(slot.item.dropPrefab, player.transform.position);
        Item itemScript = dropped.GetComponent<Item>();
        if (itemScript != null) itemScript.currentDurability = slot.currentDurability;

        slot.amount--;

        if (slot.amount <= 0) slot.ClearSlot();
        else slot.UpdateUI();
    }

    public void DropFullStack(InventorySlot slot)
    {
        if (slot == null || slot.item == null || slot.item.dropPrefab == null) return;
        if (!slot.item.canBeDropped) return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        for (int i = 0; i < slot.amount; i++)
        {
            GameObject dropped = SpawnPhysicalItem(slot.item.dropPrefab, player.transform.position);
            Item itemScript = dropped.GetComponent<Item>();
            if (itemScript != null) itemScript.currentDurability = slot.currentDurability;
        }

        slot.ClearSlot();
    }

    public void DropDraggedItemsToGround()
    {
        if (draggedItem == null || draggedItem.dropPrefab == null) return;
        if (!draggedItem.canBeDropped) return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        for (int i = 0; i < draggedAmount; i++)
        {
            GameObject dropped = SpawnPhysicalItem(draggedItem.dropPrefab, player.transform.position);
            Item itemScript = dropped.GetComponent<Item>();
            if (itemScript != null) itemScript.currentDurability = draggedDurability;
        }

        draggedItem = null;
        draggedAmount = 0;
        draggedDurability = -1;
    }

    // Теперь метод возвращает созданный объект, чтобы мы могли записать в него прочность
    private GameObject SpawnPhysicalItem(GameObject prefab, Vector2 playerPos)
    {
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        Vector2 spawnPos = playerPos + (randomDirection * 1f);
        return Instantiate(prefab, spawnPos, Quaternion.identity);
    }

    // --- МЕТОДЫ ДЛЯ КРАФТА ---
    public int GetItemCount(ItemData itemToCheck)
    {
        int count = 0;
        foreach (InventorySlot slot in allSlots)
        {
            if (slot.item == itemToCheck) count += slot.amount;
        }
        return count;
    }

    public void ConsumeItems(ItemData itemToRemove, int amountToRemove)
    {
        int remainingToRemove = amountToRemove;

        foreach (InventorySlot slot in allSlots)
        {
            if (slot.item == itemToRemove)
            {
                if (slot.amount >= remainingToRemove)
                {
                    slot.amount -= remainingToRemove;
                    remainingToRemove = 0;

                    if (slot.amount == 0) slot.ClearSlot();
                    else slot.UpdateUI();

                    break;
                }
                else
                {
                    remainingToRemove -= slot.amount;
                    slot.ClearSlot();
                }
            }
        }
    }

    // --- МЕТОДЫ ДЛЯ СТРОИТЕЛЬСТВА ---
    public ItemData GetActiveItem()
    {
        if (allSlots[selectedSlotIndex].item != null)
        {
            return allSlots[selectedSlotIndex].item;
        }
        return null;
    }

    public void RemoveActiveItem()
    {
        if (allSlots[selectedSlotIndex].amount > 0)
        {
            allSlots[selectedSlotIndex].amount--;

            if (allSlots[selectedSlotIndex].amount <= 0)
                allSlots[selectedSlotIndex].ClearSlot();
            else
                allSlots[selectedSlotIndex].UpdateUI();
        }
    }
    // --- ОБНОВЛЕННЫЙ МЕТОД ДЛЯ ПОЛОМКИ ИНСТРУМЕНТОВ ---
    public void DamageActiveTool()
    {
        // Проверяем текущий выбранный слот
        InventorySlot activeSlot = allSlots[selectedSlotIndex];

        if (activeSlot.item != null && activeSlot.item.isTool)
        {
            activeSlot.currentDurability--;
            Debug.Log("Прочность инструмента уменьшилась! Осталось: " + activeSlot.currentDurability);

            if (activeSlot.currentDurability <= 0)
            {
                Debug.Log("Инструмент полностью сломался!");
                activeSlot.item = null; // Удаляем предмет
                activeSlot.amount = 0;  // Сбрасываем количество
                activeSlot.UpdateUI();  // Иконка полностью исчезнет
            }
            else
            {
                activeSlot.UpdateUI();  // Обновляем полоску прочности
            }
        }
    }

}
