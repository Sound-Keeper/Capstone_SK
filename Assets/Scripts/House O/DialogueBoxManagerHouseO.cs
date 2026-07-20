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
        // New structure to allow effortless customization inside the Unity Inspector
        [System.Serializable]
        public struct HomophoneChoiceData
        {
            public string wordName;
            public AudioClip ttsAudioClip;
        }

        [System.Serializable]
        public struct HomophoneExplanationData
        {
            [Header("Song Definition Context")]
            [TextArea(2, 3)] public string initialIntroductionText;
            public HomophoneChoiceData choice1;
            public HomophoneChoiceData choice2;
            public HomophoneChoiceData choice3;
            public HomophoneChoiceData choice4;
        }

        [Header("UI References")]
        public TextMeshProUGUI dialogueText;
        public TextMeshProUGUI speakerNameText;
        public GameObject pip;
        public GameObject grandmaPhonics;

        [Header("Dialogue Flow Indicator")]
        [Tooltip("Drag and drop your blinking icon/dot GameObject indicator here.")]
        public GameObject blinkerObject;
        public float indicatorBlinkSpeed = 0.4f;

        public Button continuebutton;

        [Header("Song Interaction Controls")]
        public Button song1Button;
        public Button song2Button;
        public Button song3Button;

        [Header("Song Audio Clips")]
        public AudioClip song1Audio;
        public AudioClip song2Audio;
        public AudioClip song3Audio;

        [Header("Vowel Stone Reference")]
        public VowelStoneCutscene stoneCutscene;

        [Header("NPC Dialogue Arrays")]
        public string[] introDialogues = {
            "Welcome to the trial of vowels.",
            "One word is missing at the end. Use your...",
            "We have three songs to mend, so listen...",
            "Choose the right rhyming word!"
        };

        [Tooltip("Pip's initial reaction text before explaining the definition.")]
        public string correctFeedbackGreeting = "That's correct! Great job!";

        public string[] wrongDialogues = { "Oops! Try again." };
        public string[] nextStepDialogues = { "Now flip the page to continue." };

        [Header("Extra Messages")]
        public string[] finalDialogues = {
            "There we are! All three songs, good as...",
            "Bless your heart, Sound Keeper. Miss Sp..."
        };

        [Header("House O Shared Audio")]
        public AudioClip correctSFX;
        public AudioClip wrongSFX;

        [Header("House O - New Inspector Customizable Explanations")]
        [Tooltip("Configure the definitions for each of the 3 song milestones here. Element 0 is for Song 1, Element 1 is for Song 2, etc.")]
        public HomophoneExplanationData[] songExplanationData = new HomophoneExplanationData[3];

        [Header("Timing")]
        public float wrongDisplayDuration = 1.5f;
        public float dialogueLineDelay = 2.0f;

        [Header("Typing Settings")]
        public float typingSpeed = 0.05f;

        [Header("Book Reference")]
        public BookPro book;

        private Coroutine dialogueSequenceCoroutine;
        private Coroutine characterPrinterCoroutine;
        private Coroutine blinkCoroutine;
        private Coroutine songMonitorCoroutine;

        public bool puzzleCompleted = false;
        private int correctCount = 0;

        [Header("House O Puzzle Rules")]
        public int totalRequiredCorrect = 3;

        private bool pageWasFlipped = false;
        private UnityEngine.Events.UnityAction flipListener;
        private UnityEngine.Events.UnityAction songPageTrackerListener;

        private bool isTyping = false;
        private bool currentLineSkipped = false;
        private bool userClickedNext = false;
        private bool inputIsDisabled = false;

        void Start()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            flipListener = () => { pageWasFlipped = true; };

            songPageTrackerListener = () => { UpdateSongInteractivityByPage(); };
            if (book != null)
            {
                book.OnFlip.AddListener(songPageTrackerListener);
            }

            if (blinkerObject != null) blinkerObject.SetActive(false);

            SetSpeakerUI(showGrandma: true);
            PlayDialogueGroup(introDialogues, isGrandma: true);

            UpdateSongInteractivityByPage();

            if (song1Button != null) song1Button.onClick.AddListener(() => PlaySongAudio(1));
            if (song2Button != null) song2Button.onClick.AddListener(() => PlaySongAudio(2));
            if (song3Button != null) song3Button.onClick.AddListener(() => PlaySongAudio(3));
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

        public void PlaySongAudio(int songNumber)
        {
            AudioClip clipToPlay = null;

            if (songNumber == 1) clipToPlay = song1Audio;
            else if (songNumber == 2) clipToPlay = song2Audio;
            else if (songNumber == 3) clipToPlay = song3Audio;

            if (clipToPlay != null)
            {
                if (songMonitorCoroutine != null)
                {
                    StopCoroutine(songMonitorCoroutine);
                }

                CoreAudioManager.StopSFX();
                CoreAudioManager.PauseBGM();
                CoreAudioManager.PlaySFX(clipToPlay);

                songMonitorCoroutine = StartCoroutine(WaitForSongToEnd(clipToPlay.length));
            }
        }

        private IEnumerator WaitForSongToEnd(float clipLength)
        {
            yield return new WaitForSeconds(clipLength);
            CoreAudioManager.ResumeBGM();
            songMonitorCoroutine = null;
        }

        private void UpdateSongInteractivityByPage()
        {
            if (book == null) return;

            int currentPaperIndex = book.currentPaper;

            if (song1Button != null) song1Button.interactable = (currentPaperIndex == 1);
            if (song2Button != null) song2Button.interactable = (currentPaperIndex == 2);
            if (song3Button != null) song3Button.interactable = (currentPaperIndex == 3);
        }

        public void ShowFeedback(bool isCorrect, Action onWrongFinished = null)
        {
            if (songMonitorCoroutine != null)
            {
                StopCoroutine(songMonitorCoroutine);
                songMonitorCoroutine = null;
            }

            CoreAudioManager.FadeOutSFX(0.5f);
            ResetAllActiveDialogues();

            if (isCorrect)
            {
                SetSpeakerUI(showGrandma: false);
                correctCount++;

                if (correctCount >= totalRequiredCorrect)
                {
                    puzzleCompleted = true;

                    PuzzleProgress.HouseOSolved = true;
                    PuzzleProgress.HouseOComplete = true;
                    PuzzleProgress.HasVowelOStone = true;

                    dialogueSequenceCoroutine = StartCoroutine(Answer3Sequence());
                }
                else
                {
                    pageWasFlipped = false;
                    if (book != null) book.OnFlip.AddListener(flipListener);

                    dialogueSequenceCoroutine = StartCoroutine(CorrectSequence(correctCount - 1));
                }
            }
            else
            {
                SetSpeakerUI(showGrandma: false);
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

            for (int i = 0; i < lines.Length; i++)
            {
                bool shouldBlink = (lines == introDialogues && i >= 0 && i <= 2);

                yield return StartCoroutine(TypeText(lines[i], shouldBlink));
                yield return StartCoroutine(WaitUntilClick());
            }
        }

        // Handles the interactive homophone explanation mechanics smoothly
        IEnumerator CorrectSequence(int explanationIndex)
        {
            SetSpeakerName(isGrandma: false);

            if (correctSFX != null) CoreAudioManager.PlaySFX(correctSFX);

            yield return StartCoroutine(TypeText(correctFeedbackGreeting, showIndicator: true));
            yield return StartCoroutine(WaitUntilClick());

            // Run interactive breakdown sequence
            if (explanationIndex >= 0 && explanationIndex < songExplanationData.Length)
            {
                SetSpeakerUI(showGrandma: true);
                SetSpeakerName(isGrandma: true);

                HomophoneExplanationData data = songExplanationData[explanationIndex];

                // Section 1: Introduction text block
                yield return StartCoroutine(TypeText(data.initialIntroductionText, showIndicator: true));
                yield return StartCoroutine(WaitUntilClick());

                // Section 2: "Lets read it together."
                yield return StartCoroutine(TypeText("Let's read it together.", showIndicator: true));
                yield return StartCoroutine(WaitUntilClick());

                // Section 3: Word 1 choice
                if (data.choice1.ttsAudioClip != null) CoreAudioManager.PlaySFX(data.choice1.ttsAudioClip);
                yield return StartCoroutine(TypeText(data.choice1.wordName, showIndicator: true));
                yield return StartCoroutine(WaitUntilClick());

                // Section 4: Word 2 choice
                if (data.choice2.ttsAudioClip != null) CoreAudioManager.PlaySFX(data.choice2.ttsAudioClip);
                yield return StartCoroutine(TypeText(data.choice2.wordName, showIndicator: true));
                yield return StartCoroutine(WaitUntilClick());

                // Section 5: Word 3 choice
                if (data.choice3.ttsAudioClip != null) CoreAudioManager.PlaySFX(data.choice3.ttsAudioClip);
                yield return StartCoroutine(TypeText(data.choice3.wordName, showIndicator: true));
                yield return StartCoroutine(WaitUntilClick());

                // Section 6: Word 4 choice
                if (data.choice4.ttsAudioClip != null) CoreAudioManager.PlaySFX(data.choice4.ttsAudioClip);
                yield return StartCoroutine(TypeText(data.choice4.wordName, showIndicator: true));
                yield return StartCoroutine(WaitUntilClick());
            }

            SetSpeakerUI(showGrandma: false);
            SetSpeakerName(isGrandma: false);

            foreach (string line in nextStepDialogues)
            {
                if (pageWasFlipped) break;

                yield return StartCoroutine(TypeText(line, showIndicator: false));
                yield return StartCoroutine(WaitUntilClick());
            }

            yield return new WaitUntil(() => pageWasFlipped);

            if (book != null)
                book.OnFlip.RemoveListener(flipListener);

            SetSpeakerUI(showGrandma: true);
            SetSpeakerName(isGrandma: true);

            if (introDialogues.Length > 0)
            {
                yield return StartCoroutine(TypeText(introDialogues[introDialogues.Length - 1], showIndicator: false));
            }
        }

        IEnumerator Answer3Sequence()
        {
            SetSpeakerName(isGrandma: false);

            if (correctSFX != null) CoreAudioManager.PlaySFX(correctSFX);

            yield return StartCoroutine(TypeText(correctFeedbackGreeting, showIndicator: true));
            yield return StartCoroutine(WaitUntilClick());

            // Run explanation for Song 3 milestone safely
            if (songExplanationData.Length >= 3)
            {
                SetSpeakerUI(showGrandma: true);
                SetSpeakerName(isGrandma: true);

                HomophoneExplanationData data = songExplanationData[2];

                yield return StartCoroutine(TypeText(data.initialIntroductionText, showIndicator: true));
                yield return StartCoroutine(WaitUntilClick());

                yield return StartCoroutine(TypeText("Let's read it together.", showIndicator: true));
                yield return StartCoroutine(WaitUntilClick());

                if (data.choice1.ttsAudioClip != null) CoreAudioManager.PlaySFX(data.choice1.ttsAudioClip);
                yield return StartCoroutine(TypeText(data.choice1.wordName, showIndicator: true));
                yield return StartCoroutine(WaitUntilClick());

                if (data.choice2.ttsAudioClip != null) CoreAudioManager.PlaySFX(data.choice2.ttsAudioClip);
                yield return StartCoroutine(TypeText(data.choice2.wordName, showIndicator: true));
                yield return StartCoroutine(WaitUntilClick());

                if (data.choice3.ttsAudioClip != null) CoreAudioManager.PlaySFX(data.choice3.ttsAudioClip);
                yield return StartCoroutine(TypeText(data.choice3.wordName, showIndicator: true));
                yield return StartCoroutine(WaitUntilClick());

                if (data.choice4.ttsAudioClip != null) CoreAudioManager.PlaySFX(data.choice4.ttsAudioClip);
                yield return StartCoroutine(TypeText(data.choice4.wordName, showIndicator: true));
                yield return StartCoroutine(WaitUntilClick());
            }

            if (stoneCutscene != null)
            {
                stoneCutscene.PlayCutscene();
            }
            else
            {
                StartFinalDialogueSequence();
            }
        }

        IEnumerator WrongSequence(Action callback)
        {
            inputIsDisabled = true;
            SetSpeakerName(isGrandma: false);

            if (wrongSFX != null) CoreAudioManager.PlaySFX(wrongSFX);

            foreach (string line in wrongDialogues)
            {
                yield return StartCoroutine(TypeText(line, showIndicator: true));
                yield return new WaitForSeconds(wrongDisplayDuration);
            }

            SetSpeakerUI(showGrandma: true);
            SetSpeakerName(isGrandma: true);

            if (introDialogues.Length > 0)
            {
                yield return StartCoroutine(TypeText(introDialogues[introDialogues.Length - 1], showIndicator: false));
            }

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
            SetSpeakerUI(showGrandma: true);
            SetSpeakerName(isGrandma: true);

            for (int i = 0; i < finalDialogues.Length; i++)
            {
                bool shouldBlink = (i == 0);
                yield return StartCoroutine(TypeText(finalDialogues[i], shouldBlink));
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
            if (blinkerObject == null) return;
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
            if (blinkerObject != null)
            {
                blinkerObject.SetActive(false);
            }
        }

        private IEnumerator BlinkRoutine()
        {
            while (true)
            {
                blinkerObject.SetActive(true);
                yield return new WaitForSeconds(indicatorBlinkSpeed);
                blinkerObject.SetActive(false);
                yield return new WaitForSeconds(indicatorBlinkSpeed);
            }
        }

        private void SetSpeakerUI(bool showGrandma)
        {
            if (grandmaPhonics != null) grandmaPhonics.gameObject.SetActive(showGrandma);
            if (pip != null) pip.gameObject.SetActive(!showGrandma);
        }

        private void OnDisable()
        {
            if (songMonitorCoroutine != null)
            {
                StopCoroutine(songMonitorCoroutine);
                songMonitorCoroutine = null;
            }
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
            if (book != null)
            {
                book.OnFlip.RemoveListener(flipListener);
                book.OnFlip.RemoveListener(songPageTrackerListener);
            }
        }
    }
}