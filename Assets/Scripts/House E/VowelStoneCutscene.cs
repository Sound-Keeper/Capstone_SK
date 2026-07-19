using UnityEngine;
using System.Collections;
using BookChoice; // Matches your dialogue manager's namespace

public class VowelStoneCutscene : MonoBehaviour
{
    [Header("Hierarchy Assignments")]
    public GameObject puzzleCanvas;         // Drag 'Canvas' from your hierarchy here
    public Transform stonePrefab;          // Drag 'EStone_Prefab' here
    public ParticleSystem poofParticles;   // Drag 'Particle System (1)' here

    [Header("Win Panel Setup")]
    [Tooltip("Drag the CanvasGroup for your Quest/House Completed Banner here.")]
    [SerializeField] private CanvasGroup winPanelCanvas;
    [SerializeField] private float winPanelFadeDuration = 1f;

    [Header("Dialogue Manager References")]
    [Tooltip("Drag 'Dialoguebox' here if using House E.")]
    public DialogueBoxManager dialogueBoxManager;

    [Tooltip("Drag 'Dialoguebox' here if using House O.")]
    public DialogueBoxManagerHouseO dialogueBoxManagerHouseO;

    // ─── NEW ONE-SHOT SFX & FADE SETTINGS ─────────────────────────────────────────
    [Header("Cutscene Audio Setup")]
    [Tooltip("Drag the one-shot victory sound effect here.")]
    public AudioClip cutsceneSFX;

    [Tooltip("How long it takes for the background music to fade to silence.")]
    public float bgmFadeOutDuration = 0.5f;

    [Tooltip("How long it takes for the background music to fade back in at the end.")]
    public float bgmFadeInDuration = 1.0f;
    // ─────────────────────────────────────────────────────────────────────────────

    [Header("Animation Settings")]
    public float rotationSpeed = 150f;
    public float spinDuration = 3f;

    void Start()
    {
        if (poofParticles != null)
            poofParticles.Stop();

        // Ensure the win banner is hidden right at the start
        if (winPanelCanvas != null)
            winPanelCanvas.alpha = 0f;
    }

    public void PlayCutscene()
    {
        StartCoroutine(CutsceneSequence());
    }

    private IEnumerator CutsceneSequence()
    {
        // --- STEP 1: Fade out the background music smoothly ---
        CoreAudioManager.FadeOutBGM(bgmFadeOutDuration);

        // Turn off the entire UI canvas completely
        if (puzzleCanvas != null)
            puzzleCanvas.SetActive(false);

        // --- STEP 2: Play your one-shot sound effect in the clear space ---
        if (cutsceneSFX != null)
        {
            CoreAudioManager.PlaySFX(cutsceneSFX);
        }

        // Start fading the Win Panel up immediately AS the spin begins
        if (winPanelCanvas != null)
        {
            StartCoroutine(FadeCanvas(winPanelCanvas, 0f, 1f, winPanelFadeDuration));
        }

        // Spin the stone over time
        float timer = 0f;
        while (timer < spinDuration)
        {
            if (stonePrefab != null)
            {
                stonePrefab.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.Self);
            }
            timer += Time.deltaTime;
            yield return null;
        }

        // Poof particles & disable the mesh asset
        if (poofParticles != null)
            poofParticles.Play();

        // Fade out the Win Panel dynamically while particles vanish
        if (winPanelCanvas != null)
        {
            StartCoroutine(FadeCanvas(winPanelCanvas, 1f, 0f, 1.0f));
        }

        if (stonePrefab != null)
            stonePrefab.gameObject.SetActive(false);

        // Wait a small moment for the particles to expand/fade out
        yield return new WaitForSeconds(1.0f);

        if (poofParticles != null)
        {
            poofParticles.Stop();
            poofParticles.gameObject.SetActive(false);
        }

        // Cutscene finished! Safely re-enable the Canvas
        if (puzzleCanvas != null)
            puzzleCanvas.SetActive(true);

        // --- STEP 3: Fade the background music back in cleanly ---
        // Grab the baseline volume tracked by the stone collection, defaulting to 1f if unassigned
        float targetVol = VowelStone.PreCutsceneVolume;
        if (targetVol <= 0.01f) targetVol = 1f;

        CoreAudioManager.FadeInBGM(targetVol, bgmFadeInDuration);

        // Wake up whichever dialogue manager is assigned and tell it to display victory text!
        if (dialogueBoxManager != null)
        {
            dialogueBoxManager.StartFinalDialogueSequence();
        }
        else if (dialogueBoxManagerHouseO != null)
        {
            dialogueBoxManagerHouseO.StartFinalDialogueSequence();
        }
        else
        {
            Debug.LogWarning("VowelStoneCutscene: No dialogue box manager was assigned in the inspector!");
        }
    }

    // Shared smooth fading formula for the canvas banner
    private IEnumerator FadeCanvas(CanvasGroup cg, float from, float to, float duration)
    {
        float e = 0f;
        cg.alpha = from;
        while (e < duration)
        {
            e += Time.deltaTime;
            cg.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(e / duration));
            yield return null;
        }
        cg.alpha = to;
    }
}