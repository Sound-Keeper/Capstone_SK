using System.Collections;
using UnityEngine;

namespace BookChoice
{
    // The structured data class accessible directly inside the book's Inspector
    [System.Serializable]
    public class HouseUWordDefinition
    {
        [Tooltip("The word name displayed in the name text slot (e.g., 'MUG').")]
        public string wordName;

        [TextArea(3, 5)]
        [Tooltip("The actual meaning/definition text for this word.")]
        public string wordMeaning;

        [Header("Audio")]
        [Tooltip("Drag the custom audio/voiceover clip for this specific word here (Optional).")]
        public AudioClip definitionSFX;
    }

    public class HouseUInteractiveBook : MonoBehaviour
    {
        [Header("UI Indicator Reference")]
        [Tooltip("Drag the indicator canvas asset belonging to this specific book here.")]
        public HouseUInteractionIndicator proximityIndicator;

        [Header("Animation Positions & Scale")]
        [Tooltip("Create an empty child or target transform in front of the shelf where the book floats.")]
        public Transform floatingInspectTarget;
        public float floatDuration = 1.0f;

        [Tooltip("How big should the book become when inspecting it? (e.g., 2 = double original size)")]
        public Vector3 inspectScale = new Vector3(2f, 2f, 2f);

        [Header("Puzzle Configuration")]
        [Tooltip("The parent GameObject containing the 3 choice paper assets for this book.")]
        public GameObject choicesContainer;
        [Tooltip("The correct character string needed for this shelf (e.g., 'U').")]
        public string correctLetter = "U";
        [Tooltip("Drag the specific sign/mesh target slot where THIS book's letter should fly to.")]
        public Transform customSignTargetSlot;

        [Header("House U Word Meaning Settings")]
        [Tooltip("Add as many words and definitions as this specific book needs!")]
        public HouseUWordDefinition[] definitions;

        private Vector3 originalPosition;
        private Quaternion originalRotation;
        private Vector3 originalScale; // Tracks your initial bookshelf scale layout
        private bool isInspecting = false;
        private bool isSolved = false;

        void Start()
        {
            originalPosition = transform.position;
            originalRotation = transform.rotation;
            originalScale = transform.localScale; // Remember how big it was originally

            if (choicesContainer != null)
                choicesContainer.SetActive(false);
        }

        public void StartInspectionViaRaycast()
        {
            if (isSolved || isInspecting) return;

            isInspecting = true;

            if (proximityIndicator != null)
            {
                proximityIndicator.DisablePermanently();
            }

            // Float to target position while scaling UP to inspectScale
            StartCoroutine(FloatToTarget(floatingInspectTarget.position, floatingInspectTarget.rotation, inspectScale, () =>
            {
                if (choicesContainer != null)
                    choicesContainer.SetActive(true);
            }));
        }

        public void CompleteBook(Transform targetSignPosition)
        {
            isSolved = true;
            isInspecting = false;

            if (choicesContainer != null)
                choicesContainer.SetActive(false);

            // If definitions are assigned, loop through them portraitless before flying back
            if (DialogueManager.Instance != null && definitions != null && definitions.Length > 0)
            {
                StartCoroutine(PlaySequentialDefinitionsNoPortraits());
            }
            else
            {
                // Fallback direct return if no definitions are added
                ReturnBookToShelf();
            }
        }

        private IEnumerator PlaySequentialDefinitionsNoPortraits()
        {
            // 1. Block player controls safely while talking
            DialogueManager.Instance.SetPlayerControlState(false); 

            // 2. Fade out BGM at the start of the sequence
            CoreAudioManager.FadeOutBGM(1.0f);

            // 3. Loop through every single definition assigned to this book in the inspector
            for (int i = 0; i < definitions.Length; i++)
            {
                bool isCurrentLineActive = true;
                HouseUWordDefinition currentDef = definitions[i];

                // Force portraits off, update custom text fields, and type out the message
                DialogueManager.Instance.StartDialogueWithoutPortraits(
                    currentDef.wordName,
                    currentDef.wordMeaning,
                    () => { isCurrentLineActive = false; } // Callback moves to next word on interaction input
                );

                // --- Wait 0.5 seconds before playing the definition audio track ---
                yield return new WaitForSeconds(1f); 

                // Play the unique sound effect matched with this specific word
                if (currentDef.definitionSFX != null)
                {
                    CoreAudioManager.PlaySFX(currentDef.definitionSFX);
                }

                // Wait until player presses input to advance the current text frame
                while (isCurrentLineActive)
                {
                    yield return null;
                }

                // Short grace delay before pulling up the next slot
                yield return new WaitForSeconds(0.1f); 
            }

            // 4. Definitions complete! Fade BGM back in, turn controls on, and return book
            CoreAudioManager.FadeInBGM(1.0f, 1.0f);
            DialogueManager.Instance.SetPlayerControlState(true);
            ReturnBookToShelf(); 
        }

        private void ReturnBookToShelf()
        {
            // Float back to shelf position while scaling DOWN to originalScale
            StartCoroutine(FloatToTarget(originalPosition, originalRotation, originalScale, () =>
            {
                Debug.Log("Book securely returned to shelf.");
                HouseUPuzzleManager.Instance.OnShelfCompleted();
            }));
        }

        IEnumerator FloatToTarget(Vector3 targetPos, Quaternion targetRot, Vector3 targetScale, System.Action onComplete)
        {
            float elapsed = 0f;
            Vector3 startPos = transform.position;
            Quaternion startRot = transform.rotation;
            Vector3 startScale = transform.localScale;

            while (elapsed < floatDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / floatDuration);

                transform.position = Vector3.Lerp(startPos, targetPos, t);
                transform.rotation = Quaternion.Lerp(startRot, targetRot, t);
                transform.localScale = Vector3.Lerp(startScale, targetScale, t); // Smoothly scales over time
                yield return null;
            }

            transform.position = targetPos;
            transform.rotation = targetRot;
            transform.localScale = targetScale;
            onComplete?.Invoke();
        }
    }
}