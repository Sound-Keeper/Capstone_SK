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
    public Sprite paigePortrait;
    public Sprite pennPortrait;

    public Color activeTint = Color.white;
    public Color inactiveTint = new Color(0.45f, 0.45f, 0.45f, 1f);

    [Header("Typing Effect")]
    public float typingSpeed = 0.03f;

    [Header("Pip Intro Sequence Setup")]
    public PipFly pipFly;
    public Sprite pipIntroPortrait;
    public Transform fountainTarget;
    public float arriveDistance = 3f;

    [Header("Cinematic Cutscene Setup")]
    public Camera pipCutsceneCamera;

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
    private Coroutine typingCoroutine;

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

        // --- DEBUG LOGS START ---
        Debug.Log($"[DIAL_MGR START] --- Checking Static Variables ---");
        Debug.Log($"[DIAL_MGR START] DialogueManager.hasPlayedPipIntroFinished = {hasPlayedPipIntroFinished}");
        Debug.Log($"[DIAL_MGR START] DialogueManager.hasPlayedPipIntro = {hasPlayedPipIntro}");
        Debug.Log($"[DIAL_MGR START] PuzzleProgress.HouseASolved = {PuzzleProgress.HouseASolved}");
        Debug.Log($"[DIAL_MGR START] PuzzleProgress.HouseESolved = {PuzzleProgress.HouseESolved}");
        Debug.Log($"[DIAL_MGR START] PuzzleProgress.HouseISolved = {PuzzleProgress.HouseISolved}");
        Debug.Log($"[DIAL_MGR START] PuzzleProgress.HouseOSolved = {PuzzleProgress.HouseOSolved}");
        Debug.Log($"[DIAL_MGR START] PuzzleProgress.HouseUSolved = {PuzzleProgress.HouseUSolved}");
        // --- DEBUG LOGS END ---

        bool anyHouseSolved = PuzzleProgress.HouseASolved || PuzzleProgress.HouseESolved ||
                              PuzzleProgress.HouseISolved || PuzzleProgress.HouseOSolved ||
                              PuzzleProgress.HouseUSolved;

        if (anyHouseSolved || hasPlayedPipIntroFinished)
        {
            Debug.LogWarning("[DIAL_MGR START] Trigger condition met to SKIP intro sequence! Bypassing to finished state.");
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
            Debug.Log("[DIAL_MGR START] Conditions met successfully! Launching Pip Intro Dialogue.");
            hasPlayedPipIntro = true;
            SetPlayerControlState(false);
            string[] introLines = new string[] {
                "Hoo! Oh, thank goodness you're finally here, little one!",
                "Don't be afraid. My name is Pip. Welcome to Word Valley.",
                "This valley lives inside The Sound Book. It used to be the brightest, happiest place full of singing letters and laughing words.",
                "But a witch named Miss Spell grew jealous of our magic. She cast the mush-mush curse over the whole valley.",
                "Worst of all, she sealed our five Vowel Stones, <i>A, E, I, O, and U</i>. They hold the magic that keeps Word Valley alive.",
                "I searched a long, long time for someone with a kind heart and a brave spirit... and The Sound Book chose <i>you</i>.",
                "You are our <i>Sound Keeper</i>. Only you can wake the Vowel Stones and bring the valley back to life.",
                "I know reading can feel hard sometimes. I saw how you felt back in your classroom. But trust me here, every word you fix makes *you* stronger too.",
                "Take this magic wand. Point it, click it, and it will help you move, place, and choose. That's all you need!",
                "Keep up, Sound Keeper!"
            };

            // --- FIXED: Define the callback BEFORE starting the dialogue ---
            OnDialogueEnd = () => {
                SetPlayerControlState(false);
                Camera mainCam = Camera.main;
                if (pipCutsceneCamera != null)
                {
                    if (mainCam != null) mainCam.gameObject.SetActive(false);
                    pipCutsceneCamera.gameObject.SetActive(true);
                }

                pipFly.MoveToTarget(fountainTarget, () =>
                {
                    if (pipCutsceneCamera != null)
                    {
                        pipCutsceneCamera.gameObject.SetActive(false);
                        if (mainCam != null) mainCam.gameObject.SetActive(true);
                    }

                    // REMOVED: hasPlayedPipIntroFinished = true; <-- REMOVE THIS LINE!
                    // We let PipInteraction flip this flag AFTER you talk to him at the fountain.

                    SetPlayerControlState(true);
                });
            };

            StartDialogue("Pip", introLines, pipIntroPortrait);
        }
        else
        {
            // Logging if the conditions failed to initialize the cutscene
            Debug.LogWarning($"[DIAL_MGR START] Bypassed actual intro sequence trigger block. " +
                             $"pipFly missing? {pipFly == null} | fountainTarget missing? {fountainTarget == null}");
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
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);

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

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        typingCoroutine = StartCoroutine(TypeLine(FormatString(line.text)));
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
        if (currentLines != null && currentLineIndex == 8)
        {
            MagicWandReward[] wands = Resources.FindObjectsOfTypeAll<MagicWandReward>();
            MagicWandReward wandScript = wands.Length > 0 ? wands[0] : null;

            if (wandScript != null)
            {
                if (dialoguePanel != null) dialoguePanel.SetActive(false);

                wandScript.GiveWand(() => {
                    currentLineIndex++;
                    if (dialoguePanel != null) dialoguePanel.SetActive(true);

                    if (currentLines != null && currentLineIndex < currentLines.Length)
                    {
                        ShowLine();
                    }
                    else
                    {
                        EndDialogue();
                    }
                });
                return;
            }
            else
            {
                Debug.LogWarning("Could not find MagicWandReward anywhere in the scene assets!");
            }
        }

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

        if (OnDialogueEnd == null)
        {
            SetPlayerControlState(true);
        }

        Action cb = OnDialogueEnd;
        OnDialogueEnd = null;
        cb?.Invoke();
    }

    public void SetPlayerControlState(bool enable)
    {
        if (!enable)
        {
            StartCoroutine(WaitAndLockPlayer());
        }
        else
        {
            StopAllCoroutines();
            ApplyControl(true);
        }
    }

    private IEnumerator WaitAndLockPlayer()
    {
        Charactercontroller activePlayer = null;
        while (activePlayer == null)
        {
            activePlayer = FindFirstObjectByType<Charactercontroller>();
            if (activePlayer != null)
            {
                activePlayer.canControl = false;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                yield break;
            }
            yield return null;
        }
    }

    private void ApplyControl(bool enable)
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