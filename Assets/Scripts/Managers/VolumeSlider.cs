using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio; // Added for AudioMixer access

[RequireComponent(typeof(Slider))]
public class VolumeSlider : MonoBehaviour
{
    public enum VolumeType { MasterVolume, MusicVolume, SFXVolume }

    [Header("Configuration")]
    [SerializeField] private VolumeType targetChannel;
    [SerializeField] private AudioMixer audioMixer; // Drag your MainMixer here in the inspector

    public static System.Action<string, float> OnVolumeChanged;

    private Slider slider;

    private void Awake()
    {
        slider = GetComponent<Slider>();
        slider.minValue = 0.0001f;
        slider.maxValue = 1f;
    }

    private void Start()
    {
        // When the scene loads, check the actual mixer volume and position the slider correctly
        if (audioMixer != null)
        {
            if (audioMixer.GetFloat(targetChannel.ToString(), out float currentDecibels))
            {
                // Reverse the math: Convert Decibels back to Linear 0-1 for the slider
                // Formula: Linear = 10^(dB / 20)
                float linearValue = Mathf.Pow(10f, currentDecibels / 20f);

                // Temporarily remove listener so we don't trigger an accidental event broadcast while setting initial value
                slider.onValueChanged.RemoveListener(BroadcastVolumeChange);
                slider.value = linearValue;
                slider.onValueChanged.AddListener(BroadcastVolumeChange);
            }
        }
    }

    private void OnEnable()
    {
        slider.onValueChanged.AddListener(BroadcastVolumeChange);
    }

    private void OnDisable()
    {
        slider.onValueChanged.RemoveListener(BroadcastVolumeChange);
    }

    private void BroadcastVolumeChange(float value)
    {
        OnVolumeChanged?.Invoke(targetChannel.ToString(), value);
    }
}