using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using UnityEngine.InputSystem;

public class DialogueManager : MonoBehaviour
{
    public static bool hasPlayedPipIntro = false;
    public static bool HasPlayedPipIntro => hasPlayedPipIntro;
    public static bool hasPlayedPipIntroFinished = false;
    public static bool HasPlayedPipIntroFinished => hasPlayedPipIntroFinished;
    public static DialogueManager Instance;

    [Header("UI References")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;

    [Header("Portraits")]
    [Tooltip("Left-side face (NPC UI slot).")]
    public Image leftPortrait;
    [Tooltip("Right-side face (Player UI slot).")]
    public Image rightPortrait;

    [Header("Global Character Portrait Assignments")]
    [Tooltip("Global Portrait for Paige (Character 0)")]
    public Sprite paigePortrait;
    [Tooltip("Global Portrait for Penn (Character 1)")]
    public Sprite pennPortrait;

    [Tooltip("Tint for the speaker who is currently talking.")]
    public Color activeTint = Color.white;
    [Tooltip("Tint for the speaker who is NOT talking (dimmed).")]
    public Color inactiveTint = new Color(0.45f, 0.45f, 0.45f, 1f);

    [Header("Typing Effect")]
    public float typingSpeed = 0.03f;

    [Header("Pip Intro Sequence Setup")]
    public PipFly pipFly;
    [Tooltip("Assign the Pip portrait here for the opening cinematic sequence!")]
    public Sprite pipIntroPortrait;
    public Transform fountainTarget;
    public float arriveDistance = 3f;

    [Header("Cinematic Cutscene Setup")]
    [Tooltip("A camera that points at or follows Pip while he flies to the fountain.")]
    public Camera pipCutsceneCamera; // <--- ADDED THIS FIELD FOR THE CUTSCENE

    [HideInInspector] public Action OnDialogueEnd;

    DialogueLine[] currentLines;
    string npcName = "NPC";
    Sprite npcPortrait;
    Sprite runtimePlayerPortrait;
    int currentLineIndex = 0;
    bool isTyping = false;
    string currentFullLine = "";
    Camera previousCamera;
    Camera dialogueCamera;

    private float inputCooldownTimer = 0f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (pipCutsceneCamera != null) pipCutsceneCamera.gameObject.SetActive(false);
    }

    void Start()
    {
        if (pipFly == null) pipFly = FindFirstObjectByType<PipFly>();
        PipHint pipHintSystem = FindFirstObjectByType<PipHint>();
        GameObject player = GameObject.FindWithTag("Player");

        bool anyHouseSolved = PuzzleProgress.HouseASolved || PuzzleProgress.HouseESolved ||
                              PuzzleProgress.HouseISolved || PuzzleProgress.HouseOSolved ||
                              PuzzleProgress.HouseUSolved;

        if (anyHouseSolved)
        {
            hasPlayedPipIntro = true;
            hasPlayedPipIntroFinished = true;
        }

        if (pipIntroPortrait == null && pipFly != null)
        {
            PipInteraction pipInteraction = pipFly.GetComponent<PipInteraction>();
            if (pipInteraction != null)
            {
                pipIntroPortrait = pipInteraction.pipPortrait;
            }
        }

        if (!hasPlayedPipIntro && pipFly != null && fountainTarget != null)
        {
            hasPlayedPipIntro = true;


            string[] introLines = new string[] {
                "Wake up, {player}! The valley is in trouble!",
                "The sacred vowel stones have been scattered to the five houses.",
                "Follow me! Let's head over to House A first."
            };

            StartDialogue("Pip", introLines, pipIntroPortrait);

            OnDialogueEnd = () => {
                // 1. CUTSCENE START: Keep player controls locked explicitly
                SetPlayerControlState(false);

                // 2. CAMERA SWITCH: Activate the cutscene tracking camera
                Camera mainCam = Camera.main;
                if (pipCutsceneCamera != null)
                {
                    if (mainCam != null) mainCam.gameObject.SetActive(false);
                    pipCutsceneCamera.gameObject.SetActive(true);
                }

                // 3. START FLIGHT
                pipFly.MoveToTarget(fountainTarget, () => {
                    // 4. CUTSCENE END: Switch back to standard player camera view
                    if (pipCutsceneCamera != null)
                    {
                        pipCutsceneCamera.gameObject.SetActive(false);
                        if (mainCam != null) mainCam.gameObject.SetActive(true);
                    }

                    // 5. Release player movement controls
                    SetPlayerControlState(true);

                });
            };
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

    public void StartDialogue(DialogueLine[] lines, string npcName, Sprite npcPortrait,
        Sprite ignoredPlayerPortrait, Camera cam, Action onComplete = null)
    {
        if (lines == null || lines.Length == 0) return;

        SetPlayerControlState(false);

        this.npcName = npcName;
        this.npcPortrait = npcPortrait;

        if (onComplete != null)
        {
            this.OnDialogueEnd = onComplete;
        }

        if (cam != null)
        {
            previousCamera = Camera.main;
            dialogueCamera = cam;

            dialogueCamera.gameObject.SetActive(true);
            if (previousCamera != null) previousCamera.gameObject.SetActive(false);
        }

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

        if (CharacterSelection.Selected == 1)
        {
            runtimePlayerPortrait = pennPortrait;
        }
        else
        {
            runtimePlayerPortrait = paigePortrait;
        }

        SetupPortrait(leftPortrait, npcPortrait);
        SetupPortrait(rightPortrait, runtimePlayerPortrait);

        Highlight(leftPortrait, !isPlayer);
        Highlight(rightPortrait, isPlayer);

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
        if (img == null || !img.gameObject.activeSelf || img.sprite == null) return;
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

        // If a cutscene callback is assigned to OnDialogueEnd, let it handle re-enabling control!
        if (OnDialogueEnd == null)
        {
            SetPlayerControlState(true);
        }

        Action cb = OnDialogueEnd;
        OnDialogueEnd = null;
        cb?.Invoke();
    }

    private void SetPlayerControlState(bool enable)
    {
        Charactercontroller activePlayer = FindFirstObjectByType<Charactercontroller>();
        if (activePlayer != null)
        {
            activePlayer.canControl = enable;

            Cursor.lockState = enable ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !enable;
        }
    }

    string FormatString(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        return s.Replace("{player}", CharacterSelection.SelectedName);
    }
}