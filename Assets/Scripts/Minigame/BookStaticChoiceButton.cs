using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class BookStaticChoiceButton : MonoBehaviour
{
    [Header("Answer Logic")]
    public bool isCorrect = false;

    [Header("Audio Configurations")]
    [Tooltip("Audio clip for correct validation.")]
    public AudioClip correctSFX;
    [Tooltip("Audio clip for incorrect validation.")]
    public AudioClip incorrectSFX;

    [Header("Page Grouping References")]
    [Tooltip("Drag all 3 choice buttons belonging to this specific question here (including this button).")]
    public Button[] questionGroupButtons;

    [Tooltip("Drag the BookPageQuizManager component sitting on this page's parent container.")]
    public BookPageQuizManager pageManager;

    private Button currentButton;

    void Awake()
    {
        currentButton = GetComponent<Button>();
        currentButton.onClick.AddListener(EvaluateChoice);
    }

    void EvaluateChoice()
    {
        if (isCorrect)
        {
            // Play success feedback via audio core
            if (correctSFX != null)
            {
                CoreAudioManager.PlaySFX(correctSFX);
            }

            // Lock all 3 buttons in this question set completely
            DisableQuestionSet();

            // Notify the page manager that the question is solved
            if (pageManager != null)
            {
                pageManager.MarkPageAsCleared();
            }
        }
        else
        {
            // Play error fallback feedback sound
            if (incorrectSFX != null)
            {
                CoreAudioManager.PlaySFX(incorrectSFX);
            }

            Debug.Log($"[Quiz Game] Choice selection on '{gameObject.name}' is incorrect. Try again!");
        }
    }

    void DisableQuestionSet()
    {
        if (questionGroupButtons == null) return;

        foreach (Button btn in questionGroupButtons)
        {
            if (btn != null)
            {
                btn.interactable = false;
            }
        }
    }
}