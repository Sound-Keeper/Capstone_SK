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
            FindPlayer();

            if (transform.childCount > 0)
            {
                visualChild = transform.GetChild(0).gameObject;
                visualChild.SetActive(false);
            }
        }

        void Update()
        {
            if (forceHidden || visualChild == null) return;

            // NEW: If we lost or missed the player, keep searching until we find them
            if (playerTransform == null)
            {
                FindPlayer();
                if (playerTransform == null) return; // Skip this frame if still null
            }

            // Turn on if player is close, turn off if they walk away
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            visualChild.SetActive(distance <= lookRadius);
        }

        private void FindPlayer()
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }
        }

        public void DisablePermanently()
        {
            forceHidden = true;
            if (visualChild != null) visualChild.SetActive(false);
        }
    }
}