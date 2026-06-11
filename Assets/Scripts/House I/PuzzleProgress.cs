// Tiny shared flags so the puzzle scene can tell MainWorld what happened.
// Survives the scene swap because it's static.
public static class PuzzleProgress
{
    // one-shot: set true on return, makes the NPC play their thank-you lines ONCE (cleared after)
    public static bool HouseISolved = false;
    public static bool HouseASolved = false;

    // permanent: a House puzzle has been completed - used to stop the player replaying it
    public static bool HouseIComplete = false;
    public static bool HouseAComplete = false;

    // permanent: which vowel stones the player has earned (one per house)
    public static bool HasVowelIStone = false;
    public static bool HasVowelAStone = false;
    public static bool HasVowelEStone = false;
    public static bool HasVowelOStone = false;
    public static bool HasVowelUStone = false;

    // true only when the player has earned all five - unlocks the ending ritual
    public static bool HasAllVowelStones =>
        HasVowelIStone && HasVowelAStone && HasVowelEStone && HasVowelOStone && HasVowelUStone;
}
