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

    private Dictionary<string, AudioClip> bgmMapping = new Dictionary<string, AudioClip>();
    private Coroutine fadeCoroutine;

    // Singleton instance for global SFX access
    private static CoreAudioManager Instance;

    private void Awake()
    {
        // Setup Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Populate dictionary for fast O(1) scene lookups
        foreach (var track in playlist)
        {
            if (!bgmMapping.ContainsKey(track.sceneName))
                bgmMapping.Add(track.sceneName, track.bgmClip);
        }
    }

    private void OnEnable()
    {
        // Listen for Unity scene loading hooks
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Listen for dynamic cross-scene volume UI changes
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
            // If the scene name wasn't found in your playlist list at all:
            Debug.LogWarning($"[Audio System] '{scene.name}' is not registered in the BGM Playlist Mapping!");
        }
    }

    private IEnumerator FadeToTrack(AudioClip newClip)
    {
        float startVolume = bgmSource.volume;

        // Fade Out current track
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

            // Fade In new track
            for (float t = 0; t < fadeDuration; t += Time.deltaTime)
            {
                bgmSource.volume = Mathf.Lerp(0f, startVolume, t / fadeDuration);
                yield return null;
            }
            bgmSource.volume = startVolume;
        }
    }

    private void HandleVolumeChange(string mixerParameter, float linearValue)
    {
        // Math conversion: Linear slider scale (0 to 1) converted to logarithmic Mixer scale (-80dB to 20dB)
        float dB = linearValue > 0.0001f ? Mathf.Log10(linearValue) * 20f : -80f;
        audioMixer.SetFloat(mixerParameter, dB);
    }

    /// <summary>
    /// Call this from any script in any scene to play a sound effect one-shot.
    /// Example: CoreAudioManager.PlaySFX(myClip);
    /// </summary>
    public static void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (Instance != null && Instance.sfxSource != null && clip != null)
        {
            Instance.sfxSource.PlayOneShot(clip, volumeScale);
        }
    }
}