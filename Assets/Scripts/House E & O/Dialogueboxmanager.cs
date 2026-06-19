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
        [Header("UI References")]
        public TextMeshProUGUI dialogueText;
        public GameObject pip;
        public GameObject judge;
        public Button continuebutton;

        [Header("Messages")]
        public string idleMessage = "Choose the correct verb.";
        public string correctMessage = "That's correct! Great job!";
        public string wrongMessage = "Oops! Try again.";

        [Header("Extra Messages")]
        public string nextStepMessage = "Now flip the page.";
        public string finalMessage = "Good job, you have finished my puzzle.";

        [Header("Timing")]
        public float wrongDisplayDuration = 1.5f;

        [Header("Typing Settings")]
        [Tooltip("Time between each character")]
        public float typingSpeed = 0.05f;

        [Header("Book Reference")]
        public BookPro book;

        private Coroutine typingCoroutine;
        private bool hasShownCorrectOnce = false;
        public bool puzzleCompleted = false;
        private int correctCount = 0;
        public int totalRequiredCorrect = 2;

        void Start()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            judge.gameObject.SetActive(true);
            StartTyping(idleMessage);
        }


        public void ShowFeedback(bool isCorrect, Action onWrongFinished = null)
        {
            StopAllCoroutines();
            judge.gameObject.SetActive(false);

            if (isCorrect)
            {
                pip.gameObject.SetActive(true);

                correctCount++;

                // FINAL CORRECT (page 2)
                if (correctCount >= totalRequiredCorrect)
                {
                    puzzleCompleted = true;
                    StartCoroutine(FinalSequence());
                }
                else
                {
                    // First correct (page 1)
                    if (!hasShownCorrectOnce)
                    {
                        hasShownCorrectOnce = true;
                        StartCoroutine(CorrectSequence());
                    }
                    else
                    {
                        StartTyping(correctMessage);
                    }
                }
            }
            else
            {
                pip.gameObject.SetActive(true);

                StartTyping(wrongMessage);

                StartCoroutine(RevertToIdleAfterDelay(wrongDisplayDuration, onWrongFinished));
            }
        }

        IEnumerator FinalSequence()
        {
            // Hide pip, show judge
            pip.gameObject.SetActive(false);
            judge.gameObject.SetActive(true);

            // Judge speaks final message
            yield return StartCoroutine(TypeText(finalMessage));

            yield return new WaitForSeconds(1f);

            // Optional: return to idle or stay finished
            StartTyping("...");
            continuebutton.gameObject.SetActive(true);
        }
        IEnumerator CorrectSequence()
        {
            // 1. Show correct
            yield return StartCoroutine(TypeText(correctMessage));

            yield return new WaitForSeconds(1f);

            // 2. Tell to flip
            yield return StartCoroutine(TypeText(nextStepMessage));

            // 3. WAIT FOR PAGE FLIP
            bool flipped = false;

            UnityEngine.Events.UnityAction flipAction = () => { flipped = true; };

            if (book != null)
                book.OnFlip.AddListener(flipAction);

            yield return new WaitUntil(() => flipped);

            if (book != null)
                book.OnFlip.RemoveListener(flipAction);

            // 4. Reset UI
            pip.gameObject.SetActive(false);
            judge.gameObject.SetActive(true);

            StartTyping(idleMessage);
        }

        IEnumerator RevertToIdleAfterDelay(float delay, Action callback)
        {
            yield return new WaitForSeconds(delay);
            pip.gameObject.SetActive(false);
            judge.gameObject.SetActive(true);
            StartTyping(idleMessage);
            callback?.Invoke();
        }

        void StartTyping(string message)
        {
            // Stop previous typing if running
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            typingCoroutine = StartCoroutine(TypeText(message));
        }

        IEnumerator TypeText(string message)
        {
            dialogueText.text = "";

            foreach (char letter in message)
            {
                dialogueText.text += letter;
                yield return new WaitForSeconds(typingSpeed);
            }
        }
    }
}