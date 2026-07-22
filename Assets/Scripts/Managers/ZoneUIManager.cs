using System.Collections;
using UnityEngine;
using TMPro;

public class ZoneUIManager : MonoBehaviour
{
    public static ZoneUIManager Instance;

    [Header("UI References")]
    [Tooltip("Drag your CanvasGroup component here (e.g. from your Panel).")]
    [SerializeField] private CanvasGroup canvasGroup;
    [Tooltip("Drag the TextMeshPro element inside your panel here.")]
    [SerializeField] private TMP_Text zoneText;

    [Header("Fade Settings")]
    [SerializeField] private float fadeInDuration = 1.0f;
    [SerializeField] private float displayDuration = 2.0f;
    [SerializeField] private float fadeOutDuration = 1.5f;

    private Coroutine activeFadeCoroutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Instantly hide the panel on start
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
    }

    public void TriggerZoneSplash(string zoneName)
    {
        if (activeFadeCoroutine != null)
        {
            StopCoroutine(activeFadeCoroutine);
        }

        activeFadeCoroutine = StartCoroutine(FadeSequence(zoneName));
    }

    private IEnumerator FadeSequence(string zoneName)
    {
        if (zoneText != null)
        {
            zoneText.text = zoneName;
        }

        // 1. Fade In
        float timer = 0f;
        while (timer < fadeInDuration)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeInDuration);
            yield return null;
        }
        canvasGroup.alpha = 1f;

        // 2. Hold on screen
        yield return new WaitForSeconds(displayDuration);

        // 3. Fade Out
        timer = 0f;
        while (timer < fadeOutDuration)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeOutDuration);
            yield return null;
        }
        canvasGroup.alpha = 0f;

        activeFadeCoroutine = null;
    }
}