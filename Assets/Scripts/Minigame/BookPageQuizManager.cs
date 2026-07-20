using UnityEngine;
using System.Collections;
using BookCurlPro;

public class BookPageQuizManager : MonoBehaviour
{
    [Header("Page Identity")]
    [Tooltip("The Paper index of this specific page in BookPro (e.g., if this is Page 3-4, find which Paper index it maps to).")]
    public int targetPaperIndex;

    [Header("Indicator Settings")]
    [Tooltip("Assign the blinking arrow GameObject for this page here.")]
    public GameObject blinkingArrowIndicator;
    public float blinkSpeed = 0.4f;

    [Header("Book Hook")]
    [Tooltip("Assign the main BookPro object from your hierarchy.")]
    public BookPro book;

    private Coroutine blinkCoroutine;
    private bool isCleared = false;

    void Start()
    {
        if (blinkingArrowIndicator != null)
            blinkingArrowIndicator.SetActive(false);

        // Listen for book flips to cleanly track page positions
        if (book != null)
            book.OnFlip.AddListener(OnBookPageFlipped);
    }

    void OnDestroy()
    {
        if (book != null)
            book.OnFlip.RemoveListener(OnBookPageFlipped);
    }

    public void MarkPageAsCleared()
    {
        if (isCleared) return;
        isCleared = true;

        if (blinkingArrowIndicator != null)
        {
            if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);
            blinkCoroutine = StartCoroutine(BlinkRoutine());
        }
    }

    private void OnBookPageFlipped()
    {
        if (!isCleared || book == null) return;

        // If the current active book paper doesn't match our target anymore, the player turned away!
        if (book.CurrentPaper != targetPaperIndex)
        {
            if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);
            if (blinkingArrowIndicator != null) blinkingArrowIndicator.SetActive(false);
        }
    }

    private IEnumerator BlinkRoutine()
    {
        while (true)
        {
            blinkingArrowIndicator.SetActive(true);
            // ─── CHANGED TO REALTIME TO IGNORE TIME.TIMESCALE = 0 ───
            yield return new WaitForSecondsRealtime(blinkSpeed);

            blinkingArrowIndicator.SetActive(false);
            // ─── CHANGED TO REALTIME TO IGNORE TIME.TIMESCALE = 0 ───
            yield return new WaitForSecondsRealtime(blinkSpeed);
        }
    }
}