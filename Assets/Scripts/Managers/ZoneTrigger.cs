using UnityEngine;

public class ZoneTrigger : MonoBehaviour
{
    [Header("Zone Info")]
    [SerializeField] private string zoneName = "Hyrule Field";
    [SerializeField] private bool triggerOnlyOnce = true;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;

        // Checks if the object entering is either tagged "Player" OR has a CharacterController
        bool isPlayer = other.CompareTag("Player") || other.GetComponent<CharacterController>() != null;

        if (isPlayer)
        {
            if (ZoneUIManager.Instance != null)
            {
                ZoneUIManager.Instance.TriggerZoneSplash(zoneName);
            }

            if (triggerOnlyOnce)
            {
                hasTriggered = true;
            }
        }
    }
}