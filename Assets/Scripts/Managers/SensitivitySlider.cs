using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class SensitivitySlider : MonoBehaviour
{
    private Slider slider;
    private const string SENS_KEY = "MouseSensitivity";

    private void Awake()
    {
        slider = GetComponent<Slider>();

        // We set the slider range from 0.2 (very slow) to 3.0 (very fast). 1.0 is default.
        slider.minValue = 0.2f;
        slider.maxValue = 3.0f;
    }

    private void Start()
    {
        // Load the saved sensitivity on launch, defaulting to 1.0f if it doesn't exist
        float savedSensitivity = PlayerPrefs.GetFloat(SENS_KEY, 1.0f);

        // Apply it globally to the character controller
        Charactercontroller.MouseSensitivityMultiplier = savedSensitivity;

        // Set the visual slider handle to match
        slider.value = savedSensitivity;
    }

    private void OnEnable()
    {
        slider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    private void OnDisable()
    {
        slider.onValueChanged.RemoveListener(OnSliderValueChanged);
    }

    private void OnSliderValueChanged(float value)
    {
        // 1. Instantly update the Character Controller's multiplier over-the-air
        Charactercontroller.MouseSensitivityMultiplier = value;

        // 2. Save it permanently so the setting persists
        PlayerPrefs.SetFloat(SENS_KEY, value);
        PlayerPrefs.Save();
    }
}