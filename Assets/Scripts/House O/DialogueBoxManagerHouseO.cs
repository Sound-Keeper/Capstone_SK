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

        [Header("Dialogue Flow Indicator")]
        [Tooltip("Drag and drop your blinking icon/dot GameObject indicator here.")]
        public GameObject blinkerObject;
        public float indicatorBlinkSpeed = 0.4f;

        public Button continuebutton;

        [Header("Song Interaction Controls")]
        [Tooltip("Drag the UI Button component for 'Song1' here (Active on Paper Index 1).")]
        public Button song1Button;
        [Tooltip("Drag the UI Button component for 'Song2' here (Active on Paper Index 2).")]
        public Button song2Button;
        [Tooltip("Drag the UI Button component for 'Song3' here (Active on Paper Index 3).")]
        public Button song3Button;

        [Header("Song Audio Clips")]
        [Tooltip("Audio clip for Song 1.")]
        public AudioClip song1Audio;
        [Tooltip("Audio clip for Song 2.")]
        public AudioClip song2Audio;
        [Tooltip("Audio clip for Song 3.")]
        public AudioClip song3Audio;

        [Header("Vowel Stone Reference")]
        [Tooltip("Drag the House O VowelStone GameObject containing the VowelStoneCutscene script here.")]
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
        [Tooltip("The shared sound effect played every time an answer is correct.")]
        public AudioClip correctSFX;

        [Tooltip("The shared sound effect played every time an answer is wrong.")]
        public AudioClip wrongSFX;

        [Header("House O Word Meanings (Text Only)")]
        [TextArea(2, 4)] public string answer1MeaningText = "Definition for the first correct verb goes here...";

        [Space]
        [TextArea(2, 4)] public string answer2MeaningText = "Definition for the second correct verb goes here...";

        [Space]
        [TextArea(2, 4)] public string answer3MeaningText = "Definition for the third correct verb goes here...";

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
        private Coroutine songMonitorCoroutine; // Tracks natural completion of the short clips

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
                // Kill any tracking loop currently counting down a previous song finish
                if (songMonitorCoroutine != null)
                {
                    StopCoroutine(songMonitorCoroutine);
                }

                // Instantly cut any running audio from a previous clip to prevent layering
                CoreAudioManager.StopSFX();

                // Manually pause background music for the song track
                CoreAudioManager.PauseBGM();

                CoreAudioManager.PlaySFX(clipToPlay);

                // Start tracking how long the song is to bring back the background music when it ends
                songMonitorCoroutine = StartCoroutine(WaitForSongToEnd(clipToPlay.length));
            }
            else
            {
                Debug.LogWarning($"Song {songNumber} Audio Clip is missing in the Inspector!");
            }
        }

        /// <summary>
        /// Monitors running short clips. Fades ambient track back up if the track runs out naturally.
        /// </summary>
        private IEnumerator WaitForSongToEnd(float clipLength)
        {
            yield return new WaitForSeconds(clipLength);

            // Bring back background music automatically since it finished playing completely
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
            // Stop the countdown tracker since a user action explicitly interrupted the audio state
            if (songMonitorCoroutine != null)
            {
                StopCoroutine(songMonitorCoroutine);
                songMonitorCoroutine = null;
            }

            // Smoothly fade out the remaining clip fragments over 0.5 seconds and bring back BGM
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

                    if (correctCount == 1)
                        dialogueSequenceCoroutine = StartCoroutine(CorrectSequence(answer1MeaningText));
                    else if (correctCount == 2)
                        dialogueSequenceCoroutine = StartCoroutine(CorrectSequence(answer2MeaningText));
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

        IEnumerator CorrectSequence(string meaningText)
        {
            SetSpeakerName(isGrandma: false);

            if (correctSFX != null) CoreAudioManager.PlaySFX(correctSFX);

            yield return StartCoroutine(TypeText(correctFeedbackGreeting, showIndicator: true));
            yield return StartCoroutine(WaitUntilClick());

            if (!string.IsNullOrEmpty(meaningText))
            {
                SetSpeakerUI(showGrandma: true);
                SetSpeakerName(isGrandma: true);

                yield return StartCoroutine(TypeText(meaningText, showIndicator: true));
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

            SetSpeakerUI(showGrandma: true);
            SetSpeakerName(isGrandma: true);

            yield return StartCoroutine(TypeText(answer3MeaningText, showIndicator: true));
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
            // Clean up running countdowns if the UI component gets disabled or closed
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