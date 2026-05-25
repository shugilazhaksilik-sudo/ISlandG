using System.Collections;
using UnityEngine;

public class WeatherManager : MonoBehaviour
{
    public static WeatherManager instance;

    [Header("Rain Components")]
    [Tooltip("Система частиц дождя")]
    public ParticleSystem rainParticles;
    [Tooltip("Аудио-источник для звука дождя")]
    public AudioSource rainAudioSource;

    [Header("Settings")]
    [Tooltip("Шанс начала дождя при каждой проверке погоды (%)")]
    [Range(0, 100)]
    public float rainChance = 30f;
    [Tooltip("Минимальная длительность дождя в секундах")]
    public float minRainDuration = 30f;
    [Tooltip("Максимальная длительность дождя в секундах")]
    public float maxRainDuration = 90f;
    [Tooltip("Интервал проверки смены погоды в секундах")]
    public float weatherCheckInterval = 60f;

    [Header("Fade Settings")]
    [Tooltip("Длительность плавного перехода звука и дождя (нарастание / затухание) в секундах")]
    public float fadeDuration = 5f;
    [Tooltip("Максимальная громкость звука ливня")]
    [Range(0f, 1f)]
    public float maxVolume = 0.5f;
    [Tooltip("Максимальное количество капель дождя в секунду")]
    public float maxEmissionRate = 450f;

    private bool isRaining = false;
    private float weatherTimer;
    
    private Coroutine audioFadeCoroutine;
    private Coroutine particleFadeCoroutine;

    [Header("Lightning & Thunder Settings")]
    [Tooltip("Аудио-источник для звука грома (если пусто, проиграется на источнике дождя)")]
    public AudioSource thunderAudioSource;
    [Tooltip("Звук грома")]
    public AudioClip thunderSound;
    [Tooltip("Шанс возникновения молнии в секунду во время дождя (%)")]
    [Range(0f, 100f)]
    public float lightningChancePerSecond = 5f;
    [Tooltip("Минимальная задержка между молниями в секундах")]
    public float minLightningCooldown = 10f;

    private float lightningCooldownTimer = 0f;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        // Инициализируем звук и частицы в выключенном состоянии
        if (rainAudioSource != null)
        {
            rainAudioSource.volume = 0f;
            rainAudioSource.Stop();
        }

        if (rainParticles != null)
        {
            var emission = rainParticles.emission;
            emission.rateOverTime = 0f;
            rainParticles.Stop();
        }

        isRaining = false;
        weatherTimer = weatherCheckInterval;
        lightningCooldownTimer = 0f;

