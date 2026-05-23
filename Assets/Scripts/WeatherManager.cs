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

    private bool isRaining = false;
    private float weatherTimer;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
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

    // Включение дождя
    public void StartRain()
    {
        isRaining = true;
        if (rainParticles != null)
        {
            rainParticles.Play();
        }
        if (rainAudioSource != null)
        {
            rainAudioSource.Play();
        }
        Debug.Log("Начался сильный тропический ливень!");
    }

    // Выключение дождя
    public void StopRain()
    {
        isRaining = false;
        if (rainParticles != null)
        {
            rainParticles.Stop();
        }
        if (rainAudioSource != null)
        {
            rainAudioSource.Stop();
        }
        Debug.Log("Дождь закончился.");
    }
}
