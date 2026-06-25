using UnityEngine;

namespace BookChoice
{
    public class HouseUInteractionIndicator : MonoBehaviour
    {
        [Header("Distance Settings")]
        public float lookRadius = 3f;

        private Transform playerTransform;
        private GameObject visualChild;
        private bool forceHidden = false;

        void Start()
        {
            // Find the player automatically
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) playerTransform = playerObj.transform;

            // Cache the actual visual/image child so we don't deactivate the script itself
            if (transform.childCount > 0)
            {
                visualChild = transform.GetChild(0).gameObject;
                visualChild.SetActive(false);
            }
        }

        void Update()
        {
            if (forceHidden || playerTransform == null || visualChild == null) return;

            // Turn on if player is close, turn off if they walk away
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            visualChild.SetActive(distance <= lookRadius);
        }

        /// <summary>
        /// Turns off the popup permanently once the book is flying/inspected.
        /// </summary>
        public void DisablePermanently()
        {
            forceHidden = true;
            if (visualChild != null) visualChild.SetActive(false);
        }
    }
}