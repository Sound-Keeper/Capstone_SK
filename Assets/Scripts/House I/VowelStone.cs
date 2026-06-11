using System.Collections;
using UnityEngine;
using UnityEngine.Events;

// Reusable "vowel stone" reward cutscene. Built so House A (and the rest) can reuse the
// SAME component - just drop it on each house's stone and fill the slots in the Inspector.
//
// Sequence when GiveReward() is called (hook it to PuzzleManager.OnPuzzleComplete):
//   1. (optional) cut to a reward Camera and freeze the player
//   2. the stone appears, shines (spins + pulses) and PAUSES for a beat
//   3. it drifts SLOWLY into the player
//   4. the matching vowel-stone flag is granted
//   5. a "QUEST COMPLETED" panel fades in slowly and holds
//   6. control + camera are handed back, then OnRewardFinished fires
//
// Hook your return-to-MainWorld (PuzzleCompleteReturn.ReturnToMainWorld) to OnRewardFinished
// so it runs AFTER the cutscene - not to PuzzleManager.OnPuzzleComplete anymore.
public class VowelStone : MonoBehaviour
{
    public enum StoneType { None, VowelI, VowelA, VowelE, VowelO, VowelU }

    [Header("Which stone is this? (sets the matching PuzzleProgress flag)")]
    public StoneType stone = StoneType.VowelI;

    [Header("Player (leave empty to auto-find by 'Player' tag)")]
    public Transform player;
    [Tooltip("Freeze player movement during the cutscene.")]
    public bool freezePlayer = true;

    [Header("Reward camera (optional)")]
    [Tooltip("A Camera that frames the stone. It's switched on for the cutscene and off after. Leave empty to keep the normal camera.")]
    public Camera rewardCamera;

    [Header("Shine particle (optional)")]
    [Tooltip("Plays when the stone appears.")]
    public ParticleSystem shineEffect;

    [Header("Shine / pause")]
    public float spinSpeed = 90f;        // degrees/sec
    public float pulseAmount = 0.15f;    // grow/shrink while shining
    public float pulseSpeed = 3f;
    [Tooltip("Seconds it shines in place (the pause/beat) before drifting to the player.")]
    public float pauseTime = 1.5f;

    [Header("Drift to player (slow)")]
    [Tooltip("Seconds to slowly travel into the player. Bigger = slower.")]
    public float flyTime = 2.0f;
    [Tooltip("Aim above the player's feet (chest/face height).")]
    public float floatHeight = 1.2f;

    [Header("Quest Completed panel (optional)")]
    [Tooltip("A CanvasGroup holding your 'QUEST COMPLETED' text/art. Starts hidden, fades in.")]
    public CanvasGroup questCompletedPanel;
    public float questFadeTime = 1.0f;   // slow fade-in
    public float questHoldTime = 1.5f;   // how long it stays up

    [Header("When the whole cutscene is done")]
    [Tooltip("Hook PuzzleCompleteReturn.ReturnToMainWorld () here so return happens AFTER the cutscene.")]
    public UnityEvent OnRewardFinished;

    Vector3 baseScale;
    bool started = false;

    void Awake()
    {
        baseScale = transform.localScale;
        gameObject.SetActive(false);                 // hidden until the puzzle is solved
        if (questCompletedPanel != null)
        {
            questCompletedPanel.alpha = 0f;
            questCompletedPanel.gameObject.SetActive(true); // keep active so we can fade it
        }
        if (rewardCamera != null) rewardCamera.gameObject.SetActive(false);
    }

    // Hook this to PuzzleManager.OnPuzzleComplete in the Inspector.
    public void GiveReward()
    {
        if (started) return;
        started = true;

        gameObject.SetActive(true);
        if (shineEffect != null) shineEffect.Play();
        StartCoroutine(RewardRoutine());
    }

    IEnumerator RewardRoutine()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        // ---- 1. freeze player + cut to the reward camera ----
        Charactercontroller cc = (player != null) ? player.GetComponent<Charactercontroller>() : null;
        if (freezePlayer && cc != null) cc.enabled = false;

        Camera previousCamera = null;
        if (rewardCamera != null)
        {
            previousCamera = Camera.main;
            if (previousCamera != null && previousCamera != rewardCamera)
                previousCamera.gameObject.SetActive(false);
            rewardCamera.gameObject.SetActive(true);
        }

        // ---- 2. shine + pause ----
        float t = 0f;
        while (t < pauseTime)
        {
            t += Time.deltaTime;
            Spin();
            Pulse();
            yield return null;
        }

        // ---- 3. drift slowly into the player ----
        Vector3 start = transform.position;
        float f = 0f;
        while (f < 1f)
        {
            f += Time.deltaTime / Mathf.Max(0.01f, flyTime);
            float smooth = Mathf.SmoothStep(0f, 1f, f); // ease in/out so it feels gentle
            Vector3 target = (player != null) ? player.position + Vector3.up * floatHeight : start;
            transform.position = Vector3.Lerp(start, target, smooth);
            transform.localScale = Vector3.Lerp(baseScale, baseScale * 0.05f, smooth);
            Spin();
            yield return null;
        }

        // ---- 4. grant the stone ----
        GrantStone();
        gameObject.SetActive(false);

        // ---- 5. slowly show QUEST COMPLETED ----
        if (questCompletedPanel != null)
        {
            yield return FadeCanvas(questCompletedPanel, 0f, 1f, questFadeTime);
            yield return new WaitForSeconds(questHoldTime);
        }

        // ---- 6. hand control + camera back, then finish ----
        if (rewardCamera != null)
        {
            rewardCamera.gameObject.SetActive(false);
            if (previousCamera != null) previousCamera.gameObject.SetActive(true);
        }
        if (freezePlayer && cc != null) cc.enabled = true;

        OnRewardFinished?.Invoke();   // e.g. PuzzleCompleteReturn.ReturnToMainWorld ()
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

    void Spin()
    {
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.World);
    }

    void Pulse()
    {
        float s = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        transform.localScale = baseScale * s;
    }
}
