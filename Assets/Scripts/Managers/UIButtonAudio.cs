using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Required for Hover detection

[RequireComponent(typeof(Button))]
public class UIButtonAudio : MonoBehaviour, IPointerEnterHandler
{
    [Header("Audio Clips")]
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip hoverSound; // Optional

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        button.onClick.AddListener(PlayClick);
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener(PlayClick);
    }

    private void PlayClick()
    {
        if (clickSound != null) CoreAudioManager.PlaySFX(clickSound);
    }

    // Automatically runs when the mouse hovers over the button
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button.interactable && hoverSound != null)
        {
            CoreAudioManager.PlaySFX(hoverSound, 0.6f); // Slightly quieter hover
        }
    }
}