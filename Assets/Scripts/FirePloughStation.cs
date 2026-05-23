using UnityEngine;
using System.Collections;

public class FirePloughStation : MonoBehaviour
{
    [Header("Спрайты")]
    public Sprite emptyBoardSprite;   // Пустая доска
    public Sprite preparedSprite;     // Доска с положенной травой
    public Sprite sparkAppearsSprite; // Доска с искрой

    [Header("Настройки")]
    public ItemData dryFiberData;     // Сюда перетащи ScriptableObject СУХОЙ травы
    public ItemData sparkItemData;    // Сюда перетащи ScriptableObject искры
    [Range(0, 100)]
    public float sparkChance = 30f;   // Шанс появления искры (в процентах)

    private SpriteRenderer spriteRenderer;
    private bool isPlayerNear = false;
    private bool hasFiber = false;        // Положена ли трава в плуг?
    private bool isWaitingForSpark = false; // Ждем ли мы сейчас выдачи предмета?

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = emptyBoardSprite;
    }

    void Update()
    {
        if (!isPlayerNear || isWaitingForSpark) return;

        // 1. ПОДГОТОВКА (ПКМ с травой в руках)
        if (Input.GetMouseButtonDown(1) && !hasFiber)
        {
            ItemData activeItem = InventoryManager.instance.GetActiveItem();

            if (activeItem == dryFiberData)
            {
                PreparePlough();
            }
        }

        // 2. ПОПЫТКА ДОБЫТЬ ИСКРУ (ЛКМ по подготовленному плугу)
        if (Input.GetMouseButtonDown(0) && hasFiber)
        {
            TryMakeSpark();
        }
    }

    private void PreparePlough()
    {
        hasFiber = true;
        InventoryManager.instance.RemoveActiveItem(); // Тратим траву
        spriteRenderer.sprite = preparedSprite;      // Меняем вид
        Debug.Log("Трава заложена в плуг. Можно пробовать высечь искру!");
    }

    private void TryMakeSpark()
    {
        // Проверка шанса
        float roll = Random.Range(0f, 100f);

        if (roll <= sparkChance)
        {
            StartCoroutine(SparkSuccessSequence());
        }
        else
        {
            Debug.Log("Не вышло... Попробуй еще раз (ЛКМ)!");
        }
    }

    private IEnumerator SparkSuccessSequence()
    {
        isWaitingForSpark = true;
        spriteRenderer.sprite = sparkAppearsSprite; // Показываем искру
        Debug.Log("Есть искра! Ждем...");

        // Ждем 1.5 секунды
        yield return new WaitForSeconds(0.5f);

        // Пытаемся добавить искру ОДИН раз после ожидания
        if (InventoryManager.instance.TryAddItem(sparkItemData))
        {
            Debug.Log("Искра попала в инвентарь.");
        }
        else
        {
            Debug.Log("Инвентарь полон, искра погасла!");
        }

        ResetPlough();
    }

    private void ResetPlough()
    {
        hasFiber = false;
        isWaitingForSpark = false;
        spriteRenderer.sprite = emptyBoardSprite;
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