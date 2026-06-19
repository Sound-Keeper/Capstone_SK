using UnityEngine;

public static class CharacterSelection
{
    // For character selection thing - remembers which player was picked
    public const int None = -1;
    public static int Selected = None; // 0 = Paige_Prefab, 1 = Penn_Prefab

    public static bool HasSelected => Selected != None;

    // The text names displayed in dialogue boxes. Order matches your prefabs!
    static readonly string[] names = { "Paige", "Penn" };

    // Who the player picked - used by Pip's dialogue to call the player by name
    public static string SelectedName
    {
        get
        {
            if (Selected >= 0 && Selected < names.Length) return names[Selected];
            return "Paige"; // Default fallback if no character is selected yet
        }
    }
}