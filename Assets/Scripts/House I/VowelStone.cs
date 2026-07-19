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
    public float spinSpeed = 180f;

    [Header("FX / Visuals")]
    [Tooltip("The 3D model child or mesh that actually spins.")]
    public Transform stoneVisual;
    public ParticleSystem shineEffect;

    [Header("Audio")]
    [Tooltip("Drag the big 'Puzzle Completed' victory fan-fare sound effect here!")]
    public AudioClip rewardSFX;

    [Header("Blocked Progress Vines")]
    [Tooltip("The vine GameObject that should disappear when this stone is unlocked.")]
    public GameObject blockingVine;

    public GameObject hintmap;

    [Header("UI Canvas")]
    public CanvasGroup questCompletedPanel;
    public float uiFadeDuration = 1f;
    public float uiHoldDuration = 2.5f;

    [Header("Events")]
    public UnityEvent OnRewardFinished;

    private Camera previousCamera;
    private Charactercontroller cc;
    private Vector3 startPosition;

    // We track the exact pre-fade volume to safely restore it later
    public static float PreCutsceneVolume { get; private set; } = 1f;

    void Awake()
    {
        startPosition = transform.position;

        if (questCompletedPanel != null)
            questCompletedPanel.alpha = 0f;

        gameObject.SetActive(false);
    }

    void Start()
    {
        StartCoroutine(InfiniteSpinAndFXRoutine());
    }

    public void GiveReward()
    {
        gameObject.SetActive(true);

        if (player == null)
        {
            GameObject pObj = GameObject.FindGameObjectWithTag("Player");
            if (pObj != null) player = pObj.transform;
        }

        if (player != null) cc = player.GetComponent<Charactercontroller>();

        StartCoroutine(CutsceneRoutine());
    }

    IEnumerator CutsceneRoutine()
    {
        if (hintmap != null)
        {
            hintmap.SetActive(false);
        }

        if (rewardCamera != null)
        {
            previousCamera = Camera.main;
            rewardCamera.gameObject.SetActive(true);
            if (previousCamera != null) previousCamera.gameObject.SetActive(false);
        }

        if (freezePlayer && cc != null) cc.canControl = false;

        yield return new WaitForSeconds(appearDelay);

        if (blockingVine != null)
        {
            blockingVine.SetActive(false);
        }

        if (shineEffect != null && !shineEffect.isPlaying) shineEffect.Play();

        float elapsed = 0f;
        while (elapsed < pauseDuration)
        {
            SpinObject();
            KeepCameraFocused();
            elapsed += Time.deltaTime;
            yield return null;
        }

        GrantStone();

        // --- NEW: Dynamic Smooth Fade Out ---
        AudioSource bgmSource = FindFirstObjectByType<CoreAudioManager>()?.GetComponentInChildren<AudioSource>();
        if (bgmSource != null)
        {
            PreCutsceneVolume = bgmSource.volume; // Record player volume layout baseline
        }

        CoreAudioManager.FadeOutBGM(0.5f); // Smoothly fade to silence over 0.5s!

        if (rewardSFX != null)
        {
            CoreAudioManager.PlaySFX(rewardSFX);
        }

        if (questCompletedPanel != null)
        {
            yield return StartCoroutine(FadeCanvas(questCompletedPanel, 0f, 1f, uiFadeDuration));
            yield return new WaitForSeconds(uiHoldDuration);
            yield return StartCoroutine(FadeCanvas(questCompletedPanel, 1f, 0f, uiFadeDuration));
        }

        if (rewardCamera != null)
        {
            rewardCamera.gameObject.SetActive(false);
            if (previousCamera != null) previousCamera.gameObject.SetActive(true);
        }

        if (freezePlayer && cc != null) cc.canControl = true;

        OnRewardFinished?.Invoke();
    }

    IEnumerator InfiniteSpinAndFXRoutine()
    {
        while (true)
        {
            SpinObject();

            if (shineEffect != null && !shineEffect.isPlaying && gameObject.activeInHierarchy)
            {
                shineEffect.Play();
            }
            yield return null;
        }
    }

    void SpinObject()
    {
        Transform targetSpin = stoneVisual != null ? stoneVisual : transform;
        targetSpin.Rotate(Vector3.up * spinSpeed * Time.deltaTime, Space.Self);
    }

    void KeepCameraFocused()
    {
        if (rewardCamera == null) return;
        rewardCamera.transform.LookAt(transform.position);
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