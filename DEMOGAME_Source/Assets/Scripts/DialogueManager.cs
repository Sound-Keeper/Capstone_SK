using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI")]
    public GameObject dialoguePanel;
    public TMP_Text nameText;
    public TMP_Text bodyText;
    public TMP_Text hintText;

    [Header("Cameras")]
    public Camera gameplayCamera;
    public Camera dialogueCamera;
    public Camera pennyDialogueCamera;

    [Header("Pip Default Dialogue")]
    public string speakerName = "Pip";
    [TextArea(2, 5)]
    public string[] lines;

    [Header("Pip Movement")]
    public PipFly pipFly;
    public Transform pennyCilTarget;

    [Header("Scene Loading")]
    public string houseISceneName = "HouseI";
    public float pennyTeleportDelay = 1f;

    int currentLine = 0;
    bool isTalking = false;

    void Awake()
    {
        Instance = this;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (dialogueCamera != null)
            dialogueCamera.gameObject.SetActive(false);

        if (pennyDialogueCamera != null)
            pennyDialogueCamera.gameObject.SetActive(false);
    }

    void Start()
    {
        // Only play intro the first time
        if (PlayerPrefs.GetInt("WordValleyIntroDone", 0) == 0)
        {
            StartDialogue();
        }
    }

    void Update()
    {
        if (!isTalking) return;

        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            NextLine();
        }
    }

    public void StartDialogue()
    {
        if (lines == null || lines.Length == 0) return;

        currentLine = 0;
        isTalking = true;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (gameplayCamera != null)
            gameplayCamera.gameObject.SetActive(false);

        if (dialogueCamera != null)
            dialogueCamera.gameObject.SetActive(true);

        if (pennyDialogueCamera != null)
            pennyDialogueCamera.gameObject.SetActive(false);

        if (nameText != null)
            nameText.text = speakerName;

        if (bodyText != null)
            bodyText.text = lines[currentLine];

        if (hintText != null)
            hintText.text = "Press Space to continue";

        Time.timeScale = 0f;
    }

    public void StartDialogue(string speaker, string[] newLines)
    {
        speakerName = speaker;
        lines = newLines;

        if (lines == null || lines.Length == 0) return;

        currentLine = 0;
        isTalking = true;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (gameplayCamera != null)
            gameplayCamera.gameObject.SetActive(false);

        if (speaker == "Penny Cil")
        {
            if (dialogueCamera != null)
                dialogueCamera.gameObject.SetActive(false);

            if (pennyDialogueCamera != null)
                pennyDialogueCamera.gameObject.SetActive(true);
        }
        else
        {
            if (dialogueCamera != null)
                dialogueCamera.gameObject.SetActive(true);

            if (pennyDialogueCamera != null)
                pennyDialogueCamera.gameObject.SetActive(false);
        }

        if (nameText != null)
            nameText.text = speakerName;

        if (bodyText != null)
            bodyText.text = lines[currentLine];

        if (hintText != null)
            hintText.text = "Press Space to continue";

        Time.timeScale = 0f;
    }

    void NextLine()
    {
        currentLine++;

        if (currentLine >= lines.Length)
        {
            EndDialogue();
            return;
        }

        if (bodyText != null)
            bodyText.text = lines[currentLine];
    }

    void EndDialogue()
    {
        isTalking = false;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (dialogueCamera != null)
            dialogueCamera.gameObject.SetActive(false);

        if (pennyDialogueCamera != null)
            pennyDialogueCamera.gameObject.SetActive(false);

        if (gameplayCamera != null)
            gameplayCamera.gameObject.SetActive(true);

        Time.timeScale = 1f;

      
        if (speakerName == "Pip")
        {
            PlayerPrefs.SetInt("WordValleyIntroDone", 1);
            PlayerPrefs.Save();

            if (pipFly != null && pennyCilTarget != null)
            {
                pipFly.MoveToTarget(pennyCilTarget);
            }
        }

        if (speakerName == "Penny Cil")
        {
            StartCoroutine(LoadHouseIAfterDelay());
        }
    }

    IEnumerator LoadHouseIAfterDelay()
    {
        yield return new WaitForSeconds(pennyTeleportDelay);
        SceneManager.LoadScene(houseISceneName);
    }
}