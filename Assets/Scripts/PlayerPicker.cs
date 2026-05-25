using UnityEngine;

public class PlayerPicker : MonoBehaviour
{
    public AudioClip pickUpSound;
    private InventoryManager inventory;

    void Start()
    {
        // Ищем менеджер инвентаря на сцене
        inventory = FindObjectOfType<InventoryManager>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Проверяем, есть ли на объекте скрипт Item
        Item droppedItem = other.GetComponent<Item>();

        if (droppedItem != null)
        {
            // --- НОВЫЙ КОД НАЧАЛО ---
            // Проверяем, является ли этот предмет травой, которая сушится
            TinderDrying dryingLogic = other.GetComponent<TinderDrying>();

            if (dryingLogic != null)
            {
                // Если скрипт сушки есть, но трава ЕЩЕ НЕ высохла (стадия < 2)
                if (!dryingLogic.CanBePickedUp())
                {
                    // Прерываем выполнение метода. Лео просто пройдет сквозь неё.
                    // Debug.Log("Трава еще сырая, Лео не может ее поднять!"); 
                    return;
                }
            }
            // --- НОВЫЙ КОД КОНЕЦ ---

            // Пытаемся добавить предмет в инвентарь И ПЕРЕДАЕМ ПРОЧНОСТЬ
            bool wasAdded = inventory.TryAddItem(droppedItem.itemData, droppedItem.currentDurability);

            if (wasAdded)
            {
                // Воспроизводим звук подбора
                if (pickUpSound != null)
                {
                    AudioSource.PlayClipAtPoint(pickUpSound, transform.position);
                }

                // Если место было и предмет добавился — удаляем его с земли
                Destroy(other.gameObject);
            }
        }
    }
}
