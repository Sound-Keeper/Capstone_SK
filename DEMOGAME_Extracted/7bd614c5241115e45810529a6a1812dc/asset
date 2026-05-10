using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PennyDialogue : MonoBehaviour
{
    public GameObject dialoguePanel;
    public TMP_Text nameText;
    public TMP_Text bodyText;
    public TMP_Text hintText;
    public GameObject goButton;

    [TextArea(2, 5)]
    public string[] lines;

    private int currentLine = 0;
    private bool isTalking = false;

    void Start()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (goButton != null)
            goButton.SetActive(false);
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

        if (nameText != null)
            nameText.text = "Penny Cil";

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

        if (goButton != null)
            goButton.SetActive(true);

        Time.timeScale = 1f;
    }
}