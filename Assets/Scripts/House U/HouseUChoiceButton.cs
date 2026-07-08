using System.Collections;
using UnityEngine;

namespace BookChoice
{
    public class HouseUChoiceButton : MonoBehaviour
    {
        [Header("Letter Settings")]
        public string choiceLetter = "U";

        [Header("Animation Settings")]
        public float shakeDuration = 0.5f;
        public float shakeMagnitude = 0.1f;
        public float flyDuration = 1.2f;

        private HouseUInteractiveBook parentBook;
        private bool isProcessing = false;
        private Vector3 originalLocalPosition;

        void Start()
        {
            originalLocalPosition = transform.localPosition;

            // Automatically find the book this choice belongs to in the hierarchy
            parentBook = GetComponentInParent<HouseUInteractiveBook>();
        }

        public void SelectChoice()
        {
            // Lock interaction if it's already shaking or flying away
            if (isProcessing) return;

            if (parentBook != null)
            {
                // Check if this letter matches the correct letter string on the book
                if (choiceLetter.ToUpper() == parentBook.correctLetter.ToUpper())
                {
                    StartCoroutine(FlyToSignRoutine());
                }
                else
                {
                    StartCoroutine(ShakeRoutine());
                }
            }
        }

        private IEnumerator ShakeRoutine()
        {
            isProcessing = true;
            float elapsed = 0f;

            while (elapsed < shakeDuration)
            {
                elapsed += Time.deltaTime;

                // Calculate a random offset on the X and Y plane relative to its starting point
                float randomX = Random.Range(-1f, 1f) * shakeMagnitude;
                float randomY = Random.Range(-1f, 1f) * shakeMagnitude;

                transform.localPosition = originalLocalPosition + new Vector3(randomX, randomY, 0f);
                yield return null;
            }

            // Snap back cleanly to original local layout position
            transform.localPosition = originalLocalPosition;
            isProcessing = false;
        }

        private IEnumerator FlyToSignRoutine()
        {
            isProcessing = true;

            // --- CHANGED: Get the target slot directly from the parent book script component ---
            Transform targetSignSlot = null;
            if (parentBook != null)
            {
                targetSignSlot = parentBook.customSignTargetSlot;
            }

            // Fallback safety safety net: if forgot to assign in inspector, check manager index
            if (targetSignSlot == null)
            {
                targetSignSlot = HouseUPuzzleManager.Instance.GetActiveSignTargetSlot();
            }

            Vector3 startWorldPos = transform.position;
            Quaternion startWorldRot = transform.rotation;

            float elapsed = 0f;
            while (elapsed < flyDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / flyDuration);

                if (targetSignSlot != null)
                {
                    transform.position = Vector3.Lerp(startWorldPos, targetSignSlot.position, t);
                    transform.rotation = Quaternion.Lerp(startWorldRot, targetSignSlot.rotation, t);
                }
                yield return null;
            }

            // Ensure precise placement on final frame
            if (targetSignSlot != null)
            {
                transform.position = targetSignSlot.position;
                transform.rotation = targetSignSlot.rotation;
                transform.localScale = targetSignSlot.localScale;

                // Make the letter physically part of the world sign mesh canvas now
                transform.SetParent(targetSignSlot);
            }

            // Let the book clean itself up and fly back onto the shelf
            if (parentBook != null)
            {
                parentBook.CompleteBook(targetSignSlot);
            }
        }
    }
}