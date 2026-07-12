using BookCurlPro;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BookChoice
{
    public class DialogueBoxManagerHouseO : MonoBehaviour
    {
        [Header("UI References")]
        public TextMeshProUGUI dialogueText;
        public TextMeshProUGUI speakerNameText;
        public GameObject pip;
        public GameObject grandmaPhonics;
        public Button continuebutton;

        [Header("Vowel Stone Reference")]
        [Tooltip("Drag the House O VowelStone GameObject containing the VowelStoneCutscene script here.")]
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

        [Header("House O Puzzle Rules")]
        public int totalRequiredCorrect = 3;

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

            // Cache the flip listener setup
            flipListener = () => { pageWasFlipped = true; };

            SetSpeakerUI(showGrandma: true);
            PlayDialogueGroup(introDialogues, isGrandma: true);
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
                SetSpeakerUI(showGrandma: false); // Pip takes over
                correctCount++;

                if (correctCount >= totalRequiredCorrect)
                {
                    puzzleCompleted = true;

                    // --- EXCLUSIVE HOUSE O GLOBAL PROGRESS FLAGS ---
                    PuzzleProgress.HouseOSolved = true;
                    PuzzleProgress.HouseOComplete = true;
                    PuzzleProgress.HasVowelOStone = true;

                    FinalSequence();
                }
                else
                {
                    // Start listening for a page flip immediately!
                    pageWasFlipped = false;
                    if (book != null) book.OnFlip.AddListener(flipListener);

                    dialogueSequenceCoroutine = StartCoroutine(CorrectSequence());
                }
            }
            else
            {
                SetSpeakerUI(showGrandma: false); // Pip takes over
                dialogueSequenceCoroutine = StartCoroutine(WrongSequence(onWrongFinished));
            }
        }

        public void PlayDialogueGroup(string[] lines, bool isGrandma)
        {
            if (dialogueSequenceCoroutine != null)
                StopCoroutine(dialogueSequenceCoroutine);

            SetSpeakerName(isGrandma);
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
            SetSpeakerUI(showGrandma: true);
            SetSpeakerName(isGrandma: true);

            foreach (string line in finalDialogues)
            {
                yield return StartCoroutine(TypeText(line));
                yield return StartCoroutine(WaitUntilClick());
            }

            yield return StartCoroutine(TypeText("..."));
            yield return StartCoroutine(WaitUntilClick());

            if (continuebutton != null) continuebutton.gameObject.SetActive(true);
        }

        // --- AUTOMATIC CORRECT SEQUENCE ---
        IEnumerator CorrectSequence()
        {
            inputIsDisabled = true; // Disable click tracking completely for feedback execution
            SetSpeakerName(isGrandma: false);

            foreach (string line in correctDialogues)
            {
                yield return StartCoroutine(TypeText(line));
                yield return new WaitForSeconds(dialogueLineDelay); // Automatic delay
            }

            yield return new WaitForSeconds(0.5f);

            foreach (string line in nextStepDialogues)
            {
                // If they've already flipped early, don't force them to read "Now flip the page."
                if (pageWasFlipped) break;

                yield return StartCoroutine(TypeText(line));
                yield return new WaitForSeconds(dialogueLineDelay); // Automatic delay
            }

            inputIsDisabled = false; // Restore click requirements for the active page flip block

            // --- FIX: Safely wait for the flag instead of an active event window ---
            yield return new WaitUntil(() => pageWasFlipped);

            if (book != null)
                book.OnFlip.RemoveListener(flipListener);

            SetSpeakerUI(showGrandma: true);
            SetSpeakerName(isGrandma: true);

            if (introDialogues.Length > 0)
                yield return StartCoroutine(TypeText(introDialogues[introDialogues.Length - 1]));
        }

        // --- AUTOMATIC WRONG SEQUENCE ---
        IEnumerator WrongSequence(Action callback)
        {
            inputIsDisabled = true; // Disable click tracking completely for feedback execution
            SetSpeakerName(isGrandma: false);

            foreach (string line in wrongDialogues)
            {
                yield return StartCoroutine(TypeText(line));
                yield return new WaitForSeconds(wrongDisplayDuration); // Automatic delay
            }

            SetSpeakerUI(showGrandma: true);
            SetSpeakerName(isGrandma: true);

            if (introDialogues.Length > 0)
                yield return StartCoroutine(TypeText(introDialogues[introDialogues.Length - 1]));

            inputIsDisabled = false; // Restore normal click requirements
            callback?.Invoke();
        }

        private void ResetAllActiveDialogues()
        {
            // Clean up the book listener tracking if a sequence resets mid-action
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

        private void SetSpeakerUI(bool showGrandma)
        {
            if (grandmaPhonics != null) grandmaPhonics.gameObject.SetActive(showGrandma);
            if (pip != null) pip.gameObject.SetActive(!showGrandma);
        }

        private void SetSpeakerName(bool isGrandma)
        {
            if (speakerNameText != null)
            {
                speakerNameText.text = isGrandma ? "Grandma Phonics:" : "Pip:";
            }
        }

        private void OnDestroy()
        {
            if (book != null) book.OnFlip.RemoveListener(flipListener);
        }
    }
}