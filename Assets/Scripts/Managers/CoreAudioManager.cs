using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class CoreAudioManager : MonoBehaviour
{
    [System.Serializable]
    public struct SceneTrack
    {
        public string sceneName;
        public AudioClip bgmClip;
    }

    [Header("Audio Mixer & Sources")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("BGM Playlist Mapping")]
    [SerializeField] private List<SceneTrack> playlist = new List<SceneTrack>();
    [SerializeField] private float fadeDuration = 0.5f;

    [Header("Voiceover BGM Ducking Settings")]
    [Tooltip("Target volume multiplier for BGM when a voiceover is active (e.g., 0.3 means BGM drops to 30%).")]
    [Range(0f, 1f)]
    public float voiceoverDuckedBGMVolume = 0.3f;
    public float duckingFadeDuration = 0.2f;

    private Dictionary<string, AudioClip> bgmMapping = new Dictionary<string, AudioClip>();
    private Coroutine fadeCoroutine;
    private Coroutine duckCoroutine;

    private float originalBGMVolume = 1f;

    private static CoreAudioManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        foreach (var track in playlist)
        {
            if (!bgmMapping.ContainsKey(track.sceneName))
                bgmMapping.Add(track.sceneName, track.bgmClip);
        }

        if (bgmSource != null)
        {
            originalBGMVolume = bgmSource.volume;
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        VolumeSlider.OnVolumeChanged += HandleVolumeChange;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        VolumeSlider.OnVolumeChanged -= HandleVolumeChange;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (bgmMapping.TryGetValue(scene.name, out AudioClip newClip))
        {
            if (newClip == null) return;

            if (bgmSource.clip != newClip)
            {
                if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
                fadeCoroutine = StartCoroutine(FadeToTrack(newClip));
            }
        }
        else
        {
            Debug.LogWarning($"[Audio System] '{scene.name}' is not registered in the BGM Playlist Mapping!");
        }
    }

    private IEnumerator FadeToTrack(AudioClip newClip)
    {
        float startVolume = bgmSource.volume;

        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            bgmSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeDuration);
            yield return null;
        }
        bgmSource.volume = 0f;

        bgmSource.clip = newClip;

        if (newClip != null)
        {
            bgmSource.Play();

            for (float t = 0; t < fadeDuration; t += Time.deltaTime)
            {
                bgmSource.volume = Mathf.Lerp(0f, startVolume, t / fadeDuration);
                yield return null;
            }
            bgmSource.volume = startVolume;
        }

        originalBGMVolume = bgmSource.volume;
    }

    private void HandleVolumeChange(string mixerParameter, float linearValue)
    {
        float dB = linearValue > 0.0001f ? Mathf.Log10(linearValue) * 20f : -80f;
        audioMixer.SetFloat(mixerParameter, dB);
    }

    public static void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (Instance != null && Instance.sfxSource != null && clip != null)
        {
            Instance.sfxSource.pitch = 1.0f;

            if (clip.length > 5.0f)
            {
                PauseBGM();
            }

            Instance.sfxSource.PlayOneShot(clip, volumeScale);
        }
    }

    public static void StopSFX()
    {
        if (Instance != null && Instance.sfxSource != null)
        {
            Instance.sfxSource.Stop();
        }
    }

    public static void FadeOutSFX(float duration)
    {
        if (Instance != null && Instance.sfxSource != null)
        {
            Instance.StartCoroutine(Instance.FadeSFXVolume(Instance.sfxSource.volume, 0f, duration));
        }
    }

    private IEnumerator FadeSFXVolume(float startVol, float endVol, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            sfxSource.volume = Mathf.Lerp(startVol, endVol, elapsed / duration);
            yield return null;
        }
        sfxSource.volume = endVol;

        sfxSource.Stop();
        sfxSource.volume = startVol;

        ResumeBGM();
    }

    // ============================================================
    // DIALOGUE VOICEOVER FUNCTIONS (WITH DUCKING)
    // ============================================================

    public static void PlayVoiceover(AudioClip clip)
    {
        if (Instance != null && Instance.sfxSource != null && clip != null)
        {
            Instance.sfxSource.pitch = 1.0f;
            Instance.sfxSource.clip = clip;
            Instance.sfxSource.Play();

            // Duck the background music down
            Instance.DuckBGM(true);
        }
    }

    public static void StopVoiceover()
    {
        if (Instance != null && Instance.sfxSource != null)
        {
            Instance.sfxSource.Stop();

            // Restore the background music volume
            Instance.DuckBGM(false);
        }
    }

    private void DuckBGM(bool enable)
    {
        if (bgmSource == null) return;

        if (duckCoroutine != null) StopCoroutine(duckCoroutine);

        float targetVol = enable ? (originalBGMVolume * voiceoverDuckedBGMVolume) : originalBGMVolume;
        duckCoroutine = StartCoroutine(FadeBGMDucking(targetVol, duckingFadeDuration));
    }

    private IEnumerator FadeBGMDucking(float targetVol, float duration)
    {
        float startVol = bgmSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(startVol, targetVol, elapsed / duration);
            yield return null;
        }

        bgmSource.volume = targetVol;
    }

    // ============================================================
    // LOOPING AUDIO FUNCTIONS 
    // ============================================================

    public static void PlayLoopingSFX(AudioClip clip, float volumeScale = 1f)
    {
        if (Instance != null && Instance.bgmSource != null && clip != null)
        {
            if (Instance.fadeCoroutine != null) Instance.StopCoroutine(Instance.fadeCoroutine);

            Instance.bgmSource.clip = clip;
            Instance.bgmSource.loop = true;
            Instance.bgmSource.volume = volumeScale;
            Instance.bgmSource.Play();

            Instance.originalBGMVolume = volumeScale;
        }
    }

    public static void StopLoopingSFX()
    {
        if (Instance != null && Instance.bgmSource != null)
        {
            Instance.bgmSource.Stop();
            Instance.bgmSource.loop = false;
        }
    }

    public static void FadeOutLoopingSFX(float duration)
    {
        if (Instance != null && Instance.bgmSource != null)
        {
            if (Instance.fadeCoroutine != null) Instance.StopCoroutine(Instance.fadeCoroutine);
            Instance.fadeCoroutine = Instance.StartCoroutine(Instance.FadeBGMVolume(Instance.bgmSource.volume, 0f, duration));
        }
    }

    // ============================================================

    public static void PauseBGM()
    {
        if (Instance != null && Instance.bgmSource != null)
        {
            Instance.bgmSource.Pause();
        }
    }

    public static void ResumeBGM()
    {
        if (Instance != null && Instance.bgmSource != null)
        {
            Instance.bgmSource.UnPause();
        }
    }

    public static void FadeOutBGM(float duration)
    {
        if (Instance != null && Instance.bgmSource != null)
        {
            if (Instance.fadeCoroutine != null) Instance.StopCoroutine(Instance.fadeCoroutine);
            Instance.fadeCoroutine = Instance.StartCoroutine(Instance.FadeBGMVolume(Instance.bgmSource.volume, 0f, duration));
        }
    }

    public static void FadeInBGM(float targetVolume, float duration)
    {
        if (Instance != null && Instance.bgmSource != null)
        {
            if (Instance.fadeCoroutine != null) Instance.StopCoroutine(Instance.fadeCoroutine);
            Instance.fadeCoroutine = Instance.StartCoroutine(Instance.FadeBGMVolume(0f, targetVolume, duration));
        }
    }

    private IEnumerator FadeBGMVolume(float startVol, float endVol, float duration)
    {
        if (endVol > 0.001f && !bgmSource.isPlaying)
        {
            bgmSource.UnPause();
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(startVol, endVol, elapsed / duration);
            yield return null;
        }
        bgmSource.volume = endVol;

        if (endVol <= 0.001f)
        {
            bgmSource.Pause();
        }
    }
}