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
