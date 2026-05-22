using UnityEngine;

public class PlacementSystem : MonoBehaviour
{
    [Header("Настройки размещения")]
    public float placementRadius = 2.5f;

    void Update()
    {
        // Клик правой кнопкой мыши для установки
        if (Input.GetMouseButtonDown(1))
        {
            TryPlaceItem();
        }
    }

    private void TryPlaceItem()
    {
        // 1. Получаем выбранный предмет из хотбара через наш новый метод
        ItemData selectedItem = InventoryManager.instance.GetActiveItem();

        if (selectedItem == null) return;

        // 2. Проверяем галочку isPlaceable
        if (selectedItem.isPlaceable && selectedItem.placeablePrefab != null)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            if (Vector2.Distance(transform.position, mousePos) <= placementRadius)
            {
                // Устанавливаем префаб плуга/костра
                Instantiate(selectedItem.placeablePrefab, mousePos, Quaternion.identity);

                // 3. Тратим предмет
                InventoryManager.instance.RemoveActiveItem();
            }
            else
            {
                Debug.Log("Слишком далеко от игрока!");
            }
        }
    }
}