using System.Collections;
using TMPro;
using UnityEngine;

public class TypingScript : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI textComponent;

    [Header("Typing Settings")]
    [TextArea]
    public string fullText;

    [Tooltip("Time (in seconds) between each character")]
    public float typingSpeed = 0.05f;

    private void Start()
    {
        StartCoroutine(TypeText());
    }

    IEnumerator TypeText()
    {
        textComponent.text = "";

        foreach (char letter in fullText.ToCharArray())
        {
            textComponent.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }
}