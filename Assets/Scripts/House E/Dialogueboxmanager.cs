using UnityEngine;
using TMPro;
using System;
using System.Collections;
using BookCurlPro;
using UnityEngine.UI;

namespace BookChoice
{
    public class DialogueBoxManager : MonoBehaviour
    {
        public enum HouseType { HouseE, HouseO, HouseA, HouseI, HouseU }

        [Header("House Settings")]
        public HouseType currentHouse = HouseType.HouseE;

        [Header("UI References")]
        public TextMeshProUGUI dialogueText;
        public TextMeshProUGUI speakerNameText;
        public GameObject pip;
        public GameObject judge;
        public Button continuebutton;

        [Header("Dialogue Flow Indicator")]
        [Tooltip("Assign an icon/image here that blinks ONLY when there is MORE text following it.")]
        public GameObject nextLineIndicator;
        public float indicatorBlinkSpeed = 0.4f;

        [Header("Vowel Stone Reference")]
        public VowelStoneCutscene stoneCutscene;

        [Header("NPC Dialogue Arrays")]
        public string[] introDialogues;

        [Tooltip("Pip's initial reaction text before explaining the definition.")]
        public string correctFeedbackGreeting = "That's correct! Great job!";

        public string[] wrongDialogues;
        public string[] nextStepDialogues;

        [Header("Extra Messages")]
        public string[] finalDialogues;

        [Header("House E Word Meanings & Audio")]
        [TextArea(2, 4)] public string answer1MeaningText = "Definition for the first correct verb goes here...";
        public AudioClip answer1MeaningSFX;

        [Space]
        [TextArea(2, 4)] public string answer2MeaningText = "Definition for the second correct verb goes here...";
        public AudioClip answer2MeaningSFX;

        // ─── NEW WRONG FEEDBACK SFX SLOT ─────────────────────────────────────────────
        [Space]
        [Tooltip("Drag your custom 'Oops! Try again' incorrect buzz/error sound effect here.")]
        public AudioClip wrongFeedbackSFX;
        // ─────────────────────────────────────────────────────────────────────────────

        [Header("Timing")]
        public float wrongDisplayDuration = 1.5f;

        [Header("Typing Settings")]
        public float typingSpeed = 0.05f;

        [Header("Book Reference")]
        public BookPro book;

        private Coroutine dialogueSequenceCoroutine;
        private Coroutine characterPrinterCoroutine;
        private Coroutine blinkCoroutine;

        public bool puzzleCompleted = false;
        private int correctCount = 0;
        public int totalRequiredCorrect = 2;

        private bool pageWasFlipped = false;
        private UnityEngine.Events.UnityAction flipListener;

        private bool isTyping = false;
        private bool currentLineSkipped = false;
        private bool userClickedNext = false;
        private bool inputIsDisabled = false;

        void Start()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            flipListener = () => { pageWasFlipped = true; };

            if (nextLineIndicator != null) nextLineIndicator.SetActive(false);

            SetSpeakerUI(showJudge: true);
            PlayDialogueGroup(introDialogues, isJudge: true);
        }

