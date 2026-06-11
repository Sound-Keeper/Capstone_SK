using UnityEngine;

public static class CharacterSelection
{
    //for character selection thing - remembers which player was picked
    public const int None = -1;
    public static int Selected = None;

    public static bool HasSelected => Selected != None;

    //name per character index. polyart wizard = Penny for now (no other asset yet)
    static readonly string[] names = { "Penny" };

    //who the player picked - used by Pip's dialogue to call the player by name
    public static string SelectedName
    {
        get
        {
            if (Selected >= 0 && Selected < names.Length) return names[Selected];
            return "Penny";
        }
    }
}
