using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class MagicWandReward : MonoBehaviour
{
    [Header("Player Tracking")]
    public Transform player;

    [Header("Cutscene Settings")]
    [Tooltip("An independent camera that views the wand sequence. If left empty, it uses the dialogue/main camera view.")]
    public Camera rewardCamera;
    public float appearDelay = 0.2f;
    public float pauseDuration = 1.5f;
    public float moveDuration = 2f;
    public float spinSpeed = 180f;

    [Header("FX / Visuals")]
    [Tooltip("The 3D wand model child or mesh that actually spins.")]
    public Transform wandVisual;
    public ParticleSystem shineEffect;

    [Header("Events")]
    public UnityEvent OnWandCollected;

    private Camera previousCamera;
    private Charactercontroller cc;
    private bool isMoving = false;

    void Awake()
    {
        // Keep hidden initially until called via dialogue line breakpoint
        gameObject.SetActive(false);
    }

    public void GiveWand(System.Action onSequenceFinished)
    {
        gameObject.SetActive(true);

        if (player == null)
        {
            GameObject pObj = GameObject.FindGameObjectWithTag("Player");
            if (pObj != null) player = pObj.transform;
        }

        if (player != null) cc = player.GetComponent<Charactercontroller>();

        StartCoroutine(CutsceneRoutine(onSequenceFinished));
    }

    IEnumerator CutsceneRoutine(System.Action onSequenceFinished)
    {
        // 1. Setup Camera and Freeze Player
        if (rewardCamera != null)
        {
            previousCamera = Camera.main;
            rewardCamera.gameObject.SetActive(true);
            if (previousCamera != null) previousCamera.gameObject.SetActive(false);
        }

        if (cc != null) cc.canControl = false;

        yield return new WaitForSeconds(appearDelay);

        if (shineEffect != null) shineEffect.Play();

        // 2. Pause & Spin Beat
        float elapsed = 0f;
        while (elapsed < pauseDuration)
        {
            SpinObject();
            KeepCameraFocused();
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 3. Float directly into the Player's chest area
        elapsed = 0f;
        Vector3 initialPos = transform.position;
        isMoving = true;

        Vector3 targetPos = player != null ? player.position + Vector3.up * 1.2f : initialPos;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / moveDuration;

            transform.position = Vector3.Lerp(initialPos, targetPos, t);

            SpinObject();
            KeepCameraFocused();
            yield return null;
        }
        isMoving = false;

        // Save progress if you track a global bool for the wand
        // PuzzleProgress.HasWand = true; 
        Debug.Log("Magic Wand collected!");

        gameObject.SetActive(false);

        // 4. Clean up camera views
        if (rewardCamera != null)
        {
            rewardCamera.gameObject.SetActive(false);
            if (previousCamera != null) previousCamera.gameObject.SetActive(true);
        }

        OnWandCollected?.Invoke();
        onSequenceFinished?.Invoke(); // Resumes dialogue manager flow execution
    }

    void SpinObject()
    {
        Transform targetSpin = wandVisual != null ? wandVisual : transform;
        targetSpin.Rotate(Vector3.up * spinSpeed * Time.deltaTime, Space.Self);
    }

    void KeepCameraFocused()
    {
        if (rewardCamera == null) return;

        rewardCamera.transform.LookAt(transform.position);

        if (isMoving && player != null)
        {
            Vector3 targetCamPos = transform.position - (player.forward * 2.5f) + (Vector3.up * 0.4f);
            rewardCamera.transform.position = Vector3.Lerp(rewardCamera.transform.position, targetCamPos, Time.deltaTime * 2f);
        }
    }
}