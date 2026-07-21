using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    [Header("UI & Health Elements")]
    [Tooltip("Drag the Canvas/Panel containing the Health Bar UI here so it can be turned on/off.")]
    public GameObject healthUIContainer;

    [Tooltip("Drag your Health Slider here.")]
    public Slider healthSlider;

    [Tooltip("Drag the TextMeshPro component that is floating above the head here.")]
    public TextMeshProUGUI damageText;

    [Tooltip("Drag the TextMeshPro component for the health numbers (e.g., 100/100) here.")]
    public TextMeshProUGUI healthText; // NEW: Controls the health string display

    [Header("Timing Configuration")]
    [Tooltip("How many seconds to wait for your background scene transition to finish loading before starting the cutscene.")]
    public float startDelay = 1.5f;

    [Tooltip("How long should the cutscene camera stay active before returning to the player?")]
    public float cutsceneDuration = 3f;

    public AudioClip hurtsfx;

    private static HashSet<string> playedHouses = new HashSet<string>();
    private float currentHealth = 100f;
    private float maxHealth = 100f; // Track the maximum health value

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); }

        if (cutsceneCamera != null) cutsceneCamera.gameObject.SetActive(false);

        if (healthUIContainer != null) healthUIContainer.SetActive(false);
        if (damageText != null) damageText.gameObject.SetActive(false);
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
            if (PuzzleProgress.IsHouseComplete(house) && !playedHouses.Contains(house))
            {
                playedHouses.Add(house);
                StartCoroutine(PlayCutsceneSequence());
                return;
            }
        }
    }

    private IEnumerator PlayCutsceneSequence()
    {
        float currentHealth = PuzzleProgress.GlobalCurrentHealth;
        Charactercontroller activePlayer = FindAnyObjectByType<Charactercontroller>();
        if (activePlayer != null) activePlayer.canControl = false;

        yield return new WaitForSeconds(startDelay);

        if (cutsceneCamera != null)
        {
            cutsceneCamera.depth = 99f;
            cutsceneCamera.gameObject.SetActive(true);
        }

        if (healthUIContainer != null)
        {
            healthUIContainer.SetActive(true);
            if (healthSlider != null) healthSlider.value = currentHealth;

            // Initialize text numbers to full before the damage happens
            if (healthText != null) healthText.text = $"{(int)currentHealth}/{(int)maxHealth}";
        }
        CoreAudioManager.PlaySFX(hurtsfx);
        if (hurtParticles != null) hurtParticles.Play();
        if (targetAnimator != null) targetAnimator.Play("hurt", 0, 0f);

        StartCoroutine(AnimateHealthLoss(20f, 1.0f));

        yield return new WaitForSeconds(cutsceneDuration);

        if (targetAnimator != null) targetAnimator.Play("Idle", 0, 0f);
        if (healthUIContainer != null) healthUIContainer.SetActive(false);

        if (cutsceneCamera != null)
        {
            cutsceneCamera.gameObject.SetActive(false);
            cutsceneCamera.depth = -1f;
        }

        if (activePlayer != null) activePlayer.canControl = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private IEnumerator AnimateHealthLoss(float damageAmount, float duration)
    {
        float startHealth = PuzzleProgress.GlobalCurrentHealth;
        float targetHealth = Mathf.Max(0, startHealth - damageAmount);

        if (damageText != null)
        {
            damageText.text = $"-{damageAmount}";
            damageText.gameObject.SetActive(true);
            damageText.color = new Color(damageText.color.r, damageText.color.g, damageText.color.b, 1f);
        }

        float elapsed = 0f;
        Vector3 initialTextPos = damageText != null ? damageText.transform.localPosition : Vector3.zero;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Calculate current frame's numerical health
            float animatedHealth = Mathf.Lerp(startHealth, targetHealth, t);

            if (healthSlider != null)
            {
                healthSlider.value = animatedHealth;
            }

            // NEW: Update the 100/100 string dynamically every frame
            if (healthText != null)
            {
                healthText.text = $"{(int)animatedHealth}/{(int)maxHealth}";
            }

            if (damageText != null)
            {
                damageText.transform.localPosition = initialTextPos + new Vector3(0, t * 1.5f, 0);
                damageText.color = new Color(damageText.color.r, damageText.color.g, damageText.color.b, 1f - t);

                //if (cutsceneCamera != null)
                //{
                //    damageText.transform.LookAt(damageText.transform.position + cutsceneCamera.transform.rotation * Vector3.forward,
                //        cutsceneCamera.transform.rotation * Vector3.up);
                //}
            }
            PuzzleProgress.GlobalCurrentHealth = targetHealth;
            yield return null;
        }

        currentHealth = targetHealth;
        if (healthSlider != null) healthSlider.value = currentHealth;
        if (healthText != null) healthText.text = $"{(int)currentHealth}/{(int)maxHealth}";
        if (damageText != null) damageText.gameObject.SetActive(false);
    }
}