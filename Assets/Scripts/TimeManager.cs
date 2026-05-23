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

    [Header("Storm Tint Settings")]
    [Tooltip("Цвет затенения во время тропического ливня (грозовые тучи)")]
    public Color stormTint = new Color(0.11f, 0.11f, 0.15f, 0.45f);
    [Tooltip("Скорость плавного затенения при дожде (в секундах)")]
    public float weatherFadeDuration = 5f;

    private int currentDay = 1;
    private float rainTintIntensity = 0f;

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

        // --- Управление плавным затемнением от дождя ---
        bool isRaining = (WeatherManager.instance != null && WeatherManager.instance.IsRaining());
        float targetIntensity = isRaining ? 1f : 0f;
        rainTintIntensity = Mathf.MoveTowards(rainTintIntensity, targetIntensity, Time.deltaTime / weatherFadeDuration);

        // Обновляем визуальный оттенок на экране
        UpdateOverlayColor();
    }

    private void UpdateOverlayColor()
    {
        if (screenOverlay != null && timeColorGradient != null)
        {
            float normalizedTime = currentTimeOfDay / 24f;
            Color baseDayNightColor = timeColorGradient.Evaluate(normalizedTime);
            Color finalColor = baseDayNightColor;

            // Если идет дождь, плавно смешиваем дневной/ночной оттенок с темным грозовым
            if (rainTintIntensity > 0f)
            {
                // Грозовой цвет должен быть достаточно темным, но подстраиваться под ночь
                float targetAlpha = Mathf.Max(baseDayNightColor.a, stormTint.a);
                Color activeStormColor = new Color(stormTint.r, stormTint.g, stormTint.b, targetAlpha);

                // Плавно смешиваем базовый цвет суток и цвет грозы (на максимум 75% интенсивности, чтобы день/ночь считывались)
                finalColor = Color.Lerp(baseDayNightColor, activeStormColor, rainTintIntensity * 0.75f);
            }

            screenOverlay.color = finalColor;
        }
    }

    // Метод, гасящий все активные костры в мире
    public void ExtinguishAllCampfires()
    {
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
