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
    [Tooltip("Длительность плавного перехода звука (нарастание / затухание) в секундах")]
    public float fadeDuration = 3f;
    [Tooltip("Максимальная громкость звука ливня")]
    [Range(0f, 1f)]
    public float maxVolume = 0.5f;

    private bool isRaining = false;
    private float weatherTimer;
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        // Устанавливаем громкость в 0 и выключаем в начале игры
        if (rainAudioSource != null)
        {
            rainAudioSource.volume = 0f;
            rainAudioSource.Stop();
        }
        StopRain();
        weatherTimer = weatherCheckInterval;
    }

    private void Update()
    {
        weatherTimer -= Time.deltaTime;
        if (weatherTimer <= 0f)
        {
            weatherTimer = weatherCheckInterval;
            TryChangeWeather();
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

    // Включение дождя с плавным нарастанием звука
    public void StartRain()
    {
        isRaining = true;
        if (rainParticles != null)
        {
            rainParticles.Play();
        }

        if (rainAudioSource != null)
        {
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            fadeCoroutine = StartCoroutine(FadeAudio(rainAudioSource, maxVolume, fadeDuration));
        }

        Debug.Log("Начался сильный тропический ливень с плавным нарастанием звука!");
    }

    // Выключение дождя с плавным затуханием звука
    public void StopRain()
    {
        isRaining = false;
        if (rainParticles != null)
        {
            rainParticles.Stop();
        }

        if (rainAudioSource != null)
        {
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            fadeCoroutine = StartCoroutine(FadeAudioAndStop(rainAudioSource, 0f, fadeDuration));
        }

        Debug.Log("Дождь закончился с плавным затуханием звука.");
    }

    // Корутина для плавного нарастания громкости
    private IEnumerator FadeAudio(AudioSource source, float targetVolume, float duration)
    {
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
    private IEnumerator FadeAudioAndStop(AudioSource source, float targetVolume, float duration)
    {
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
}
