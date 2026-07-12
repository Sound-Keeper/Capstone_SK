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
        [Tooltip("Select which house this specific scene belongs to.")]
        public HouseType currentHouse = HouseType.HouseE;

        [Header("UI References")]
        public TextMeshProUGUI dialogueText;
        public TextMeshProUGUI speakerNameText;
        public GameObject pip;
        public GameObject judge;
        public Button continuebutton;

        [Header("Vowel Stone Reference")]
        [Tooltip("Drag the VowelStone GameObject containing the VowelStoneCutscene script here.")]
        public VowelStoneCutscene stoneCutscene;

        [Header("NPC Dialogue Arrays")]
        public string[] introDialogues = { "Welcome to the trial of vowels.", "Choose the correct verb to proceed." };
        public string[] correctDialogues = { "That's correct! Great job!", "Your linguistic skills are sharp." };
        public string[] wrongDialogues = { "Oops! Try again.", "That syntax doesn't seem quite right..." };
        public string[] nextStepDialogues = { "Splendid.", "Now flip the page." };

        [Header("Extra Messages")]
        public string[] finalDialogues = { "Good job, you have finished my puzzle.", "You have proven your worth." };

        [Header("Timing")]
        public float wrongDisplayDuration = 1.5f;
        public float dialogueLineDelay = 2.0f;

        [Header("Typing Settings")]
        public float typingSpeed = 0.05f;

        [Header("Book Reference")]
        public BookPro book;

        private Coroutine dialogueSequenceCoroutine;
        private Coroutine characterPrinterCoroutine;

        public bool puzzleCompleted = false;
        private int correctCount = 0;
        public int totalRequiredCorrect = 2;

        // --- TRACK EARLY FLIPS ---
        private bool pageWasFlipped = false;
        private UnityEngine.Events.UnityAction flipListener;

        // --- CLICK TO SKIP & ADVANCE STATES ---
        private bool isTyping = false;
        private bool currentLineSkipped = false;
        private bool userClickedNext = false;
        private bool inputIsDisabled = false; // Prevents clicking during auto feedback sequences

        void Start()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            flipListener = () => { pageWasFlipped = true; };

            SetSpeakerUI(showJudge: true);
            PlayDialogueGroup(introDialogues, isJudge: true);
        }

        void Update()
        {
            if (inputIsDisabled) return;

            // Listen for a left-mouse click or interaction button input
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space))
            {
                if (isTyping)
                {
                    currentLineSkipped = true; // Skip typing to reveal whole text block
                }
                else
                {
                    userClickedNext = true; // Advance to the next line of dialogue
                }
            }
        }

        public void ShowFeedback(bool isCorrect, Action onWrongFinished = null)
        {
            ResetAllActiveDialogues();

            if (isCorrect)
            {
                SetSpeakerUI(showJudge: false);
                correctCount++;

                if (correctCount >= totalRequiredCorrect)
                {
                    puzzleCompleted = true;
                    ApplyHouseCompletionFlags();
                    FinalSequence();
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
                case HouseType.HouseO:
                    PuzzleProgress.HouseOSolved = true;
                    PuzzleProgress.HouseOComplete = true;
                    PuzzleProgress.HasVowelOStone = true;
                    break;
                case HouseType.HouseA:
                    PuzzleProgress.HouseASolved = true;
                    PuzzleProgress.HouseAComplete = true;
                    PuzzleProgress.HasVowelAStone = true;
                    break;
                case HouseType.HouseI:
                    PuzzleProgress.HouseISolved = true;
                    PuzzleProgress.HouseIComplete = true;
                    PuzzleProgress.HasVowelIStone = true;
                    break;
                case HouseType.HouseU:
                    PuzzleProgress.HouseUSolved = true;
                    PuzzleProgress.HouseUComplete = true;
                    PuzzleProgress.HasVowelUStone = true;
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

            foreach (string line in lines)
            {
                yield return StartCoroutine(TypeText(line));
                yield return StartCoroutine(WaitUntilClick());
            }
        }

        private void FinalSequence()
        {
            if (stoneCutscene != null)
            {
                stoneCutscene.PlayCutscene();
            }
            else
            {
                StartFinalDialogueSequence();
            }
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

            foreach (string line in finalDialogues)
            {
                yield return StartCoroutine(TypeText(line));
                yield return StartCoroutine(WaitUntilClick());
            }

            yield return StartCoroutine(TypeText("..."));
            if (continuebutton != null) continuebutton.gameObject.SetActive(true);
        }

        // --- AUTOMATIC CORRECT SEQUENCE ---
        IEnumerator CorrectSequence()
        {
            inputIsDisabled = true; // Disable click tracking completely for feedback execution
            SetSpeakerName(isJudge: false);

            foreach (string line in correctDialogues)
            {
                yield return StartCoroutine(TypeText(line));
                yield return new WaitForSeconds(dialogueLineDelay); // Back to automatic timing
            }

            yield return new WaitForSeconds(0.5f);

            foreach (string line in nextStepDialogues)
            {
                if (pageWasFlipped) break;

                yield return StartCoroutine(TypeText(line));
                yield return new WaitForSeconds(dialogueLineDelay); // Back to automatic timing
            }

            inputIsDisabled = false; // Re-enable click requirements for the upcoming page flip block

            yield return new WaitUntil(() => pageWasFlipped);

            if (book != null)
                book.OnFlip.RemoveListener(flipListener);

            SetSpeakerUI(showJudge: true);
            SetSpeakerName(isJudge: true);

            if (introDialogues.Length > 0)
                yield return StartCoroutine(TypeText(introDialogues[introDialogues.Length - 1]));
        }

        // --- AUTOMATIC WRONG SEQUENCE ---
        IEnumerator WrongSequence(Action callback)
        {
            inputIsDisabled = true; // Disable click tracking completely for feedback execution
            SetSpeakerName(isJudge: false);

            foreach (string line in wrongDialogues)
            {
                yield return StartCoroutine(TypeText(line));
                yield return new WaitForSeconds(wrongDisplayDuration); // Back to automatic timing
            }

            SetSpeakerUI(showJudge: true);
            SetSpeakerName(isJudge: true);

            if (introDialogues.Length > 0)
                yield return StartCoroutine(TypeText(introDialogues[introDialogues.Length - 1]));

            inputIsDisabled = false; // Restore normal click requirements
            callback?.Invoke();
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

        IEnumerator TypeText(string message)
        {
            StopCharacterPrinter();
            dialogueText.text = "";

            isTyping = true;
            currentLineSkipped = false;

            characterPrinterCoroutine = StartCoroutine(CoroutineObjectReferenceHolder(message));
            yield return characterPrinterCoroutine;
        }

        private IEnumerator CoroutineObjectReferenceHolder(string message)
        {
            foreach (char letter in message)
            {
                // Only skip via click if click inputs aren't disabled right now
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