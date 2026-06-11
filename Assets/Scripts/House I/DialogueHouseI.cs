using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using UnityEngine.InputSystem;

public class DialogueHouseI : MonoBehaviour
{
    public static DialogueHouseI Instance;

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

    Action onDialogueComplete;
    DialogueLine[] currentLines;
    string npcName = "NPC";
    string playerName = "You";
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
            //skip the typing animation and show the whole line
            StopAllCoroutines();
            dialogueText.text = currentFullLine;
            isTyping = false;
        }
        else
        {
            NextLine();
        }
    }

    // npcName / playerName decide what name + portrait shows depending on each line's speaker.
    public void StartDialogue(DialogueLine[] lines, string npcName, string playerName,
        Sprite npcPortrait, Sprite playerPortrait, Camera cam, Action onComplete)
    {
        if (lines == null || lines.Length == 0) return;

        this.npcName = npcName;
        this.playerName = playerName;
        this.npcPortrait = npcPortrait;
        this.playerPortrait = playerPortrait;
        onDialogueComplete = onComplete;

        previousCamera = Camera.main;
        dialogueCamera = cam;

        if (dialogueCamera != null)
        {
            if (previousCamera != null) previousCamera.gameObject.SetActive(false);
            dialogueCamera.gameObject.SetActive(true);
        }

        //set up both faces once: left = NPC, right = Player
        SetupPortrait(leftPortrait, npcPortrait);
        SetupPortrait(rightPortrait, playerPortrait);

        currentLines = lines;
        currentLineIndex = 0;
        dialoguePanel.SetActive(true);

        ShowLine();
    }

    void ShowLine()
    {
        DialogueLine line = currentLines[currentLineIndex];
        bool isPlayer = (line.speaker == Speaker.Player);

        //pick the name based on who is speaking this line
        nameText.text = isPlayer ? playerName : npcName;

        //left = NPC, right = Player. Brighten the speaker, dim the other.
        Highlight(leftPortrait, !isPlayer);
        Highlight(rightPortrait, isPlayer);

        StopAllCoroutines();
        StartCoroutine(TypeLine(line.text));
    }

    //assign a face to a side; hide that side entirely if no sprite was given
    void SetupPortrait(Image img, Sprite face)
    {
        if (img == null) return;
        img.sprite = face;
        img.enabled = (face != null);
    }

    //tint a side: bright when speaking, dim when not (only if it has a face)
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

        if (currentLineIndex < currentLines.Length)
            ShowLine();
        else
            EndDialogue();
    }

    void EndDialogue()
    {
        dialoguePanel.SetActive(false);

        if (dialogueCamera != null)
        {
            dialogueCamera.gameObject.SetActive(false);
            if (previousCamera != null) previousCamera.gameObject.SetActive(true);
        }

        onDialogueComplete?.Invoke();
    }
}
