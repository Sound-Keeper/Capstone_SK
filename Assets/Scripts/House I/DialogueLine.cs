using UnityEngine;

// Who is speaking on a given line.
public enum Speaker
{
    NPC,
    Player
}

// One line of dialogue, fully editable in the Inspector.
// Add a list of these on each NPC and pick the speaker per line.
[System.Serializable]
public class DialogueLine
{
    [Tooltip("Who says this line? Switches the name shown automatically.")]
    public Speaker speaker = Speaker.NPC;

    [TextArea(2, 4)]
    [Tooltip("What this speaker says on this line.")]
    public string text;
}
