using System.Collections;
using UnityEngine;
using TMPro;

public class PuzzleHint : MonoBehaviour
{
    //small upper-left hint that pops only when the player gets a puzzle wrong;
    //after enough wrong tries it glows the right answer

    public static PuzzleHint Instance;

    [Header("Corner hint UI (pin this panel top-left)")]
    public CanvasGroup hintPanel;
    public TMP_Text hintText;
    [Tooltip("How long each hint stays before fading out.")]
    public float showSeconds = 3f;
    public float fadeSpeed = 4f;

    [Header("Glow")]
    [Tooltip("Wrong tries before the right answer starts glowing.")]
    public int wrongsBeforeGlow = 3;

    int wrongCount = 0;
    Coroutine showRoutine;

    void Awake()
    {
        Instance = this;
        if (hintPanel != null)
        {
            hintPanel.alpha = 0f;
            hintPanel.gameObject.SetActive(true);
        }
    }

    // a puzzle slot calls this on a wrong placement.
    // hints = escalating lines (1st wrong -> hints[0], ...). correctAnswer = object to glow after enough wrongs.
    public void WrongAnswer(string[] hints, HintGlow correctAnswer)
    {
        wrongCount++;

        ShowHint(PickHint(hints));

        if (wrongCount >= wrongsBeforeGlow && correctAnswer != null)
            correctAnswer.StartGlow();
    }

    string PickHint(string[] hints)
    {
        if (hints == null || hints.Length == 0) return "Hmm, that's not it. Try again!";
        int i = Mathf.Min(wrongCount - 1, hints.Length - 1);
        return hints[i];
    }

    public void ShowHint(string msg)
    {
        if (hintText != null) hintText.text = msg;

        if (showRoutine != null) StopCoroutine(showRoutine);
        showRoutine = StartCoroutine(ShowRoutine());
    }

    IEnumerator ShowRoutine()
    {
        while (hintPanel != null && hintPanel.alpha < 1f)
        {
            hintPanel.alpha = Mathf.MoveTowards(hintPanel.alpha, 1f, fadeSpeed * Time.deltaTime);
            yield return null;
        }

        yield return new WaitForSeconds(showSeconds);

        while (hintPanel != null && hintPanel.alpha > 0f)
        {
            hintPanel.alpha = Mathf.MoveTowards(hintPanel.alpha, 0f, fadeSpeed * Time.deltaTime);
            yield return null;
        }
    }

    // call when a puzzle starts/closes so the wrong-count starts fresh
    public void ResetHints()
    {
        wrongCount = 0;
    }
}
