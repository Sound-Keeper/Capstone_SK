using UnityEngine;
using System.Collections;
using BookCurlPro;

public class BookPageQuizManager : MonoBehaviour
{
    [Header("Page Identity")]
    [Tooltip("The Paper index of this specific page in BookPro.")]
    public int targetPaperIndex;

    [Header("Indicator Settings")]
    [Tooltip("Assign the blinking arrow GameObject for this page here.")]
    public GameObject blinkingArrowIndicator;
    public float blinkSpeed = 0.4f;

    [Header("Book Hook")]
    [Tooltip("Assign the main BookPro object from your hierarchy.")]
    public BookPro book;

    // Track total completed pages across all instances
    public static int totalClearedPages = 0;
    public static int totalPagesInBook = 5; // Set this to the total number of questions/pages in the book

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
        totalClearedPages++;

        // Start blinking arrow feedback for turning page
        if (blinkingArrowIndicator != null)
        {
            if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);
            blinkCoroutine = StartCoroutine(BlinkRoutine());
        }

        // Check if all pages in the book are completed!
        if (totalClearedPages >= totalPagesInBook)
        {
            SoundBookTrigger bookTrigger = FindAnyObjectByType<SoundBookTrigger>();
            if (bookTrigger != null)
            {
                bookTrigger.OnAllPagesCompleted();
            }
        }
    }

    private void OnBookPageFlipped()
    {
        if (!isCleared || book == null) return;

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
            yield return new WaitForSecondsRealtime(blinkSpeed);

            blinkingArrowIndicator.SetActive(false);
            yield return new WaitForSecondsRealtime(blinkSpeed);
        }
    }
}