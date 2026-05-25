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
    [Tooltip("Цвет затенения во время тропического ливня (полупрозрачный серый)")]
    public Color stormTint = new Color(0.15f, 0.15f, 0.18f, 0.45f);
    [Tooltip("Скорость плавного затенения при дожде (в секундах)")]
    public float weatherFadeDuration = 5f;

    private int currentDay = 1;
    private float rainTintIntensity = 0f;

    [HideInInspector]
    public float lightningIntensity = 0f; // Интенсивность вспышки молнии (0 - нет вспышки, 1 - полная вспышка)

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
            AutoSave();
        }
        // Если время проскочило (например, при быстром перемотке времени с вечера на утро при сне)
        else if (prevTime > currentTimeOfDay && currentTimeOfDay >= 6f && prevTime < 24f)
        {
            ExtinguishAllCampfires();
            AutoSave();
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

            // Если идет дождь, плавно переходим в полупрозрачный серый штормовой оттенок
            if (rainTintIntensity > 0f)
            {
                // Чтобы избежать эффекта выбеливания, мы принудительно берем темно-серый цвет в качестве
                // базового для перехода, чтобы плавно нарастала только его прозрачность (Альфа).
                finalColor.r = Mathf.Lerp(baseDayNightColor.r * 0.15f, stormTint.r, rainTintIntensity);
                finalColor.g = Mathf.Lerp(baseDayNightColor.g * 0.15f, stormTint.g, rainTintIntensity);
                finalColor.b = Mathf.Lerp(baseDayNightColor.b * 0.18f, stormTint.b, rainTintIntensity);

                // Альфа плавно нарастает до заданной прозрачности шторма, но сохраняет ночную темноту, если сейчас ночь
                float targetAlpha = Mathf.Max(baseDayNightColor.a, stormTint.a * rainTintIntensity);
                finalColor.a = targetAlpha;
            }
            // Вспышка молнии временно выбеливает экран
            if (lightningIntensity > 0f)
            {
                // Для того чтобы вспышка оставалась ярко-белой (а не грязной/серой), 
                // мы плавно переводим все цветовые каналы RGB к 1f (чисто белому цвету).
                finalColor.r = Mathf.Lerp(finalColor.r, 1f, lightningIntensity);
                finalColor.g = Mathf.Lerp(finalColor.g, 1f, lightningIntensity);
                finalColor.b = Mathf.Lerp(finalColor.b, 1f, lightningIntensity);

                // Ограничиваем максимальную плотность/альфу вспышки до 0.4f, 
                // чтобы она была мягкой, полупрозрачной и не слепила игрока сплошным белым цветом.
                finalColor.a = Mathf.Lerp(finalColor.a, 0.4f, lightningIntensity);
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

    // Автоматическое сохранение утром
    private void AutoSave()
    {
        if (SaveSystem.instance != null)
        {
            SaveSystem.instance.SaveGame();
            Debug.Log("Утреннее автосохранение успешно выполнено!");
        }
    }
}
