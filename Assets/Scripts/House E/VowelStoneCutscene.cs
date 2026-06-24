using UnityEngine;
using System.Collections;
using BookChoice; // Matches your dialogue manager's namespace

public class VowelStoneCutscene : MonoBehaviour
{
    [Header("Hierarchy Assignments")]
    public GameObject puzzleCanvas;         // Drag 'Canvas' from your hierarchy here
    public Transform stonePrefab;          // Drag 'EStone_Prefab' here
    public ParticleSystem poofParticles;   // Drag 'Particle System (1)' here

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

        if (stonePrefab != null)
            stonePrefab.gameObject.SetActive(false);

        // Wait a small moment for the particles to expand
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
}