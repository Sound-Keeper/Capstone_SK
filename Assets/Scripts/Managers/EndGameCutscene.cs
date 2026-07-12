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
    public ParticleSystem transformationSmoke;

    [Header("Cinematic Cameras")]
    [Tooltip("A dedicated camera focused on the fountain spotlight intervention.")]
    public Camera spotlightCamera;
    [Tooltip("Assign the 5 Vowel Stone cameras in order here (Element 0 to 4).")]
    public List<Camera> vowelStoneCameras = new List<Camera>();

    [Header("Pip Transformation Setup")]
    [Tooltip("The main flying Pip GameObject.")]
    public GameObject pipGameObject;
    [Tooltip("The regular owl-like visual object inside Pip (turned ON by default).")]
    public GameObject pipOwlVisual;
    [Tooltip("The human Archmage prefab model inside Pip (turned OFF by default).")]
    public GameObject pipArchmageVisual;
    [Tooltip("Drag the Archmage's UI portrait sprite here.")]
    public Sprite archmagePortrait;

    [Header("Miss Spell Disappearance Setup")]
    [Tooltip("Drag the Miss Spell GameObject/Prefab instance here.")]
    public GameObject missSpellGameObject;
    [Tooltip("A dedicated camera focused on Miss Spell during her defeat.")]
    public Camera missSpellCamera;
    [Tooltip("The particle system that plays when Miss Spell disappears.")]
    public ParticleSystem missSpellDisappearParticles;
    [Tooltip("How long to stay on Miss Spell's camera to watch the particles blow away before switching to the Archmage.")]
    public float missSpellCutsceneDelay = 2.0f;

    // ============================================================
    // NEW: MISS SPELL LIGHT CONTROLS
    // ============================================================
    [Header("Miss Spell Light Controls")]
    [Tooltip("First light to change color during Miss Spell's vanish sequence.")]
    public Light missSpellLight1;
    [Tooltip("Second light to change color during Miss Spell's vanish sequence.")]
    public Light missSpellLight2;
    [Tooltip("The color the lights will change to when Miss Spell vanishes.")]
    public Color missSpellTargetColor = Color.magenta;

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
    [Tooltip("Archmage's celebration text after Miss Spell vanishes.")]
    public List<string> archmageCelebrationLines = new List<string> {
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
            fountainSpotlight.gameObject.SetActive(true);
            fountainSpotlight.intensity = 0f;
        }
        if (spotlightCamera != null) spotlightCamera.gameObject.SetActive(false);
        if (missSpellCamera != null) missSpellCamera.gameObject.SetActive(false);
    }

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
            if (i - 1 < vowelStoneCameras.Count && i > 0 && vowelStoneCameras[i - 1] != null)
            {
                vowelStoneCameras[i - 1].gameObject.SetActive(false);
            }

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

        if (vowelStoneCameras.Count > 0 && vowelStoneCameras[vowelStoneCameras.Count - 1] != null)
        {
            vowelStoneCameras[vowelStoneCameras.Count - 1].gameObject.SetActive(false);
        }

        // ============================================================
        // PHASE 2: Divine Spotlight Camera & Intensity Ramp (Divine Light)
        // ============================================================
        if (spotlightCamera != null) spotlightCamera.gameObject.SetActive(true);

        if (fountainSpotlight != null)
        {
            float duration = 5.0f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                fountainSpotlight.intensity = Mathf.Lerp(0f, 8000f, elapsed / duration);
                yield return null;
            }
            fountainSpotlight.intensity = 8000f;
        }

        // ============================================================
        // PHASE 3: Pip turns into Archmage + Daylight Transformation
        // ============================================================
        ExecutePipHumanTransformation();
        TriggerDaylightTransformation();
        yield return new WaitForSeconds(1.0f);

        if (fountainSpotlight != null) fountainSpotlight.intensity = 0f;
        if (spotlightCamera != null) spotlightCamera.gameObject.SetActive(false);

        // ============================================================
        // PHASE 4: Miss Spell Vanish Sequence (With Camera & Light Change)
        // ============================================================
        // 1. Turn on Miss Spell's camera
        if (missSpellCamera != null) missSpellCamera.gameObject.SetActive(true);

        // 2. NEW: Change the color of the two designated lights
        if (missSpellLight1 != null) missSpellLight1.color = missSpellTargetColor;
        if (missSpellLight2 != null) missSpellLight2.color = missSpellTargetColor;

        if (missSpellGameObject != null)
        {
            // 3. Play particles
            if (missSpellDisappearParticles != null)
            {
                missSpellDisappearParticles.transform.position = missSpellGameObject.transform.position;
                missSpellDisappearParticles.Play();
            }

            // 4. Brief hold so the camera sees her right as particles flash
            yield return new WaitForSeconds(0.1f);

            // 5. Turn Miss Spell off
            missSpellGameObject.SetActive(false);
            Debug.Log("[EndGameCutscene] Miss Spell vanished! Lights updated to new color.");

            // Hold camera frame for the remainder of the timer
            yield return new WaitForSeconds(Mathf.Max(0.1f, missSpellCutsceneDelay - 0.1f));
        }
        else
        {
            yield return new WaitForSeconds(missSpellCutsceneDelay);
        }

        // Turn off Miss Spell's camera before shifting to dialogue
        if (missSpellCamera != null) missSpellCamera.gameObject.SetActive(false);

        // Bring back the cutscene camera view focused on the Archmage
        if (DialogueManager.Instance.pipCutsceneCamera != null)
        {
            DialogueManager.Instance.pipCutsceneCamera.gameObject.SetActive(true);
        }

        // ============================================================
        // PHASE 5: Archmage Talking (Celebration Dialogue)
        // ============================================================
        bool celebrationDone = false;

        Sprite finalSpeakerFace = archmagePortrait;
        if (finalSpeakerFace == null && DialogueManager.Instance != null)
            finalSpeakerFace = DialogueManager.Instance.pipIntroPortrait;

        DialogueManager.Instance.StartDialogue("Archmage", archmageCelebrationLines.ToArray(), finalSpeakerFace);
        DialogueManager.Instance.OnDialogueEnd = () => celebrationDone = true;

        while (!celebrationDone) yield return null;

        // ============================================================
        // PHASE 6: Clean up and Restore Control
        // ============================================================
        if (DialogueManager.Instance.pipCutsceneCamera != null) DialogueManager.Instance.pipCutsceneCamera.gameObject.SetActive(false);
        if (Camera.main != null) Camera.main.gameObject.SetActive(true);

        DialogueManager.Instance.SetPlayerControlState(true);
        Debug.Log("[EndGameCutscene] Game Complete! World is saved.");
    }

    private void ExecutePipHumanTransformation()
    {
        if (transformationSmoke != null)
        {
            transformationSmoke.Play();
        }

        if (pipOwlVisual != null) pipOwlVisual.SetActive(false);
        if (pipArchmageVisual != null) pipArchmageVisual.SetActive(true);

        if (pipGameObject != null)
        {
            PipFly pipFlightSystem = pipGameObject.GetComponent<PipFly>();
            if (pipFlightSystem != null)
            {
                pipFlightSystem.floatHeight = 1.1f;
                pipFlightSystem.floatAmplitude = 0f;
            }
        }
    }

    private void TriggerDaylightTransformation()
    {
        if (daySkybox != null) RenderSettings.skybox = daySkybox;
        if (directionalLight != null) directionalLight.gameObject.SetActive(true);
        RenderSettings.fog = false;
        DynamicGI.UpdateEnvironment();
    }
}