using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseCompletionCutscene : MonoBehaviour
{
    public static HouseCompletionCutscene Instance { get; private set; }

    [Header("Cinematic Elements")]
    [Tooltip("The dedicated camera focusing on whatever is hurting/reacting.")]
    public Camera cutsceneCamera;

    [Tooltip("The GameObject that has the Animator with your 'hurt' state.")]
    public Animator targetAnimator;

    [Tooltip("Drag the hurt Particle System here.")]
    public ParticleSystem hurtParticles;

    [Header("Timing Configuration")]
    [Tooltip("How many seconds to wait for your background scene transition to finish loading before starting the cutscene.")]
    public float startDelay = 1.5f;

    [Tooltip("How long should the cutscene camera stay active before returning to the player?")]
    public float cutsceneDuration = 3f;

    private static HashSet<string> playedHouses = new HashSet<string>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); }

        if (cutsceneCamera != null) cutsceneCamera.gameObject.SetActive(false);
    }

    void Start()
    {
        CheckForRecentCompletion();
    }

    private void CheckForRecentCompletion()
    {
        string[] houses = { "A", "E", "I", "O", "U" };

        foreach (string house in houses)
        {
            // If the puzzle flag is complete, and we haven't played the cutscene for this specific house yet
            if (PuzzleProgress.IsHouseComplete(house) && !playedHouses.Contains(house))
            {
                playedHouses.Add(house); // Mark this specific house as seen permanently
                StartCoroutine(PlayCutsceneSequence());
                return; // Play the sequence and exit completely so it doesn't try to double-play
            }
        }
    }

    private IEnumerator PlayCutsceneSequence()
    {
        // 1. Freeze player movement completely
        Charactercontroller activePlayer = FindFirstObjectByType<Charactercontroller>();
        if (activePlayer != null) activePlayer.canControl = false;

        // 2. WAIT for the scene transition overlay to clear
        yield return new WaitForSeconds(startDelay);

        // 3. Force the cutscene camera to draw ON TOP of everything else
        if (cutsceneCamera != null)
        {
            cutsceneCamera.depth = 99f;
            cutsceneCamera.gameObject.SetActive(true);
        }

        // 4. Play the hurt particles
        if (hurtParticles != null)
        {
            hurtParticles.Play();
        }

        // 5. Force-trigger the Animator state directly from frame zero
        if (targetAnimator != null)
        {
            targetAnimator.Play("hurt", 0, 0f);
        }

        // 6. Wait out the cinematic animation timer
        yield return new WaitForSeconds(cutsceneDuration);

        // 7. SNAP HER BACK TO IDLE STATE
        if (targetAnimator != null)
        {
            targetAnimator.Play("Idle", 0, 0f);
        }

        // 8. Deactivate the cutscene camera so view drops back down to the player smoothly
        if (cutsceneCamera != null)
        {
            cutsceneCamera.gameObject.SetActive(false);
            cutsceneCamera.depth = -1f;
        }

        // 9. Give controls back to the player
        if (activePlayer != null) activePlayer.canControl = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}