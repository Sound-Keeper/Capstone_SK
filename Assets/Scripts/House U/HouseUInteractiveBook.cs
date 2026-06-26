using System.Collections;
using UnityEngine;

namespace BookChoice
{
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