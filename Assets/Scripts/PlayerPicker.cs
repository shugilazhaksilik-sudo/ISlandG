using UnityEngine;

public class PlayerPicker : MonoBehaviour
{
    private InventoryManager inventory;

    void Start()
    {
        // Находим менеджер инвентаря на сцене
        inventory = FindObjectOfType<InventoryManager>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Если это размещенный костер, не подбираем его автоматически при ходьбе
        if (other.GetComponent<Campfire>() != null)
        {
            return;
        }

        // Проверяем, что у объекта есть компонент Item (выброшенный предмет)
        Item droppedItem = other.GetComponent<Item>();

        if (droppedItem != null)
        {
            // Проверяем логику высыхания трута/волокна, если это сушащаяся трава
            TinderDrying dryingLogic = other.GetComponent<TinderDrying>();

            if (dryingLogic != null)
            {
                // Если трава еще не высохла, не даем ее подобрать
                if (!dryingLogic.CanBePickedUp())
                {
                    return;
                }
            }

            // Пытаемся добавить предмет в инвентарь
            bool wasAdded = inventory.TryAddItem(droppedItem.itemData, droppedItem.currentDurability);

            if (wasAdded)
            {
                // Если успешно добавлено, уничтожаем физический объект со сцены
                Destroy(other.gameObject);
            }
        }
    }
}
