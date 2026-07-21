using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractableSign : MonoBehaviour
{
    [Header("Unique Identifier")]
    public string uniqueSignID;

    [Header("Distance Settings")]
    [Tooltip("Distance measured from the closest edge of the sign's Box Collider.")]
    public float interactionDistance = 3.0f;

    [Header("References")]
    public GameObject interactPromptUI;
    public Camera zoomCamera;

    [Header("Settings")]
    public float lookDuration = 3.0f;
    public AudioClip signAudioClip;

    private bool isInteracting = false;
    private bool playerIsClose = false;

    private Charactercontroller playerController;
    private BoxCollider boxCollider;

    void Start()
    {
        if (interactPromptUI != null) interactPromptUI.SetActive(false);
        if (zoomCamera != null) zoomCamera.gameObject.SetActive(false);

        if (string.IsNullOrEmpty(uniqueSignID))
        {
            uniqueSignID = gameObject.name + "_" + transform.position.GetHashCode();
        }

        boxCollider = GetComponent<BoxCollider>();

        // Try to find the player immediately, but don't throw an error if missing yet
        FindPlayerCharacter();
    }

    // New helper method to locate the active character
    private void FindPlayerCharacter()
    {
        playerController = Object.FindAnyObjectByType<Charactercontroller>();
    }

    void Update()
    {
        // If the player wasn't found at Start, keep looking until they are selected/enabled
        if (playerController == null)
        {
            FindPlayerCharacter();
            return; // Skip the rest of the frame until we have a player
        }

        if (isInteracting) return;

        float currentDistance = 0f;

        if (boxCollider != null)
        {
            Vector3 closestPointOnCollider = boxCollider.ClosestPoint(playerController.transform.position);
            currentDistance = Vector3.Distance(closestPointOnCollider, playerController.transform.position);
        }
        else
        {
            currentDistance = Vector3.Distance(transform.position, playerController.transform.position);
        }

        if (currentDistance <= interactionDistance)
        {
            if (!playerIsClose)
            {
                playerIsClose = true;
                if (interactPromptUI != null) interactPromptUI.SetActive(true);
            }

            if (playerController.canControl)
            {
                if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
                {
                    StartCoroutine(InteractRoutine());
                }
            }
        }
        else
        {
            if (playerIsClose)
            {
                playerIsClose = false;
                if (interactPromptUI != null) interactPromptUI.SetActive(false);
            }
        }
    }

    private IEnumerator InteractRoutine()
    {
        isInteracting = true;
        playerIsClose = false;
        if (interactPromptUI != null) interactPromptUI.SetActive(false);

        playerController.canControl = false;

        // --- TARGET VISUAL MESH DIRECTLY ---
        // Check if the player controller has the mesh assigned, then toggle its active state
        if (playerController.characterVisualMesh != null)
        {
            playerController.characterVisualMesh.gameObject.SetActive(false);
        }

        // Swap Cameras
        Camera mainCam = Camera.main;
        if (zoomCamera != null && mainCam != null)
        {
            zoomCamera.gameObject.SetActive(true);
            mainCam.gameObject.SetActive(false);
        }

        // Audio & Discovery Log Setup
        CoreAudioManager.FadeOutBGM(0.4f);
        Debug.Log($"[Sign Manager] The sign '{uniqueSignID}' has been discovered!");

        if (SignDiscoveryManager.Instance != null)
        {
            SignDiscoveryManager.Instance.ReportSignDiscovered(uniqueSignID);
        }
        else
        {
            Debug.LogError("[Sign System] Missing SignDiscoveryManager instance in scene!");
        }

        yield return new WaitForSeconds(0.5f);

        if (signAudioClip != null)
        {
            CoreAudioManager.PlaySFX(signAudioClip);
        }

        float remainingTime = Mathf.Max(0f, lookDuration - 0.5f);
        yield return new WaitForSeconds(remainingTime);

        CoreAudioManager.StopSFX();
        CoreAudioManager.FadeInBGM(1.0f, 0.5f);

        // Restore Cameras
        if (zoomCamera != null) zoomCamera.gameObject.SetActive(false);
        if (mainCam != null) mainCam.gameObject.SetActive(true);

        // --- RESTORE VISUAL MESH ---
        if (playerController.characterVisualMesh != null)
        {
            playerController.characterVisualMesh.gameObject.SetActive(true);
        }

        playerController.canControl = true;
        isInteracting = false;
    }

    private void OnDrawGizmosSelected()
    {
        BoxCollider box = GetComponent<BoxCollider>();
        if (box != null)
        {
            Gizmos.color = Color.cyan;
            Vector3 extendedSize = box.size + new Vector3(interactionDistance * 2, interactionDistance * 2, interactionDistance * 2);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(box.center, extendedSize);
        }
        else
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, interactionDistance);
        }
    }
}