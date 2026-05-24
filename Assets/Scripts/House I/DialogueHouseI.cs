using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

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
        if (isTalking) return;

        currentLines = lines;
        currentLine = 0;
        onFinished = onFinish;
        isTalking = true;

        //pag switch ng camera - check if smooth if yes keep
        if(mainCamera != null) mainCamera.enabled = false;
        if (npcDialogueCam != null)
        {
            dialogueCamera = npcDialogueCam;
            dialogueCamera.enabled = true;
        }

        //Test if mag show ang UI
        if(dialoguePanel != null) mainCamera.enabled=true;
        if(nameText != null) nameText.text = speakerName;
        if(bodyText != null) bodyText.text =lines[0];
    }

    void NextLine()
    {
        currentLine++;

        if (currentLine >= currentLines.Length) { EndDialogue();return; }

    }
    void EndDialogue()
    {
        isTalking = false;

        if(dialogueCamera != null) dialoguePanel.SetActive(false);
        if(dialogueCamera != null) dialogueCamera.enabled=false;
        if (mainCamera != null) mainCamera.enabled = true;

        if (onFinished != null)
        { onFinished.Invoke(); onFinished = null; }
    }

}
