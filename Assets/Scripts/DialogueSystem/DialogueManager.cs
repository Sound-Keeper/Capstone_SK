using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using UnityEngine.InputSystem;

public class DialogueManager : MonoBehaviour
{
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

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (dialoguePanel != null) dialoguePanel.SetActive(false);
    }

    void Start()
    {
        // 1. Find Pip and her scripts
        PipFly pipFly = FindFirstObjectByType<PipFly>();
        PipHint pipHint = FindFirstObjectByType<PipHint>();
        GameObject player = GameObject.FindWithTag("Player");

        if (pipFly != null && fountainTarget != null)
        {
            // Keep her automatic radar brain asleep while you two are chatting
            if (pipHint != null) pipHint.autoGuide = false;

            // 2. Open the text box instantly so she talks first while standing still
            string[] introLines = new string[] {
            "Wake up, {player}! The valley is in trouble!",
            "The sacred vowel stones have been scattered to the five houses.",
            "Follow me! Let's head over to House A first."
        };
            StartDialogue("Pip", introLines);

            // 3. THIS RUNS ONLY AFTER THE PLAYER CLOSES THE ENTIRE DIALOGUE PANEL!
            OnDialogueEnd = () => {

                // First, command her to physically fly out to the fountain target!
                pipFly.MoveToTarget(fountainTarget, () => {

                    // This microsecond callback runs ONLY after she finishes her flight and lands at the fountain:
                    if (pipHint != null)
                    {
                        pipHint.autoGuide = true; // Turn her automatic house-tracking brain on!
                    }
                    else if (player != null)
                    {
                        pipFly.FollowPlayerStart(player.transform); // Fallback: hover on shoulder
                    }
                });
            };
        }
    }

    void Update()
    {
        if (dialoguePanel == null || !dialoguePanel.activeSelf) return;

        bool advance =
            (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame) ||
            (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame);

        if (!advance) return;

        if (isTyping)
        {
            // Skip the typing animation and show the whole line instantly
            StopAllCoroutines();
            dialogueText.text = currentFullLine;
            isTyping = false;
        }
        else
        {
            NextLine();
        }
    }

    // Overload Function: Allows Pip's automated intro sequences and the Altar script 
    // to play plain text strings without throwing an error.
    public void StartDialogue(string speaker, string[] newLines)
    {
        if (newLines == null || newLines.Length == 0) return;

        // 1. Force the physical UI panel to wake up and appear on screen immediately!
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }

        // Convert raw text strings into proper DialogueLine structural formats
        DialogueLine[] convertedLines = new DialogueLine[newLines.Length];
        for (int i = 0; i < newLines.Length; i++)
        {
            convertedLines[i] = new DialogueLine
            {
                speaker = Speaker.NPC,
                text = newLines[i]
            };
        }

        // Send to master method with portraits set safely to empty/null
        StartDialogue(convertedLines, speaker, null, null, dialogueCamera, null);
    }

    // Call this to trigger ANY complex dialogue conversation across the game
    public void StartDialogue(DialogueLine[] lines, string npcName, Sprite npcPortrait,
        Sprite playerPortrait, Camera cam, Action onComplete = null)
    {
        if (lines == null || lines.Length == 0) return;

        this.npcName = npcName;
        this.npcPortrait = npcPortrait;
        this.playerPortrait = playerPortrait;
        this.OnDialogueEnd = onComplete;

        previousCamera = Camera.main;
        dialogueCamera = cam;

        if (dialogueCamera != null)
        {
            if (previousCamera != null) previousCamera.gameObject.SetActive(false);
            dialogueCamera.gameObject.SetActive(true);
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

        // Switches names automatically based on character selection choice
        nameText.text = isPlayer ? CharacterSelection.SelectedName : npcName;

        Highlight(leftPortrait, !isPlayer);
        Highlight(rightPortrait, isPlayer);

        StopAllCoroutines();
        StartCoroutine(TypeLine(FormatString(line.text)));
    }

    void SetupPortrait(Image img, Sprite face)
    {
        if (img == null) return;
        img.sprite = face;
        img.enabled = (face != null);
    }

    void Highlight(Image img, bool isSpeaking)
    {
        if (img == null || !img.enabled) return;
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

        if (dialogueCamera != null)
        {
            dialogueCamera.gameObject.SetActive(false);
            if (previousCamera != null) previousCamera.gameObject.SetActive(true);
        }

        // Fire any logic that is waiting for the text box to finish closing
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