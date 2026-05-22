using UnityEngine;
using System.Collections;

public class SmoothDrop : MonoBehaviour
{
    [Header("Настройки падения")]
    public float dropDuration = 0.5f;   // Сколько секунд длится разлет (0.5 - золотая середина)
    public float scatterRadius = 0.5f;  // Радиус разброса вокруг бревна

    // Внутренние переменные
    private Vector3 targetPosition;
    private Vector3 startPosition;

    // Оставляем пустым, чтобы префаб сам по себе ничего не запускал
    void Start()
    {
    }

    // Этот метод теперь вызывается из ResourceNode в момент Instantiate!
    public void SetupDrop()
    {
        // 1. Стартовая точка — это место, где предмет только что появился
        startPosition = transform.position;

        // 2. Рассчитываем случайное смещение для сочного разлета в стороны
        Vector2 randomOffset = Random.insideUnitCircle * scatterRadius;

        // Переносим точку на землю. Больше не вычитаем -1.5f, 
        // так как ResourceNode уже спавнит предмет на правильной высоте!
        targetPosition = startPosition + new Vector3(randomOffset.x, randomOffset.y, 0f);

        // 3. Перезапускаем корутину (на случай, если она уже шла)
        StopAllCoroutines();
        StartCoroutine(DropRoutine());
    }

    IEnumerator DropRoutine()
    {
        float elapsedTime = 0f;

        // Если на предмете есть коллайдер подбора, выключаем его на время полета,
        // чтобы игрок случайно не подобрал летящий топор/палку до приземления
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        while (elapsedTime < dropDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / dropDuration;

            // Добавим красивую дугу (прыжок вверх во время разлета)
            // Синус дает нам плавный подъем и спуск от 0 до 1 и обратно в 0
            float arc = Mathf.Sin(t * Mathf.PI) * 0.3f;

            // Лерпим основную позицию и накидываем высоту дуги по Y
            Vector3 currentPos = Vector3.Lerp(startPosition, targetPosition, t);
            transform.position = currentPos + new Vector3(0f, arc, 0f);

            yield return null;
        }

        // Гарантируем точную посадку в цель
        transform.position = targetPosition;

        // Включаем коллайдер обратно — теперь предмет можно подобрать!
        if (col != null) col.enabled = true;
    }
}