using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class UHouseManager : MonoBehaviour
{
    [Header("UI Split Layout Elements (TMP)")]
    public TMP_Text textLeft;
    public TMP_Text textRight;
    public TMP_Text progressText;

    [Tooltip("The UI RectTransform used as the anchor blank.")]
    public RectTransform dynamicUnderlineTarget;

    [Tooltip("🎯 NEW: Drag your dedicated 'Win_Status_Text' GameObject here!")]
    public GameObject winStatusTextObject;

    [System.Serializable]
    public struct VowelTask
    {
        public string leftLetters;
        public string correctVowel;
        public string rightLetters;
    }

    [Header("Task Configuration")]
    public VowelTask[] tasks = new VowelTask[3];
    private int currentTaskIndex = 0;

    [Header("Movement Settings")]
    public float flySpeed = 8f;

    [Header("Juice & Feedback")]
    public Transform shakeTarget;
    public float shakeDuration = 0.5f;
    public float shakeMagnitude = 0.1f;

    [Header("Events")]
    public UnityEvent OnChallengeComplete;

    private bool isAnimating = false;
    private Dictionary<GameObject, Vector3> paperHomePositions = new Dictionary<GameObject, Vector3>();

    void Start()
    {
        currentTaskIndex = 0;
        UpdateTaskUI();
        RememberHomePositions();

        // Safety check: Ensure the win screen text is hidden when the game starts
        if (winStatusTextObject != null) winStatusTextObject.SetActive(false);
    }

    void RememberHomePositions()
    {
        VowelPaper[] papers = FindObjectsByType<VowelPaper>(FindObjectsSortMode.None);
        foreach (VowelPaper paper in papers)
        {
            if (paper.houseManager == this)
            {
                paperHomePositions[paper.gameObject] = paper.transform.position;
            }
        }
    }

    void UpdateTaskUI()
    {
        if (currentTaskIndex < tasks.Length)
        {
            textLeft.text = tasks[currentTaskIndex].leftLetters;
            textRight.text = tasks[currentTaskIndex].rightLetters;
            progressText.text = $"Tasks Complete: {currentTaskIndex}/{tasks.Length}";
        }
    }

    public void CheckVowelSelection(string chosenVowel, GameObject paperObject)
    {
        if (isAnimating || currentTaskIndex >= tasks.Length) return;

        if (chosenVowel.ToUpper() == tasks[currentTaskIndex].correctVowel.ToUpper())
        {
            StartCoroutine(FlyToUnderline(paperObject));
        }
        else
        {
            StartCoroutine(WrongAnswerRoutine(paperObject));
        }
    }

    private IEnumerator FlyToUnderline(GameObject paper)
    {
        isAnimating = true;

        // Disable collisions so it can't be clicked mid-flight
        if (paper.TryGetComponent<Collider>(out Collider col)) col.enabled = false;

        Vector3 finalTargetPos = dynamicUnderlineTarget.position;

        // 1. Fly SMOOTHLY to the underline blank
        while (Vector3.Distance(paper.transform.position, finalTargetPos) > 0.05f)
        {
            finalTargetPos = dynamicUnderlineTarget.position;
            paper.transform.position = Vector3.MoveTowards(paper.transform.position, finalTargetPos, flySpeed * Time.deltaTime);
            yield return null;
        }
        paper.transform.position = finalTargetPos;

        // 2. Pause so the player sees the solved word
        yield return new WaitForSeconds(1.2f);

        // 3. SNAP INSTANTLY back home instead of flying
        Vector3 homePos = paperHomePositions[paper.gameObject];
        paper.transform.position = homePos;

        // Re-enable collisions so it's ready to be clicked again for the next word
        if (col != null) col.enabled = true;

        // 4. Advance to the next task
        currentTaskIndex++;
        if (currentTaskIndex >= tasks.Length)
        {
            CompleteChallenge();
        }
        else
        {
            UpdateTaskUI();
        }

        isAnimating = false;
    }

    private IEnumerator WrongAnswerRoutine(GameObject paper)
    {
        isAnimating = true;
        StartCoroutine(PlayShakeEffect());

        Vector3 homePos = paperHomePositions[paper.gameObject];
        Vector3 slightDip = homePos + (Vector3.down * 0.3f);

        float timer = 0f;
        while (timer < 0.15f)
        {
            paper.transform.position = Vector3.Lerp(homePos, slightDip, timer / 0.15f);
            timer += Time.deltaTime;
            yield return null;
        }

        timer = 0f;
        while (timer < 0.15f)
        {
            paper.transform.position = Vector3.Lerp(slightDip, homePos, timer / 0.15f);
            timer += Time.deltaTime;
            yield return null;
        }
        paper.transform.position = homePos;

        isAnimating = false;
    }

    private IEnumerator PlayShakeEffect()
    {
        if (shakeTarget == null) yield break;

        Vector3 originalPos = shakeTarget.localPosition;
        float elapsed = 0.0f;

        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;

            shakeTarget.localPosition = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);
            elapsed += Time.deltaTime;

            yield return null;
        }

        shakeTarget.localPosition = originalPos;
    }

    void CompleteChallenge()
    {
        if (progressText != null) progressText.text = "Tasks Complete: 3/3!";

        // Hide the layout container holding Text_Left, Text_Right, and the Underline line
        if (dynamicUnderlineTarget != null && dynamicUnderlineTarget.parent != null)
        {
            dynamicUnderlineTarget.parent.gameObject.SetActive(false);
        }

        // 🎯 FIX: Explicitly turn ON your big beautiful Challenge Clear text!
        if (winStatusTextObject != null)
        {
            winStatusTextObject.SetActive(true);
        }

        Debug.Log("House U Completed! Win screen triggered cleanly.");
        OnChallengeComplete?.Invoke();
    }
}