using System;
using System.Collections;
using UnityEngine;

public class LoadingOverlay : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeIntime = 0.5f;
    [SerializeField] private float fadeOuttime = 0.5f;
    public IEnumerator FadeInBlack()
    {
        yield return Fadeto(1f, fadeIntime);
    }

    public IEnumerator FadeOutBlack()
    {
        yield return Fadeto(0f, fadeOuttime);
    }
    private IEnumerator Fadeto(float targetAlpha, float duration)
    {
        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }
        canvasGroup.alpha = targetAlpha;
    }
}
