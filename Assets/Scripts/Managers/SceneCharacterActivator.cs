using UnityEngine;

public class SceneCharacterActivator : MonoBehaviour
{
    [Header("Hero Assets (Disabled by Default)")]
    [Tooltip("Assign the Paige_Prefab GameObject from your scene hierarchy here.")]
    public GameObject paigeCharacter; // Index 0

    [Tooltip("Assign the Penn_Prefab GameObject from your scene hierarchy here.")]
    public GameObject pennCharacter;  // Index 1

    private void Start()
    {
        GameObject activeHero = null;

        // 1. Unpack chosen layout index
        if (paigeCharacter != null) paigeCharacter.SetActive(false);
        if (pennCharacter != null) pennCharacter.SetActive(false);

        if (CharacterSelection.Selected == 0 && paigeCharacter != null) activeHero = paigeCharacter;
        else if (CharacterSelection.Selected == 1 && pennCharacter != null) activeHero = pennCharacter;
        else if (paigeCharacter != null) activeHero = paigeCharacter; // Fallback anchor configuration

        if (activeHero == null) return;

        activeHero.SetActive(true);

        // 2. Checkpoint Relocation Override (Checks if we are resuming from your Pause Menu loop or puzzle swap)
        if (CoreManager.Instance != null && CoreManager.Instance.HasSavedPosition)
        {
            // Turn off controller momentarily to prevent coordinate override jittering
            if (activeHero.TryGetComponent<CharacterController>(out CharacterController cc)) cc.enabled = false;

            activeHero.transform.position = CoreManager.Instance.SavedPlayerPosition;
            activeHero.transform.rotation = CoreManager.Instance.SavedPlayerRotation;

            if (cc != null) cc.enabled = true;
            Debug.Log($"[Activator] Player repositioned back to core save slot coordinates: {activeHero.transform.position}");
        }
    }
}