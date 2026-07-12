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
        // 1. Turn off the entire UI canvas completely
        if (puzzleCanvas != null)
            puzzleCanvas.SetActive(false);

        // --- NEW: Start fading the Win Panel up immediately AS the spin begins ---
        if (winPanelCanvas != null)
        {
            StartCoroutine(FadeCanvas(winPanelCanvas, 0f, 1f, winPanelFadeDuration));
        }

        // 2. Spin the stone over time
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

        // 3. Poof particles & disable the mesh asset
        if (poofParticles != null)
            poofParticles.Play();

        // --- NEW: Fade out the Win Panel dynamically while particles vanish ---
        if (winPanelCanvas != null)
        {
            StartCoroutine(FadeCanvas(winPanelCanvas, 1f, 0f, 1.0f)); // Fades out completely over the 1-second particle duration
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

        // 4. Cutscene finished! Safely re-enable the Canvas
        if (puzzleCanvas != null)
            puzzleCanvas.SetActive(true);

        // 5. Wake up whichever dialogue manager is assigned and tell it to display victory text!
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

    // Shared smooth fading formula imported directly from VowelStone.cs
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