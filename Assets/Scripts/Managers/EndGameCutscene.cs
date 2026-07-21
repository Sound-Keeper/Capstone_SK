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

    [Header("Miss Spell Light Controls")]
    [Tooltip("First light to change color during Miss Spell's vanish sequence.")]
    public Light missSpellLight1;
    [Tooltip("Second light to change color during Miss Spell's vanish sequence.")]
    public Light missSpellLight2;
    [Tooltip("The color the lights will change to when Miss Spell vanishes.")]
    public Color missSpellTargetColor = Color.magenta;

    [Header("Audio Customization Setup")]
    [Tooltip("The audio clip track played when the divine intervention spotlight hits the fountain.")]
    public AudioClip divineLightSFX;
    [Tooltip("Add the 5 audio tracks for the ritual chant lines in order here (Element 0 to 4).")]
    public AudioClip fullChantAudioTrack;
    [Tooltip("How long (in seconds) the background music should take to fade away at the start.")]
    public float bgmFadeOutTime = 1.0f;

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

    void Start()
    {
        // If loaded from Main Menu after completion, re-apply visual world state
        if (PlayerPrefs.GetInt("IsEndGameCompleted", 0) == 1)
        {
            ApplyPostCutsceneWorldState();
        }
    }

    public void ApplyPostCutsceneWorldState()
    {
        TriggerDaylightTransformation(); // Restores skybox and directional light

        if (pipOwlVisual != null) pipOwlVisual.SetActive(false); 
        if (pipArchmageVisual != null) pipArchmageVisual.SetActive(true); 

        if (missSpellGameObject != null) missSpellGameObject.SetActive(false); 
        if (fountainSpotlight != null) fountainSpotlight.intensity = 0f; 
    }

    public void StartFountainRitual()
    {
        if (ritualSequenceStarted) return;
        ritualSequenceStarted = true;

        StartCoroutine(RitualSequenceRoutine());
    }

    private IEnumerator RitualSequenceRoutine()
    {
        // --- AUDIO TRIGGER: Fade out the current background music immediately ---
        CoreAudioManager.FadeOutBGM(bgmFadeOutTime);

        yield return new WaitForSeconds(0.2f);

        // --- AUDIO TRIGGER: Play the full chant background audio track on LOOP ---
        if (fullChantAudioTrack != null)
        {
            CoreAudioManager.PlayLoopingSFX(fullChantAudioTrack);
        }

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

        // --- AUDIO TRIGGER: Stop the looping chant track before Phase 2 starts ---
        CoreAudioManager.StopLoopingSFX();

        // ============================================================
        // PHASE 2: Divine Spotlight Camera & Intensity Ramp (Divine Light)
        // ============================================================
        if (spotlightCamera != null) spotlightCamera.gameObject.SetActive(true);

        // --- AUDIO TRIGGER: Play the divine intervention / alternative music track ---
        if (divineLightSFX != null)
        {
            CoreAudioManager.PlaySFX(divineLightSFX);
        }

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
        if (missSpellCamera != null) missSpellCamera.gameObject.SetActive(true);

        if (missSpellLight1 != null) missSpellLight1.color = missSpellTargetColor;
        if (missSpellLight2 != null) missSpellLight2.color = missSpellTargetColor;

        if (missSpellGameObject != null)
        {
            if (missSpellDisappearParticles != null)
            {
                missSpellDisappearParticles.transform.position = missSpellGameObject.transform.position;
                missSpellDisappearParticles.Play();
            }

            yield return new WaitForSeconds(0.1f);

            missSpellGameObject.SetActive(false);
            Debug.Log("[EndGameCutscene] Miss Spell vanished! Lights updated to new color.");

            yield return new WaitForSeconds(Mathf.Max(0.1f, missSpellCutsceneDelay - 0.1f));
        }
        else
        {
            yield return new WaitForSeconds(missSpellCutsceneDelay);
        }

        if (missSpellCamera != null) missSpellCamera.gameObject.SetActive(false);

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

        PlayerPrefs.SetInt("IsEndGameCompleted", 1);
        PlayerPrefs.Save();

        // --- AUDIO TRIGGER: Fade the main background music safely back on! ---
        CoreAudioManager.FadeInBGM(1.0f, 1.5f);

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