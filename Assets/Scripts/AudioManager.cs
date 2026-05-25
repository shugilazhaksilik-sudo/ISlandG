using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Audio Mixer Groups")]
    [Tooltip("Группа микшера для звуковых эффектов (SFX: удары, разрушения, подбор предметов)")]
    public AudioMixerGroup sfxGroup;
    
    [Tooltip("Группа микшера для окружения (Ambient: дождь, костер, море)")]
    public AudioMixerGroup ambientGroup;

    [Tooltip("Группа микшера для звуков игрока (Player: еда, шаги, урон)")]
    public AudioMixerGroup playerGroup;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Воспроизводит 3D-звук эффектов (SFX), корректно направляя его в соответствующую группу микшера.
    /// </summary>
    public static void PlaySFX(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (clip == null) return;

        AudioMixerGroup group = (instance != null) ? instance.sfxGroup : null;
        PlayClipAtPointWithMixer(clip, position, group, volume);
    }

    /// <summary>
    /// Воспроизводит 3D-звук игрока, корректно направляя его в соответствующую группу микшера.
    /// </summary>
    public static void PlayPlayerSFX(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (clip == null) return;

        AudioMixerGroup group = (instance != null) ? instance.playerGroup : null;
        PlayClipAtPointWithMixer(clip, position, group, volume);
    }

    /// <summary>
    /// Вспомогательный метод для создания временного объекта и воспроизведения звука через группу микшера.
    /// Это решает классический баг Unity, когда AudioSource.PlayClipAtPoint игнорирует Audio Mixer.
    /// </summary>
    private static void PlayClipAtPointWithMixer(AudioClip clip, Vector3 position, AudioMixerGroup group, float volume)
    {
        // Создаем временный GameObject для проигрывания звука
        GameObject tempGO = new GameObject("TempAudio_" + clip.name);
        tempGO.transform.position = position;

        // Добавляем и настраиваем компонент AudioSource
        AudioSource source = tempGO.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = volume;
        source.spatialBlend = 1f; // Полный 3D-звук с разлетом
        source.outputAudioMixerGroup = group; // Привязываем группу микшера!

        // Запускаем проигрывание
        source.Play();

        // Автоматически уничтожаем объект после того, как звук доиграет
        Destroy(tempGO, clip.length);
    }
}
