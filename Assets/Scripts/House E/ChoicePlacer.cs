using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace BookChoice
{
    /// <summary>
    /// Attach this to each choice button (1stOption, 2ndOption, 3rdOption, 4thOption).
    /// </summary>
    public class ChoicePlacer : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The blank/underline RectTransform on the left page where the answer snaps to.")]
        public RectTransform blankSlot;

        [Tooltip("Is this the correct answer?")]
        public bool isCorrect = false;

        [Tooltip("Dialogue box manager that shows feedback.")]
        public DialogueBoxManager dialogueBox;

        // Shared across all ChoicePlacer instances to track if the slot is taken
        private bool slotOccupied = false;

        // ── internals ──────────────────────────────────────────────────────────
        private RectTransform rectTransform;
        private Vector2 originalAnchoredPosition;
        private Transform originalParent;
        private FloatingChar floatingChar;
        private Button button;
        private bool isPlaced = false;

        void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            floatingChar = GetComponent<FloatingChar>();
            button = GetComponent<Button>();

            originalAnchoredPosition = rectTransform.anchoredPosition;
            originalParent = transform.parent;

            button.onClick.AddListener(OnClicked);
        }

        void OnClicked()
        {
            if (isPlaced) return;
            if (slotOccupied) return;

            PlaceOnBlank();
        }

        void PlaceOnBlank()
        {
            isPlaced = true;
            slotOccupied = true;

            if (floatingChar != null) floatingChar.enabled = false;

            transform.SetParent(blankSlot.parent, true);
            rectTransform.anchoredPosition = blankSlot.anchoredPosition;

            button.interactable = false;

            if (dialogueBox != null)
                dialogueBox.ShowFeedback(isCorrect, OnWrongAnswerDialogueFinished);
        }

        public void OnWrongAnswerDialogueFinished()
        {
            if (isCorrect) return;
            ReturnToOriginalPlace();
        }


        void ReturnToOriginalPlace()
        {
            isPlaced = false;
            slotOccupied = false;

            transform.SetParent(originalParent, true);
            rectTransform.anchoredPosition = originalAnchoredPosition;

            if (floatingChar != null)
            {
                floatingChar.enabled = true;
                floatingChar.ResetFloatOrigin();
            }

            button.interactable = true;
        }
    }
}