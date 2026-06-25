using System.Collections.Generic;
using UnityEngine;

public class PipHint : MonoBehaviour
{
    [System.Serializable]
    public class HintObjective
    {
        public string houseLetter = "A";
        public Transform hoverLocation;

        [TextArea(2, 4)]
        public List<string> dialogueHints = new List<string>(); // Hint text

        [TextArea(2, 4)]
        public string completionDialogue = "Great job! Let's move to the next house."; // New field
    }

    [Header("Fountain Intro")]
    [TextArea(2, 4)]
    public List<string> fountainIntroDialogue = new List<string>();

    public PipFly pip;
    public List<HintObjective> objectives = new List<HintObjective>();

    // This returns the objective that isn't complete yet
    public HintObjective GetActiveObjective()
    {
        foreach (HintObjective o in objectives)
        {
            if (!PuzzleProgress.IsHouseComplete(o.houseLetter))
                return o;
        }
        return null;
    }
}