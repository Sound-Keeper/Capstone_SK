using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class DialogueHouseI : MonoBehaviour
{
    //Test for smooth camera transition
    public static DialogueHouseI Instance;

    [Header("UI")]
    public GameObject dialoguePanel;
    public TMP_Text nameText;
    public TMP_Text bodyText;

    [Header("Camera")]
    public Camera mainCamera;
    public Camera dialogueCamera;

    bool isTalking = false;
    string[] currentLines;
    int currentLine = 0;
    System.Action onFinished;

    void Awake()
    {
        Instance = this;
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
    }

    void Update()
    {
        if (!isTalking) return;

        if (Keyboard.current.spaceKey.wasPressedThisFrame ||
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            NextLine();
        }
    }

    public void StartDialogue(string speakerName, string[] lines, Camera npcDialogueCam,
        System.Action onFinish = null)
    {
        Debug.Log("StartDialogue called for: " + speakerName);

        if (isTalking) return;

        //test if refs are set
        if (dialoguePanel == null) { Debug.LogError("dialoguePanel ref is NULL!"); return; }
        if (nameText == null) { Debug.LogError("nameText ref is NULL!"); return; }
        if (bodyText == null) { Debug.LogError("bodyText ref is NULL!"); return; }

        //save dialogue state
        currentLines = lines;
        currentLine = 0;
        onFinished = onFinish;
        isTalking = true;

        //show the UI
        dialoguePanel.SetActive(true);
        nameText.text = speakerName;
        bodyText.text = lines[0];

        //pag switch ng camera - main off, dialogue cam on
        if (mainCamera != null) mainCamera.enabled = false;
        if (npcDialogueCam != null)
        {
            dialogueCamera = npcDialogueCam;
            dialogueCamera.enabled = true;
        }
    }

    void NextLine()
    {
        currentLine++;

        if (currentLine >= currentLines.Length)
        {
            EndDialogue();
            return;
        }

        //update body text to next line
        bodyText.text = currentLines[currentLine];
    }

    void EndDialogue()
    {
        isTalking = false;

        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (dialogueCamera != null) dialogueCamera.enabled = false;
        if (mainCamera != null) mainCamera.enabled = true;

        if (onFinished != null)
        {
            onFinished.Invoke();
            onFinished = null;
        }
    }
}