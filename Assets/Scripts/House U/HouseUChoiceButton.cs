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

        [Header("Audio Settings")]
        [Tooltip("Drag the hover sound effect for this choice letter here.")]
        public AudioClip hoverSFX;
        [Tooltip("Minimum time (in seconds) that must pass before this choice can trigger its hover sound again.")]
        public float hoverCooldown = 0.6f;

        private HouseUInteractiveBook parentBook;
        private bool isProcessing = false;
        private Vector3 originalLocalPosition;
        private float nextPlayTime = 0f;

        void Start()
        {
            originalLocalPosition = transform.localPosition;
            parentBook = GetComponentInParent<HouseUInteractiveBook>();
        }

        public AudioClip GetHoverSFX()
        {
            // If the cooldown hasn't finished yet, return null (no sound)
            if (Time.time < nextPlayTime)
            {
                return null;
            }

            // Otherwise, update the timestamp and return the clip
            nextPlayTime = Time.time + hoverCooldown;
            return hoverSFX;
        }

        public void SelectChoice()
        {
            if (isProcessing) return;

            if (parentBook != null)
            {
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
                float randomX = Random.Range(-1f, 1f) * shakeMagnitude;
                float randomY = Random.Range(-1f, 1f) * shakeMagnitude;

                transform.localPosition = originalLocalPosition + new Vector3(randomX, randomY, 0f);
                yield return null;
            }

            transform.localPosition = originalLocalPosition;
            isProcessing = false;
        }

        private IEnumerator FlyToSignRoutine()
        {
            isProcessing = true;
            Transform targetSignSlot = null;

            if (parentBook != null)
            {
                targetSignSlot = parentBook.customSignTargetSlot;
            }

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

            if (targetSignSlot != null)
            {
                transform.position = targetSignSlot.position;
                transform.rotation = targetSignSlot.rotation;
                transform.localScale = targetSignSlot.localScale;
                transform.SetParent(targetSignSlot);
            }

            if (parentBook != null)
            {
                parentBook.CompleteBook(targetSignSlot);
            }
        }
    }
}