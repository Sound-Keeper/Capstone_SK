using System.Collections;
using UnityEngine;

namespace BookChoice
{
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
        [Header("Glow Outline Settings")]
        [Tooltip("Drag the Outline script component attached to this book's mesh here.")]
        public MonoBehaviour outlineComponent;

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

        [Header("Book Movement SFX")]
        [Tooltip("Sound played when the book moves out toward the player.")]
        public AudioClip bookOpenSFX;
        [Tooltip("Sound played when the book floats back onto the shelf.")]
        public AudioClip bookCloseSFX;

        [Header("House U Word Meaning Settings")]
        [Tooltip("Add as many words and definitions as this specific book needs!")]
        public HouseUWordDefinition[] definitions;

        private Vector3 originalPosition;
        private Quaternion originalRotation;
        private Vector3 originalScale;
        private bool isInspecting = false;
        private bool isSolved = false;

        void Start()
        {
            originalPosition = transform.position;
            originalRotation = transform.rotation;
            originalScale = transform.localScale;

            if (choicesContainer != null)
                choicesContainer.SetActive(false);
        }

        public void SetOutlineHover(bool isHovered)
        {
            if (isSolved || isInspecting)
            {
                if (outlineComponent != null) outlineComponent.enabled = false;
                return;
            }

            if (outlineComponent != null)
            {
                outlineComponent.enabled = isHovered;
            }
        }

        public void StartInspectionViaRaycast()
        {
            if (isSolved || isInspecting) return;

            isInspecting = true;

            SetOutlineHover(false);

            // --- AUDIO TRIGGER: Play open sound when floating out ---
            if (bookOpenSFX != null)
            {
                CoreAudioManager.PlaySFX(bookOpenSFX);
            }

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

            if (DialogueManager.Instance != null && definitions != null && definitions.Length > 0)
            {
                StartCoroutine(PlaySequentialDefinitionsNoPortraits());
            }
            else
            {
                ReturnBookToShelf();
            }
        }

        private IEnumerator PlaySequentialDefinitionsNoPortraits()
        {
            DialogueManager.Instance.SetPlayerControlState(false);
            CoreAudioManager.FadeOutBGM(1.0f);

            for (int i = 0; i < definitions.Length; i++)
            {
                bool isCurrentLineActive = true;
                HouseUWordDefinition currentDef = definitions[i];

                DialogueManager.Instance.StartDialogueWithoutPortraits(
                    currentDef.wordName,
                    currentDef.wordMeaning,
                    () => { isCurrentLineActive = false; }
                );

                yield return new WaitForSeconds(1f);

                if (currentDef.definitionSFX != null)
                {
                    CoreAudioManager.PlaySFX(currentDef.definitionSFX);
                }

                while (isCurrentLineActive)
                {
                    yield return null;
                }

                yield return new WaitForSeconds(0.1f);
            }

            CoreAudioManager.FadeInBGM(1.0f, 1.0f);
            DialogueManager.Instance.SetPlayerControlState(true);
            ReturnBookToShelf();
        }

        private void ReturnBookToShelf()
        {
            // --- AUDIO TRIGGER: Play close sound when returning to shelf ---
            if (bookCloseSFX != null)
            {
                CoreAudioManager.PlaySFX(bookCloseSFX);
            }

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
                transform.localScale = Vector3.Lerp(startScale, targetScale, t);
                yield return null;
            }

            transform.position = targetPos;
            transform.rotation = targetRot;
            transform.localScale = targetScale;
            onComplete?.Invoke();
        }
    }
}   