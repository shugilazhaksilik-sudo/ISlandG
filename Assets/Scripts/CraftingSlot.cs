using UnityEngine;
using UnityEngine.UI;

public class CraftingSlot : MonoBehaviour
{
    public CraftingRecipe recipe;
    public Image recipeIcon;
    public Button craftButton;

    [Header("Отображение количества")]
    public Image amountImage;      // Сюда перетащи объект AmountImage
    public Sprite[] numberSprites; // Сюда перетащи те же спрайты цифр, что в инвентаре

    void Start()
    {
        if (recipe != null)
        {
            recipeIcon.sprite = recipe.resultItem.icon;
            UpdateAmountUI();
        }
    }

    // Логика отображения твоих спрайтов-цифр
    void UpdateAmountUI()
    {
        if (amountImage == null || numberSprites == null) return;

        if (recipe.resultAmount > 1)
        {
            amountImage.enabled = true;
            int spriteIndex = recipe.resultAmount - 2; // Логика: 2 -> индекс 0, 3 -> индекс 1 и т.д.

            if (spriteIndex >= 0 && spriteIndex < numberSprites.Length)
            {
                amountImage.sprite = numberSprites[spriteIndex];
            }
        }
        else
        {
            amountImage.enabled = false; // Если 1 штука, цифру не показываем
        }
    }

    void Update()
    {
        UpdateColorAndState();
    }

    public void UpdateColorAndState()
    {
        if (recipe == null) return;

        bool canCraft = CanCraft();

        // Определяем цвет: белый (цветной), если можем крафтить, и серый, если нет
        Color targetColor = canCraft ? Color.white : new Color(0.4f, 0.4f, 0.4f, 1f);

        // Красим иконку предмета
        recipeIcon.color = targetColor;

        // КРАСИМ ЦИФРУ (если она есть)
        if (amountImage != null)
        {
            amountImage.color = targetColor;
        }

        craftButton.interactable = canCraft;
    }

    public bool CanCraft()
    {
        foreach (CraftingIngredient ingredient in recipe.ingredients)
        {
            if (InventoryManager.instance.GetItemCount(ingredient.item) < ingredient.amount)
            {
                return false;
            }
        }
        return true;
    }

    // Крафт "в руку" к курсору
    public void OnCraftButtonClicked()
    {
        if (InventoryManager.instance.draggedItem != null) return;

        if (CanCraft())
        {
            foreach (CraftingIngredient ingredient in recipe.ingredients)
            {
                InventoryManager.instance.ConsumeItems(ingredient.item, ingredient.amount);
            }

            // Назначаем данные
            InventoryManager.instance.draggedItem = recipe.resultItem;
            InventoryManager.instance.draggedAmount = recipe.resultAmount;

            // Настраиваем саму иконку на курсоре
            InventoryManager.instance.dragIcon.sprite = recipe.resultItem.icon;

            // Включаем её ГАРАНТИРОВАННО
            InventoryManager.instance.dragIcon.enabled = true;

            // Сразу перемещаем в позицию мыши, чтобы не было задержки
            InventoryManager.instance.dragIcon.transform.position = Input.mousePosition;

            InventoryManager.instance.originalSlot = null;
        }
    }
}