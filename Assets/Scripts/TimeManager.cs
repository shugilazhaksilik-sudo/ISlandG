using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeManager : MonoBehaviour
{
    public static TimeManager instance;

    [Header("Time Settings")]
    [Tooltip("Длина игровых суток в реальных секундах")]
    public float dayLengthInSeconds = 120f;
    [Tooltip("Текущее время суток (от 0 до 24 часов)")]
    [Range(0f, 24f)]
    public float currentTimeOfDay = 6f; // Начинаем игру в 6:00 утра

    [Header("Visual Overlay")]
    [Tooltip("Изображение на весь экран (Canvas Image) для затемнения/оттенков времени суток")]
    public Image screenOverlay;
    [Tooltip("Градиент цвета суток (Утро, День, Вечер, Ночь)")]
    public Gradient timeColorGradient;

    private int currentDay = 1;

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        float prevTime = currentTimeOfDay;

        // Рассчитываем время
        currentTimeOfDay += (Time.deltaTime / dayLengthInSeconds) * 24f;

        if (currentTimeOfDay >= 24f)
        {
            currentTimeOfDay -= 24f;
            currentDay++;
            Debug.Log($"Наступил день {currentDay}!");
        }

        // Если время перешагнуло отметку 6:00 утра (начало утра/пробуждение игрока)
        if (prevTime < 6f && currentTimeOfDay >= 6f)
        {
            ExtinguishAllCampfires();
        }
        // Если время проскочило (например, при быстром перемотке времени с вечера на утро при сне)
        else if (prevTime > currentTimeOfDay && currentTimeOfDay >= 6f && prevTime < 24f)
        {
            ExtinguishAllCampfires();
        }

        // Обновляем визуальный оттенок на экране
        UpdateOverlayColor();
    }

    private void UpdateOverlayColor()
    {
        if (screenOverlay != null && timeColorGradient != null)
        {
            float normalizedTime = currentTimeOfDay / 24f;
            screenOverlay.color = timeColorGradient.Evaluate(normalizedTime);
        }
    }

    // Метод, гасящий все активные костры в мире
    public void ExtinguishAllCampfires()
    {
        // Создаем копию списка, чтобы избежать изменения коллекции во время перебора
        foreach (Campfire fire in new List<Campfire>(Campfire.activeCampfires))
        {
            if (fire.IsBurning())
            {
                fire.Extinguish();
            }
        }
        Debug.Log("Утро наступило! Все костры потухли.");
    }
}