        if (TimeManager.instance != null)
        {
            TimeManager.instance.lightningIntensity = 0f;
        }
    }

    private void Update()
    {
        weatherTimer -= Time.deltaTime;
        if (weatherTimer <= 0f)
        {
            weatherTimer = weatherCheckInterval;
            TryChangeWeather();
        }

        // Логика молний во время дождя
        if (isRaining)
        {
            if (lightningCooldownTimer > 0f)
            {
                lightningCooldownTimer -= Time.deltaTime;
            }
            else
            {
                // Каждую секунду проверяем шанс возникновения молнии
                float chance = lightningChancePerSecond * Time.deltaTime;
                if (Random.Range(0f, 100f) < chance)
                {
                    StartCoroutine(TriggerLightning());
                    lightningCooldownTimer = minLightningCooldown;
                }
            }
        }
        else
        {
            // Сбрасываем таймер кулдауна, если дождь закончился
            lightningCooldownTimer = 0f;
        }
    }

    private IEnumerator TriggerLightning()
    {
        // 1. Вспышка молнии (двойная вспышка для максимальной реалистичности!)
        if (TimeManager.instance != null)
        {
            // Первая вспышка (более мягкая, прозрачная)
            TimeManager.instance.lightningIntensity = 0.4f;
            yield return new WaitForSeconds(0.05f);
            TimeManager.instance.lightningIntensity = 0.05f;
            yield return new WaitForSeconds(0.05f);

            // Вторая вспышка (максимум до 0.5f для приятной прозрачности)
            TimeManager.instance.lightningIntensity = 0.5f;
            yield return new WaitForSeconds(0.15f);

            // Плавное угасание вспышки
            float elapsed = 0f;
            float flashFadeDuration = 0.4f;
            while (elapsed < flashFadeDuration)
            {
                elapsed += Time.deltaTime;
                TimeManager.instance.lightningIntensity = Mathf.Lerp(0.5f, 0f, elapsed / flashFadeDuration);
                yield return null;
            }
            TimeManager.instance.lightningIntensity = 0f;
        }

        // 2. Реалистичная задержка звука грома (сокращена в два раза для динамичности)
        float soundDelay = Random.Range(0.15f, 0.9f);
        yield return new WaitForSeconds(soundDelay);

        // 3. Воспроизведение звука грома
        if (thunderAudioSource != null && thunderSound != null)
        {
            thunderAudioSource.PlayOneShot(thunderSound);
        }
        else if (rainAudioSource != null && thunderSound != null)
        {
            // Если отдельного источника нет, проигрываем поверх дождя на его аудио-источнике
            rainAudioSource.PlayOneShot(thunderSound);
        }
    }

    // Проверка, идет ли сейчас дождь
    public bool IsRaining()
    {
        return isRaining;
    }

    private void TryChangeWeather()
    {
        if (isRaining) return; // Если дождь уже идет, повторно не запускаем

        float roll = Random.Range(0f, 100f);
        if (roll <= rainChance)
        {
            StartCoroutine(RainSequence());
        }
    }

    private IEnumerator RainSequence()
    {
        StartRain();
        float duration = Random.Range(minRainDuration, maxRainDuration);
        yield return new WaitForSeconds(duration);
        StopRain();
    }

    // Включение дождя с плавным нарастанием звука и капель
    public void StartRain()
    {
        isRaining = true;

        if (rainParticles != null)
        {
            rainParticles.Play();
            if (particleFadeCoroutine != null)
            {
                StopCoroutine(particleFadeCoroutine);
            }
            particleFadeCoroutine = StartCoroutine(FadeParticles(maxEmissionRate, fadeDuration));
        }

        if (rainAudioSource != null)
        {
            if (audioFadeCoroutine != null)
            {
                StopCoroutine(audioFadeCoroutine);
            }
            audioFadeCoroutine = StartCoroutine(FadeAudio(maxVolume, fadeDuration));
        }

        Debug.Log("Начался ливень с плавным нарастанием звука и капель!");
    }

    // Выключение дождя с плавным затуханием звука и капель
    public void StopRain()
    {
        isRaining = false;

        if (rainParticles != null)
        {
            if (particleFadeCoroutine != null)
            {
                StopCoroutine(particleFadeCoroutine);
            }
            particleFadeCoroutine = StartCoroutine(FadeParticlesAndStop(0f, fadeDuration));
        }

        if (rainAudioSource != null)
        {
            if (audioFadeCoroutine != null)
            {
                StopCoroutine(audioFadeCoroutine);
            }
            audioFadeCoroutine = StartCoroutine(FadeAudioAndStop(0f, fadeDuration));
        }

        Debug.Log("Ливень закончился с плавным затуханием звука и капель.");
    }

    // Корутина для плавного нарастания/затухания громкости
    private IEnumerator FadeAudio(float targetVolume, float duration)
    {
        AudioSource source = rainAudioSource;
        if (!source.isPlaying)
        {
            source.volume = 0f;
            source.Play();
        }

        float startVolume = source.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
            yield return null;
        }

        source.volume = targetVolume;
    }

    // Корутина для плавного затухания громкости и последующей остановки
    private IEnumerator FadeAudioAndStop(float targetVolume, float duration)
    {
        AudioSource source = rainAudioSource;
        float startVolume = source.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
            yield return null;
        }

        source.volume = targetVolume;
        source.Stop();
    }

    // Корутина для плавного изменения количества частиц
    private IEnumerator FadeParticles(float targetRate, float duration)
    {
        var emission = rainParticles.emission;
        float startRate = emission.rateOverTime.constant;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float currentRate = Mathf.Lerp(startRate, targetRate, elapsed / duration);
            emission.rateOverTime = currentRate;
            yield return null;
        }

        emission.rateOverTime = targetRate;
    }

    // Корутина для затухания частиц и последующей остановки системы
    private IEnumerator FadeParticlesAndStop(float targetRate, float duration)
    {
        var emission = rainParticles.emission;
        float startRate = emission.rateOverTime.constant;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float currentRate = Mathf.Lerp(startRate, targetRate, elapsed / duration);
            emission.rateOverTime = currentRate;
            yield return null;
        }

        emission.rateOverTime = targetRate;
        rainParticles.Stop();
    }
}
