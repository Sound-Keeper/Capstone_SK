using UnityEngine;

public enum Speaker
{
    NPC,
    Player
}

[System.Serializable]
public class DialogueLine
{
    [Tooltip("Who says this line? Switches the name shown automatically.")]
    public Speaker speaker = Speaker.NPC;

    [TextArea(2, 4)]
    [Tooltip("What this speaker says on this line.")]
    public string text;

    [Tooltip("Drag and drop the specific voiceover audio clip for this line here.")]
    public AudioClip voiceoverClip; // <--- ADDED FIELD
}