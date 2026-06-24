using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class VowelStone : MonoBehaviour
{
    public enum StoneType { None, VowelI, VowelA, VowelE, VowelO, VowelU }

    [Header("Which stone is this?")]
    public StoneType stone = StoneType.VowelI;

    [Header("Player (leave empty to auto-find by 'Player' tag)")]
    public Transform player;
    [Tooltip("Freeze player movement during the cutscene.")]
    public bool freezePlayer = true;

    [Header("Cutscene Settings")]
    [Tooltip("The independent camera that will view the reward sequence.")]
    public Camera rewardCamera;
    public float appearDelay = 0.5f;
    public float pauseDuration = 2f;
    public float moveDuration = 2.5f;
    public float spinSpeed = 180f;

    [Header("FX / Visuals")]
    [Tooltip("The 3D model child or mesh that actually spins.")]
    public Transform stoneVisual;
    public ParticleSystem shineEffect;

    [Header("UI Canvas")]
    public CanvasGroup questCompletedPanel;
    public float uiFadeDuration = 1f;
    public float uiHoldDuration = 2.5f;

    [Header("Events")]
    public UnityEvent OnRewardFinished;

    private Camera previousCamera;
    private Charactercontroller cc;
    private Vector3 startPosition;
    private bool isMoving = false;

    void Awake()
    {
        // Save the start position for calculations
        startPosition = transform.position;

        if (questCompletedPanel != null)
            questCompletedPanel.alpha = 0f;

        // --- ADD THIS LINE TO HIDE IT ON SCENE LOAD ---
        gameObject.SetActive(false);
    }

    public void GiveReward()
    {
        // 1. CRITICAL: Turn the GameObject back ON first!
        gameObject.SetActive(true);

        // 2. Safely grab our player references
        if (player == null)
        {
            GameObject pObj = GameObject.FindGameObjectWithTag("Player");
            if (pObj != null) player = pObj.transform;
        }

        if (player != null) cc = player.GetComponent<Charactercontroller>();

        // 3. Now that the object is awake and active, Unity can safely start the coroutine!
        StartCoroutine(CutsceneRoutine());
    }

    IEnumerator CutsceneRoutine()
    {
        // 1. Setup Camera and Freeze Player
        if (rewardCamera != null)
        {
            previousCamera = Camera.main;
            rewardCamera.gameObject.SetActive(true);
            if (previousCamera != null) previousCamera.gameObject.SetActive(false);
        }

        if (freezePlayer && cc != null) cc.canControl = false;

        yield return new WaitForSeconds(appearDelay);

        if (shineEffect != null) shineEffect.Play();

        // 2. Pause & Spin Beat
        float elapsed = 0f;
        while (elapsed < pauseDuration)
        {
            SpinObject();
            KeepCameraFocused(); // Keep camera tracking the stone!
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 3. Float directly into the Player
        elapsed = 0f;
        Vector3 initialPos = transform.position;
        isMoving = true;

        // Offset slightly upward so it goes to player's chest, not feet
        Vector3 targetPos = player != null ? player.position + Vector3.up * 1f : initialPos;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / moveDuration;

            // Smoothly move the stone's base object transform
            transform.position = Vector3.Lerp(initialPos, targetPos, t);

            SpinObject();
            KeepCameraFocused(); // Move/Turn camera along with the stone!
            yield return null;
        }
        isMoving = false;

        GrantStone();

        gameObject.SetActive(false);

        // 4. Fade Up Win Text Banner
        if (questCompletedPanel != null)
        {
            yield return StartCoroutine(FadeCanvas(questCompletedPanel, 0f, 1f, uiFadeDuration));
            yield return new WaitForSeconds(uiHoldDuration);
            yield return StartCoroutine(FadeCanvas(questCompletedPanel, 1f, 0f, uiFadeDuration));
        }

        // 5. Restore normal player camera view
        if (rewardCamera != null)
        {
            rewardCamera.gameObject.SetActive(false);
            if (previousCamera != null) previousCamera.gameObject.SetActive(true);
        }

        if (freezePlayer && cc != null) cc.canControl = true;

        OnRewardFinished?.Invoke();
    }

    void SpinObject()
    {
        // Spin the designated visual sub-mesh asset so the root transform stays clean
        Transform targetSpin = stoneVisual != null ? stoneVisual : transform;
        targetSpin.Rotate(Vector3.up * spinSpeed * Time.deltaTime, Space.Self);
    }

    void KeepCameraFocused()
    {
        if (rewardCamera == null) return;

        // 1. Force the camera to always turn and look directly at the stone model
        rewardCamera.transform.LookAt(transform.position);

        // 2. If the stone starts flying to the player, have the camera slide forward slightly 
        // to stay closely glued to the action instead of getting left behind
        if (isMoving && player != null)
        {
            Vector3 targetCamPos = transform.position - (player.forward * 3f) + (Vector3.up * 0.5f);
            rewardCamera.transform.position = Vector3.Lerp(rewardCamera.transform.position, targetCamPos, Time.deltaTime * 2f);
        }
    }

    void GrantStone()
    {
        switch (stone)
        {
            case StoneType.VowelI: PuzzleProgress.HasVowelIStone = true; break;
            case StoneType.VowelA: PuzzleProgress.HasVowelAStone = true; break;
            case StoneType.VowelE: PuzzleProgress.HasVowelEStone = true; break;
            case StoneType.VowelO: PuzzleProgress.HasVowelOStone = true; break;
            case StoneType.VowelU: PuzzleProgress.HasVowelUStone = true; break;
        }
        Debug.Log($"{stone} collected!");
    }

    IEnumerator FadeCanvas(CanvasGroup cg, float from, float to, float duration)
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