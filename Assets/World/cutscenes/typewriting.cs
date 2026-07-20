using System.Collections;
using TMPro;
using UnityEngine;

public class typewriting : MonoBehaviour
{
    public TMP_Text textBox;
    public float delay = 0.03f; // seconds per letter

    public void PlayText(string fullText)
    {
        StopAllCoroutines();
        StartCoroutine(TypeText(fullText));
    }

    private IEnumerator TypeText(string fullText)
    {
        textBox.text = "";
        foreach (char letter in fullText)
        {
            textBox.text += letter;
            yield return new WaitForSeconds(delay);
        }
    }
}