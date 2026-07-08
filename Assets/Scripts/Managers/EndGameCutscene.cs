using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndGameCutscene : MonoBehaviour
{
    public static EndGameCutscene Instance { get; private set; }

    [Header("Environment Controls")]
    [Tooltip("Drag your sunny/default Day Skybox Material here.")]
    public Material daySkybox;
    [Tooltip("Drag the scene's primary Directional Light here.")]
    public Light directionalLight;
    [Tooltip("The divine intervention spotlight sitting over the fountain.")]
    public Light fountainSpotlight;

    [Header("Cinematic Cameras")]
    [Tooltip("A dedicated camera focused on the fountain spotlight intervention.")]
    public Camera spotlightCamera;
    [Tooltip("Assign the 5 Vowel Stone cameras in order here (Element 0 to 4).")]
    public List<Camera> vowelStoneCameras = new List<Camera>();

    [Header("Inspector Editable Dialogues")]
    [Tooltip("The lines of the ritual chant spoken by the Player.")]
    public List<string> playerChantLines = new List<string> {
        "To keep Word Valley bright and loud...",
        "I AIM high...",
        "and EVERYONE can try...",
        "my voice is my OWN...",
        "facing the great UNKNOWN."
    };

    [Header("Post-Transformation Dialogue")]
    [Tooltip("Pip's celebration text after the daylight transformation happens.")]
    public List<string> pipCelebrationLines = new List<string> {
        "Look! The mush-mush curse is breaking! The sky... the light is back!",
        "Fantastic work, {player}! You saved the vowels, and you saved our home!",
        "You are officially the greatest Sound Keeper Word Valley has ever seen!"
    };

    private bool ritualSequenceStarted = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); }

        if (fountainSpotlight != null)
        {
            fountainSpotlight.gameObject.SetActive(true); // Keep active but set intensity to 0
            fountainSpotlight.intensity = 0f;
        }
        if (spotlightCamera != null) spotlightCamera.gameObject.SetActive(false);
    }

    // Called when the player interacts with Pip at the fountain after all puzzles are solved
    public void StartFountainRitual()
    {
        if (ritualSequenceStarted) return;
        ritualSequenceStarted = true;

        StartCoroutine(RitualSequenceRoutine());
    }

    private IEnumerator RitualSequenceRoutine()
    {
        yield return new WaitForSeconds(0.2f);

        // ============================================================
        // PHASE 1: The Ritual Chant & Vowel Stone Camera Cuts
        // ============================================================
        for (int i = 0; i < playerChantLines.Count; i++)
        {
            // Turn off previous camera setups
            if (i == 0)
            {
                // Turn off standard dialogue cameras if applicable
            }
            else
            {
                if (i - 1 < vowelStoneCameras.Count && vowelStoneCameras[i - 1] != null)
                {
                    vowelStoneCameras[i - 1].gameObject.SetActive(false);
                }
            }

            // Turn on current Vowel Stone camera
            if (i < vowelStoneCameras.Count && vowelStoneCameras[i] != null)
            {
                vowelStoneCameras[i].gameObject.SetActive(true);
            }

            DialogueLine singleChantLine = new DialogueLine
            {
                speaker = Speaker.Player,
                text = playerChantLines[i]
            };

            bool currentLineDone = false;
            DialogueManager.Instance.StartDialogue(
                new DialogueLine[] { singleChantLine },
                CharacterSelection.SelectedName,
                null,
                DialogueManager.Instance.pennPortrait,
                null,
                () => currentLineDone = true
            );

            while (!currentLineDone) yield return null;
        }

        // Clean up the last Vowel Stone camera
        if (vowelStoneCameras.Count > 0 && vowelStoneCameras[vowelStoneCameras.Count - 1] != null)
        {
            vowelStoneCameras[vowelStoneCameras.Count - 1].gameObject.SetActive(false);
        }

        // ============================================================
        // PHASE 2: Divine Spotlight Camera & Intensity Ramp (0 -> 8000 over 3s)
        // ============================================================
        if (spotlightCamera != null) spotlightCamera.gameObject.SetActive(true);

        if (fountainSpotlight != null)
        {
            float duration = 3.0f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                // Linearly interpolate intensity from 0 to 8000
                fountainSpotlight.intensity = Mathf.Lerp(0f, 8000f, elapsed / duration);
                yield return null;
            }
            fountainSpotlight.intensity = 8000f;
        }

        // Trigger environmental daylight changes right at the peak of the light beam intensity
        TriggerDaylightTransformation();
        yield return new WaitForSeconds(1.0f);

        // Turn off spotlight and its dedicated camera
        if (fountainSpotlight != null) fountainSpotlight.intensity = 0f;
        if (spotlightCamera != null) spotlightCamera.gameObject.SetActive(false);

        // Bring back the main cutscene camera view for Pip's reaction
        if (DialogueManager.Instance.pipCutsceneCamera != null)
        {
            DialogueManager.Instance.pipCutsceneCamera.gameObject.SetActive(true);
        }

        // ============================================================
        // PHASE 3: Celebration dialogue post-transformation
        // ============================================================
        bool celebrationDone = false;

        // --- FIXED PORTRAIT REFERENCE FALLBACK ---
        Sprite finalPipFace = null;
        PipInteraction activePipScript = FindFirstObjectByType<PipInteraction>();
        if (activePipScript != null) finalPipFace = activePipScript.pipPortrait;
        if (finalPipFace == null && DialogueManager.Instance != null) finalPipFace = DialogueManager.Instance.pipIntroPortrait;

        DialogueManager.Instance.StartDialogue("Pip", pipCelebrationLines.ToArray(), finalPipFace);
        DialogueManager.Instance.OnDialogueEnd = () => celebrationDone = true;

        while (!celebrationDone) yield return null;

        // Clean up cutscene cameras back to standard player view
        if (DialogueManager.Instance.pipCutsceneCamera != null) DialogueManager.Instance.pipCutsceneCamera.gameObject.SetActive(false);
        if (Camera.main != null) Camera.main.gameObject.SetActive(true);

        DialogueManager.Instance.SetPlayerControlState(true);
        Debug.Log("[EndGameCutscene] Game Complete! World is saved.");
    }

    private void TriggerDaylightTransformation()
    {
        if (daySkybox != null) RenderSettings.skybox = daySkybox;
        if (directionalLight != null) directionalLight.gameObject.SetActive(true);
        RenderSettings.fog = false;
        DynamicGI.UpdateEnvironment();
    }
}