using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace BookChoice
{
    public class ChoicePlacer : MonoBehaviour
    {
        [Header("References")]
        public RectTransform blankSlot;
        public bool isCorrect = false;

        [Tooltip("Assign this if you are using the standard DialogueBoxManager (e.g., House E).")]
        public DialogueBoxManager dialogueBox;

        [Tooltip("Assign this if you are using DialogueBoxManagerHouseO (e.g., House O).")]
        public DialogueBoxManagerHouseO dialogueBoxHouseO;

        [Header("Page Settings")]
        public int pageID = 0;

        // Track all choices
        private static List<ChoicePlacer> allChoices = new List<ChoicePlacer>();

        // Per-page lock system
        private static Dictionary<int, bool> pageLock = new Dictionary<int, bool>();

        // ── internals ─────────────────────────────────────────────
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

            allChoices.Add(this);
        }

        void OnDestroy()
        {
            allChoices.Remove(this);

            // --- FIX APPLIED HERE ---
            // When the scene unloads, if there are no more active choices left in the hierarchy,
            // completely clear out the static locks so the next house starts fresh!
            if (allChoices.Count == 0)
            {
                pageLock.Clear();
            }
        }

        void OnClicked()
        {
            if (isPlaced) return;

            if (pageLock.ContainsKey(pageID) && pageLock[pageID])
                return;

            pageLock[pageID] = true;

            PlaceOnBlank();
        }

        void PlaceOnBlank()
        {
            isPlaced = true;

            if (floatingChar != null)
                floatingChar.enabled = false;

            transform.SetParent(blankSlot.parent, true);
            rectTransform.anchoredPosition = blankSlot.anchoredPosition;

            DisablePageButtons();

            if (dialogueBox != null)
            {
                dialogueBox.ShowFeedback(isCorrect, OnWrongAnswerDialogueFinished);
            }
            else if (dialogueBoxHouseO != null)
            {
                dialogueBoxHouseO.ShowFeedback(isCorrect, OnWrongAnswerDialogueFinished);
            }
            else
            {
                Debug.LogWarning($"ChoicePlacer on {gameObject.name}: No dialogue manager script is assigned in the inspector!");
            }
        }

        public void OnWrongAnswerDialogueFinished()
        {
            if (isCorrect) return;

            ReturnToOriginalPlace();
        }

        void ReturnToOriginalPlace()
        {
            isPlaced = false;

            pageLock[pageID] = false;

            transform.SetParent(originalParent, true);
            rectTransform.anchoredPosition = originalAnchoredPosition;

            if (floatingChar != null)
            {
                floatingChar.enabled = true;
                floatingChar.ResetFloatOrigin();
            }

            EnablePageButtons();
        }

        void DisablePageButtons()
        {
            foreach (var choice in allChoices)
            {
                if (choice.pageID == pageID)
                    choice.button.interactable = false;
            }
        }

        void EnablePageButtons()
        {
            foreach (var choice in allChoices)
            {
                if (choice.pageID == pageID)
                    choice.button.interactable = true;
            }
        }
    }
}