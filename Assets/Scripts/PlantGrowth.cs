using UnityEngine;

public class PlantGrowth : MonoBehaviour
{
    [Header("Настройки графики")]
    public Sprite[] growthStages;
    private SpriteRenderer spriteRenderer;

    [Header("Настройки урожая")]
    public GameObject fruitPrefab;
    public Transform spawnPoint;   // Сюда мы перетащим наш пустой объект из кроны
    public int amount = 1;

    [Header("Настройки времени")]
    public float timeToGrow = 300f;
    private float timer;
    private int currentStage = 0;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        timer = timeToGrow;

        // Если забыла назначить SpawnPoint, код использует само дерево, чтобы не было ошибки
        if (spawnPoint == null) spawnPoint = transform;
    }

    void Update()
    {
        if (currentStage < growthStages.Length - 1)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                currentStage++;
                spriteRenderer.sprite = growthStages[currentStage];
                timer = timeToGrow;
            }
        }
    }

    public void CollectHarvest()
    {
        if (currentStage == growthStages.Length - 1)
        {
            for (int i = 0; i < amount; i++)
            {
                Vector3 scatter = new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(-0.3f, 0.3f), 0);
                // Теперь создаем в позиции spawnPoint, а не в центре дерева
                Instantiate(fruitPrefab, spawnPoint.position + scatter, Quaternion.identity);
            }

            currentStage = 0;
            spriteRenderer.sprite = growthStages[currentStage];
            timer = timeToGrow;

            Debug.Log("Урожай собран!");
        }
    }

    void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1))
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                float distance = Vector2.Distance(transform.position, player.transform.position);

                if (distance < 2f)
                {
                    CollectHarvest();
                }
                else
                {
                    Debug.Log("Лео слишком далеко!");
                }
            }
        }
    }
}