        void Update()
        {
            if (inputIsDisabled) return;

            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space))
            {
                if (isTyping)
                {
                    currentLineSkipped = true;
                }
                else
                {
                    userClickedNext = true;
                }
            }
        }

        public void ShowFeedback(bool isCorrect, Action onWrongFinished = null)
        {
            ResetAllActiveDialogues();

            if (isCorrect)
            {
                correctCount++;

                if (correctCount >= totalRequiredCorrect)
                {
                    puzzleCompleted = true;
                    ApplyHouseCompletionFlags();
                    dialogueSequenceCoroutine = StartCoroutine(Answer2Sequence());
                }
                else
                {
                    pageWasFlipped = false;
                    if (book != null) book.OnFlip.AddListener(flipListener);

                    dialogueSequenceCoroutine = StartCoroutine(CorrectSequence());
                }
            }
            else
            {
                SetSpeakerUI(showJudge: false);
                dialogueSequenceCoroutine = StartCoroutine(WrongSequence(onWrongFinished));
            }
        }

        private void ApplyHouseCompletionFlags()
        {
            switch (currentHouse)
            {
                case HouseType.HouseE:
                    PuzzleProgress.HouseESolved = true;
                    PuzzleProgress.HouseEComplete = true;
                    PuzzleProgress.HasVowelEStone = true;
                    break;
            }
        }

        public void PlayDialogueGroup(string[] lines, bool isJudge)
        {
            if (dialogueSequenceCoroutine != null)
                StopCoroutine(dialogueSequenceCoroutine);

            SetSpeakerName(isJudge);
            dialogueSequenceCoroutine = StartCoroutine(DisplayDialogueLines(lines));
        }

        IEnumerator DisplayDialogueLines(string[] lines)
        {
            if (lines == null || lines.Length == 0) yield break;

            for (int i = 0; i < lines.Length; i++)
            {
                bool hasMoreLines = i < lines.Length - 1;
                yield return StartCoroutine(TypeText(lines[i], hasMoreLines));
                yield return StartCoroutine(WaitUntilClick());
            }
        }

        IEnumerator CorrectSequence()
        {
            SetSpeakerUI(showJudge: false);
            SetSpeakerName(isJudge: false);

            if (answer1MeaningSFX != null) CoreAudioManager.PlaySFX(answer1MeaningSFX);

            yield return StartCoroutine(TypeText(correctFeedbackGreeting, showIndicator: true));
            yield return StartCoroutine(WaitUntilClick());

            if (!string.IsNullOrEmpty(answer1MeaningText))
            {
                SetSpeakerUI(showJudge: true);
                SetSpeakerName(isJudge: true);

                yield return StartCoroutine(TypeText(answer1MeaningText, showIndicator: true));
                yield return StartCoroutine(WaitUntilClick());
            }

            SetSpeakerUI(showJudge: false);
            SetSpeakerName(isJudge: false);

            for (int i = 0; i < nextStepDialogues.Length; i++)
            {
                if (pageWasFlipped) break;

                bool hasMoreLines = i < nextStepDialogues.Length - 1;
                yield return StartCoroutine(TypeText(nextStepDialogues[i], hasMoreLines));
                yield return StartCoroutine(WaitUntilClick());
            }

            yield return new WaitUntil(() => pageWasFlipped);

            if (book != null)
                book.OnFlip.RemoveListener(flipListener);

            SetSpeakerUI(showJudge: true);
            SetSpeakerName(isJudge: true);

            if (introDialogues.Length > 0)
                yield return StartCoroutine(TypeText(introDialogues[introDialogues.Length - 1], showIndicator: false));
        }

        IEnumerator Answer2Sequence()
        {
            SetSpeakerUI(showJudge: false);
            SetSpeakerName(isJudge: false);

            if (answer2MeaningSFX != null) CoreAudioManager.PlaySFX(answer2MeaningSFX);

            yield return StartCoroutine(TypeText(correctFeedbackGreeting, showIndicator: true));
            yield return StartCoroutine(WaitUntilClick());

            SetSpeakerUI(showJudge: true);
            SetSpeakerName(isJudge: true);

            yield return StartCoroutine(TypeText(answer2MeaningText, showIndicator: false));
            yield return StartCoroutine(WaitUntilClick());

            if (stoneCutscene != null)
            {
                stoneCutscene.PlayCutscene();
            }
            else
            {
                StartFinalDialogueSequence();
            }
        }

        // --- UPDATED WRONG SEQUENCE WITH AUDIO ---
        IEnumerator WrongSequence(Action callback)
        {
            inputIsDisabled = true;
            SetSpeakerName(isJudge: false);

            // --- NEW: Play wrong sound instantly at the exact millisecond Pip pops up ---
            if (wrongFeedbackSFX != null)
            {
                CoreAudioManager.PlaySFX(wrongFeedbackSFX);
            }

            foreach (string line in wrongDialogues)
            {
                yield return StartCoroutine(TypeText(line, showIndicator: false));
                yield return new WaitForSeconds(wrongDisplayDuration);
            }

            SetSpeakerUI(showJudge: true);
            SetSpeakerName(isJudge: true);

            if (introDialogues.Length > 0)
                yield return StartCoroutine(TypeText(introDialogues[introDialogues.Length - 1], showIndicator: false));

            inputIsDisabled = false;
            callback?.Invoke();
        }

        public void StartFinalDialogueSequence()
        {
            if (book != null)
                book.gameObject.SetActive(false);

            dialogueSequenceCoroutine = StartCoroutine(TypeFinalDialoguesText());
        }

        IEnumerator TypeFinalDialoguesText()
        {
            SetSpeakerUI(showJudge: true);
            SetSpeakerName(isJudge: true);

            for (int i = 0; i < finalDialogues.Length; i++)
            {
                bool hasMoreLines = i < finalDialogues.Length - 1;
                yield return StartCoroutine(TypeText(finalDialogues[i], hasMoreLines));
                yield return StartCoroutine(WaitUntilClick());
            }

            yield return StartCoroutine(TypeText("...", showIndicator: false));
            if (continuebutton != null) continuebutton.gameObject.SetActive(true);
        }

        private void ResetAllActiveDialogues()
        {
            if (book != null) book.OnFlip.RemoveListener(flipListener);

            if (dialogueSequenceCoroutine != null)
            {
                StopCoroutine(dialogueSequenceCoroutine);
                dialogueSequenceCoroutine = null;
            }
            StopCharacterPrinter();
            StopBlinking();
            isTyping = false;
            inputIsDisabled = false;
        }

        private void StopCharacterPrinter()
        {
            if (characterPrinterCoroutine != null)
            {
                StopCoroutine(characterPrinterCoroutine);
                characterPrinterCoroutine = null;
            }
        }

        IEnumerator TypeText(string message, bool showIndicator)
        {
            StopCharacterPrinter();
            StopBlinking();
            dialogueText.text = "";

            isTyping = true;
            currentLineSkipped = false;

            characterPrinterCoroutine = StartCoroutine(CoroutineObjectReferenceHolder(message, showIndicator));
            yield return characterPrinterCoroutine;
        }

        private IEnumerator CoroutineObjectReferenceHolder(string message, bool showIndicator)
        {
            foreach (char letter in message)
            {
                if (currentLineSkipped && !inputIsDisabled)
                {
                    dialogueText.text = message;
                    break;
                }

                dialogueText.text += letter;
                yield return new WaitForSeconds(typingSpeed);
            }

            isTyping = false;
            characterPrinterCoroutine = null;

            if (!inputIsDisabled && showIndicator)
            {
                StartBlinking();
            }

            yield return null;
            userClickedNext = false;
        }

        private IEnumerator WaitUntilClick()
        {
            while (!userClickedNext)
            {
                yield return null;
            }
            userClickedNext = false;
            StopBlinking();
        }

        private void StartBlinking()
        {
            if (nextLineIndicator == null) return;
            StopBlinking();
            blinkCoroutine = StartCoroutine(BlinkRoutine());
        }

        private void StopBlinking()
        {
            if (blinkCoroutine != null)
            {
                StopCoroutine(blinkCoroutine);
                blinkCoroutine = null;
            }
            if (nextLineIndicator != null)
            {
                nextLineIndicator.SetActive(false);
            }
        }

        private IEnumerator BlinkRoutine()
        {
            while (true)
            {
                nextLineIndicator.SetActive(true);
                yield return new WaitForSeconds(indicatorBlinkSpeed);
                nextLineIndicator.SetActive(false);
                yield return new WaitForSeconds(indicatorBlinkSpeed);
            }
        }

        private void SetSpeakerUI(bool showJudge)
        {
            if (judge != null) judge.gameObject.SetActive(showJudge);
            if (pip != null) pip.gameObject.SetActive(!showJudge);
        }

        private void SetSpeakerName(bool isJudge)
        {
            if (speakerNameText != null)
            {
                speakerNameText.text = isJudge ? "Judge Mental:" : "Pip:";
            }
        }

        private void OnDestroy()
        {
            if (book != null) book.OnFlip.RemoveListener(flipListener);
        }
    }
}