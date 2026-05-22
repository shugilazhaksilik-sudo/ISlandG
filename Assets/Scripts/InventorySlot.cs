using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    public ItemData item;
    public int amount;
    public Image iconField;

    public Image amountImage;
    public Sprite[] numberSprites;

    public Image highlightFrame;
    public InventorySlot linkedHotbarSlot;

    // --- ÍÀÑÒĞÎÉÊÈ ÒÀÉÌÅĞÀ È ÏĞÎ×ÍÎÑÒÈ ---
    [Header("UI Òàéìåğà / Ïğî÷íîñòè")]
    public Image timerBar;
    public float currentLifetime;
    public int currentDurability; // <--- ÍÎÂÎÅ
    // -------------------------------

    private InventorySlot GetRealSlot()
    {
        if (this.transform.parent.name == "HotbarContent")
        {
            foreach (InventorySlot original in InventoryManager.instance.allSlots)
            {
                if (original.linkedHotbarSlot == this) return original;
            }
        }
        return this;
    }

    // --- ÎÁÍÎÂËÅÍÍÛÉ ÌÅÒÎÄ UPDATE ---
    void Update()
    {
        if (item != null)
        {
            // Åñëè ıòî ÈÑÊĞÀ (âğåìÿ)
            if (item.hasLifetime)
            {
                currentLifetime -= Time.deltaTime;

                if (timerBar != null)
                {
                    timerBar.gameObject.SetActive(true);
                    timerBar.color = Color.green; // Äåëàåì çåëåíûì
                    timerBar.fillAmount = currentLifetime / item.maxLifetime;
                }

                if (linkedHotbarSlot != null && linkedHotbarSlot.timerBar != null)
                {
                    linkedHotbarSlot.timerBar.gameObject.SetActive(true);
                    linkedHotbarSlot.timerBar.color = Color.green;
                    linkedHotbarSlot.timerBar.fillAmount = timerBar.fillAmount;
                }

                if (currentLifetime <= 0) ClearSlot();
            }
            // Åñëè ıòî ÈÍÑÒĞÓÌÅÍÒ (òîïîğ)
            else if (item.isTool)
            {
                if (timerBar != null)
                {
                    timerBar.gameObject.SetActive(true);
                    timerBar.color = Color.red; // Äåëàåì êğàñíûì äëÿ èíñòğóìåíòà
                    timerBar.fillAmount = (float)currentDurability / item.maxDurability;
                }

                if (linkedHotbarSlot != null && linkedHotbarSlot.timerBar != null)
                {
                    linkedHotbarSlot.timerBar.gameObject.SetActive(true);
                    linkedHotbarSlot.timerBar.color = Color.red;
                    linkedHotbarSlot.timerBar.fillAmount = timerBar.fillAmount;
                }

                // Åñëè òîïîğ ñëîìàëñÿ â ğóêàõ
                if (currentDurability <= 0) ClearSlot();
            }
            else
            {
                // Îáû÷íûé ïğåäìåò (êîêîñ)
                if (timerBar != null) timerBar.gameObject.SetActive(false);
                if (linkedHotbarSlot != null && linkedHotbarSlot.timerBar != null) linkedHotbarSlot.timerBar.gameObject.SetActive(false);
            }
        }
        else
        {
            // Ïóñòàÿ ÿ÷åéêà
            if (timerBar != null) timerBar.gameObject.SetActive(false);
            if (linkedHotbarSlot != null && linkedHotbarSlot.timerBar != null) linkedHotbarSlot.timerBar.gameObject.SetActive(false);
        }
    }

    // ÄÎÁÀÂËÅÍ ×ÅÒÂÅĞÒÛÉ ÏÀĞÀÌÅÒĞ durability
    public void AddItem(ItemData newItem, int count, float lifetime = -1f, int durability = -1)
    {
        item = newItem;
        amount = count;
        iconField.sprite = item.icon;
        iconField.enabled = true;

        if (item.hasLifetime)
        {
            currentLifetime = (lifetime < 0f) ? item.maxLifetime : lifetime;
        }

        // --- ÍÎÂÎÅ ÄËß ÒÎÏÎĞÀ ---
        if (item.isTool)
        {
            // Åñëè ïğî÷íîñòü -1 (íàïğèìåğ, ìû òîëüêî ÷òî ñêğàôòèëè íîâûé òîïîğ), äàåì ìàêñèìàëüíóş. 
            // Åñëè ìû ïîäíÿëè ñòàğûé òîïîğ (durability > 0), ñîõğàíÿåì òî ÷òî åñòü.
            currentDurability = (durability < 0) ? item.maxDurability : durability;
        }

        UpdateUI();
    }

    public void UpdateUI()
    {
        if (amount > 1)
        {
            amountImage.enabled = true;
            int spriteIndex = amount - 2;
            if (spriteIndex >= 0 && spriteIndex < numberSprites.Length)
            {
                amountImage.sprite = numberSprites[spriteIndex];
            }
        }
        else
        {
            amountImage.enabled = false;
        }

        if (amount <= 0) ClearSlot();

        if (linkedHotbarSlot != null)
        {
            linkedHotbarSlot.item = this.item;
            linkedHotbarSlot.amount = this.amount;
            linkedHotbarSlot.currentLifetime = this.currentLifetime;
            linkedHotbarSlot.currentDurability = this.currentDurability; // Ïåğåäàåì â õîòáàğ
            linkedHotbarSlot.iconField.sprite = this.iconField.sprite;
            linkedHotbarSlot.iconField.enabled = this.iconField.enabled;
            linkedHotbarSlot.amountImage.sprite = this.amountImage.sprite;
            linkedHotbarSlot.amountImage.enabled = this.amountImage.enabled;
        }
    }

    public void ClearSlot()
    {
        item = null;
        amount = 0;
        currentLifetime = 0f;
        currentDurability = 0; // Î÷èùàåì ïğî÷íîñòü
        iconField.sprite = null;
        iconField.enabled = false;

        amountImage.sprite = null;
        amountImage.enabled = false;

        if (timerBar != null) timerBar.gameObject.SetActive(false);

        if (linkedHotbarSlot != null)
        {
            linkedHotbarSlot.item = null;
            linkedHotbarSlot.amount = 0;
            linkedHotbarSlot.currentLifetime = 0f;
            linkedHotbarSlot.currentDurability = 0;
            linkedHotbarSlot.iconField.sprite = null;
            linkedHotbarSlot.iconField.enabled = false;
            linkedHotbarSlot.amountImage.sprite = null;
            linkedHotbarSlot.amountImage.enabled = false;
            if (linkedHotbarSlot.timerBar != null) linkedHotbarSlot.timerBar.gameObject.SetActive(false);
        }
    }

    public void SetHighlight(bool isSelected)
    {
        if (highlightFrame != null) highlightFrame.enabled = isSelected;
        if (linkedHotbarSlot != null) linkedHotbarSlot.SetHighlight(isSelected);
    }

    // --- ÌÀÃÈß ÌÛØÊÈ ---

    public void OnPointerClick(PointerEventData eventData)
    {
        InventorySlot realSlot = GetRealSlot();

        if (InventoryManager.instance.draggedItem != null)
        {
            realSlot.OnDrop(eventData);

            if (InventoryManager.instance.draggedItem == null)
            {
                InventoryManager.instance.dragIcon.enabled = false;
            }
            return;
        }

        InventoryManager.instance.SelectSlotByReference(realSlot);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        InventorySlot realSlot = GetRealSlot();
        if (realSlot != this)
        {
            realSlot.OnBeginDrag(eventData);
            return;
        }

        if (item == null) return;

        InventoryManager.instance.originalSlot = this;
        InventoryManager.instance.draggedItem = this.item;
        InventoryManager.instance.draggedLifetime = this.currentLifetime;
        InventoryManager.instance.draggedDurability = this.currentDurability; // ÇÀÏÎÌÈÍÀÅÌ ÏĞÎ×ÍÎÑÒÜ

        if (eventData.button == PointerEventData.InputButton.Right && this.amount > 1)
        {
            InventoryManager.instance.draggedAmount = 1;
            this.amount -= 1;
            this.UpdateUI();
        }
        else
        {
            InventoryManager.instance.draggedAmount = this.amount;
            this.ClearSlot();
        }

        InventoryManager.instance.dragIcon.sprite = InventoryManager.instance.draggedItem.icon;
        InventoryManager.instance.dragIcon.enabled = true;

        Color c = iconField.color; c.a = 0.5f; iconField.color = c;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (InventoryManager.instance.draggedItem != null)
        {
            InventoryManager.instance.dragIcon.transform.position = Input.mousePosition;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        InventorySlot realSlot = GetRealSlot();
        if (realSlot != this)
        {
            realSlot.OnEndDrag(eventData);
            return;
        }

        InventoryManager.instance.dragIcon.enabled = false;
        Color c = iconField.color; c.a = 1f; iconField.color = c;

        if (eventData.pointerEnter == null || eventData.pointerEnter.GetComponent<InventorySlot>() == null)
        {
            InventoryManager.instance.DropDraggedItemsToGround();
        }
        else if (InventoryManager.instance.draggedItem != null && InventoryManager.instance.originalSlot != null)
        {
            // ÂÎÇÂĞÀÙÀÅÌ Â ß×ÅÉÊÓ
            InventoryManager.instance.originalSlot.AddItem(InventoryManager.instance.draggedItem, InventoryManager.instance.draggedAmount, InventoryManager.instance.draggedLifetime, InventoryManager.instance.draggedDurability);
        }

        InventoryManager.instance.draggedItem = null;
        InventoryManager.instance.draggedAmount = 0;
        InventoryManager.instance.draggedLifetime = 0f;
        InventoryManager.instance.draggedDurability = -1; // Î÷èùàåì ìûøêó
        InventoryManager.instance.originalSlot = null;
    }

    public void OnDrop(PointerEventData eventData)
    {
        InventorySlot realSlot = GetRealSlot();
        if (realSlot != this)
        {
            realSlot.OnDrop(eventData);
            return;
        }

        ItemData mouseItem = InventoryManager.instance.draggedItem;
        int mouseAmount = InventoryManager.instance.draggedAmount;
        float mouseLifetime = InventoryManager.instance.draggedLifetime;
        int mouseDurability = InventoryManager.instance.draggedDurability; // ÁÅĞÅÌ Ñ ÌÛØÊÈ

        if (mouseItem == null) return;

        if (this.item == null)
        {
            // ÊËÀÄÅÌ ÏĞÅÄÌÅÒ Ñ ÑÎÕĞÀÍÅÍÈÅÌ ÏĞÎ×ÍÎÑÒÈ
            this.AddItem(mouseItem, mouseAmount, mouseLifetime, mouseDurability);
            InventoryManager.instance.draggedItem = null;
        }
        else if (this.item == mouseItem)
        {
            // ÇÀÙÈÒÀ: Èíñòğóìåíòû (isTool) íå ñòàêàşòñÿ
            if (!mouseItem.hasLifetime && !mouseItem.isTool)
            {
                int spaceLeft = 5 - this.amount;
                if (spaceLeft > 0)
                {
                    int amountToAdd = Mathf.Min(spaceLeft, mouseAmount);
                    this.amount += amountToAdd;
                    this.UpdateUI();

                    InventoryManager.instance.draggedAmount -= amountToAdd;

                    if (InventoryManager.instance.draggedAmount <= 0)
                    {
                        InventoryManager.instance.draggedItem = null;
                    }
                }
            }
        }
        else
        {
            if (InventoryManager.instance.originalSlot == null) return;
            if (InventoryManager.instance.originalSlot.amount > 0) return;

            // ÌÅÍßÅÌ ÏĞÅÄÌÅÒÛ ÌÅÑÒÀÌÈ
            ItemData tempItem = this.item;
            int tempAmount = this.amount;
            float tempLifetime = this.currentLifetime;
            int tempDurability = this.currentDurability;

            this.AddItem(mouseItem, mouseAmount, mouseLifetime, mouseDurability);

            InventoryManager.instance.draggedItem = tempItem;
            InventoryManager.instance.draggedAmount = tempAmount;
            InventoryManager.instance.draggedLifetime = tempLifetime;
            InventoryManager.instance.draggedDurability = tempDurability;
        }
    }
}