using UnityEngine;

public class SceneDialogue : MonoBehaviour
{
    void Start()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue();
        }
    }
}