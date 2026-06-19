using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using UnityEngine.InputSystem;

public class DialogueManager : MonoBehaviour
{
    private static bool hasPlayedPipIntro = false;
    public static DialogueManager Instance;

    [Header("UI References")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;

    [Header("Portraits")]
    [Tooltip("Left-side face (NPC). Optional.")]
    public Image leftPortrait;
    [Tooltip("Right-side face (Player). Optional.")]
    public Image rightPortrait;
    [Tooltip("Tint for the speaker who is currently talking.")]
    public Color activeTint = Color.white;
    [Tooltip("Tint for the speaker who is NOT talking (dimmed).")]
    public Color inactiveTint = new Color(0.45f, 0.45f, 0.45f, 1f);

    [Header("Typing Effect")]
    public float typingSpeed = 0.03f;

    [Header("Pip Intro Sequence Setup")]
    public PipFly pipFly;
    public Transform fountainTarget;
    public Transform houseATarget;
    public float arriveDistance = 3f;

    [HideInInspector] public Action OnDialogueEnd;

    DialogueLine[] currentLines;
    string npcName = "NPC";
    Sprite npcPortrait;
    Sprite playerPortrait;
    int currentLineIndex = 0;
    bool isTyping = false;
    string currentFullLine = "";
    Camera previousCamera;
    Camera dialogueCamera;

    // Safety timer to prevent the "E" interaction keyframe from immediately skipping the typing effect
    private float inputCooldownTimer = 0f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (dialoguePanel != null) dialoguePanel.SetActive(false);
    }

    void Start()
    {
        PipFly pipFly = FindFirstObjectByType<PipFly>();
        PipHint pipHint = FindFirstObjectByType<PipHint>();
        GameObject player = GameObject.FindWithTag("Player");

        if (!hasPlayedPipIntro && pipFly != null && fountainTarget != null)
        {
            hasPlayedPipIntro = true;

            if (pipHint != null) pipHint.autoGuide = false;

            string[] introLines = new string[] {
                "Wake up, {player}! The valley is in trouble!",
                "The sacred vowel stones have been scattered to the five houses.",
                "Follow me! Let's head over to House A first."
            };
            StartDialogue("Pip", introLines, npcPortrait);

            OnDialogueEnd = () => {
                pipFly.MoveToTarget(fountainTarget, () => {
                    if (pipHint != null)
                    {
                        pipHint.autoGuide = true;
                    }
                    else if (player != null)
                    {
                        pipFly.FollowPlayerStart(player.transform);
                    }
                });
            };
        }
        else
        {
            if (pipFly != null && player != null)
            {
                pipFly.FollowPlayerStart(player.transform);
            }
            if (pipHint != null)
            {
                pipHint.autoGuide = true;
            }
        }
    }

    void Update()
    {
        if (dialoguePanel == null || !dialoguePanel.activeSelf) return;

        if (inputCooldownTimer > 0)
        {
            inputCooldownTimer -= Time.deltaTime;
        }

        bool advance =
            (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame) ||
            (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame);

        if (!advance || inputCooldownTimer > 0) return;

        if (isTyping)
        {
            StopAllCoroutines();
            dialogueText.text = currentFullLine;
            isTyping = false;
        }
        else
        {
            NextLine();
        }
    }

    public void StartDialogue(string speaker, string[] newLines, Sprite speakerPortrait = null)
    {
        if (newLines == null || newLines.Length == 0) return;

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }

        DialogueLine[] convertedLines = new DialogueLine[newLines.Length];
        for (int i = 0; i < newLines.Length; i++)
        {
            convertedLines[i] = new DialogueLine
            {
                speaker = Speaker.NPC,
                text = newLines[i]
            };
        }

        StartDialogue(convertedLines, speaker, speakerPortrait, null, dialogueCamera, OnDialogueEnd);
    }

    // Single Master implementation for structured node script logic
    public void StartDialogue(DialogueLine[] lines, string npcName, Sprite npcPortrait,
        Sprite playerPortrait, Camera cam, Action onComplete = null)
    {
        if (lines == null || lines.Length == 0) return;

        this.npcName = npcName;
        this.npcPortrait = npcPortrait;
        this.playerPortrait = playerPortrait;

        if (onComplete != null)
        {
            this.OnDialogueEnd = onComplete;
        }

        // 🎥 FIX: Safely store the main rendering camera and switch to the assigned NPC dialogue camera
        if (cam != null)
        {
            previousCamera = Camera.main;
            dialogueCamera = cam;

            dialogueCamera.gameObject.SetActive(true);
            if (previousCamera != null) previousCamera.gameObject.SetActive(false);
        }

        SetupPortrait(leftPortrait, npcPortrait);
        SetupPortrait(rightPortrait, playerPortrait);

        currentLines = lines;
        currentLineIndex = 0;
        dialoguePanel.SetActive(true);

        ShowLine();
    }

    void ShowLine()
    {
        if (currentLines == null || currentLineIndex >= currentLines.Length) return;

        DialogueLine line = currentLines[currentLineIndex];
        bool isPlayer = (line.speaker == Speaker.Player);

        nameText.text = isPlayer ? CharacterSelection.SelectedName : npcName;

        Sprite activePortrait = isPlayer ? playerPortrait : npcPortrait;
        SetupPortrait(leftPortrait, activePortrait);

        inputCooldownTimer = 0.2f;

        StopAllCoroutines();
        StartCoroutine(TypeLine(FormatString(line.text)));
    }

    void SetupPortrait(Image img, Sprite face)
    {
        if (img == null) return;

        if (face != null)
        {
            img.gameObject.SetActive(true);
            img.enabled = true;
            img.sprite = face;

            Color c = img.color;
            c.a = 1f;
            img.color = c;
        }
        else
        {
            img.sprite = null;
            img.enabled = false;
            img.gameObject.SetActive(false);
        }
    }

    void Highlight(Image img, bool isSpeaking)
    {
        if (img == null || !img.gameObject.activeSelf) return;
        img.color = isSpeaking ? activeTint : inactiveTint;
    }

    IEnumerator TypeLine(string line)
    {
        isTyping = true;
        currentFullLine = line;
        dialogueText.text = "";

        foreach (char c in line)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    void NextLine()
    {
        currentLineIndex++;

        if (currentLines != null && currentLineIndex < currentLines.Length)
        {
            ShowLine();
        }
        else
        {
            EndDialogue();
        }
    }

    void EndDialogue()
    {
        dialoguePanel.SetActive(false);

        // 🎥 FIX: Disable dialogue camera and return controls/rendering back to normal main scene camera view
        if (dialogueCamera != null)
        {
            dialogueCamera.gameObject.SetActive(false);
            if (previousCamera != null) previousCamera.gameObject.SetActive(true);
        }

        Action cb = OnDialogueEnd;
        OnDialogueEnd = null;
        cb?.Invoke();
    }

    string FormatString(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        return s.Replace("{player}", CharacterSelection.SelectedName);
    }
}