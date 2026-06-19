// Tiny shared flags so the puzzle scene can tell MainWorld what happened.
// Survives the scene swap because it's static.
public static class PuzzleProgress
{
    // 1. One-shot thank-you triggers (Cleared immediately after talking)
    public static bool HouseASolved = false;
    public static bool HouseESolved = false;
    public static bool HouseISolved = false;
    public static bool HouseOSolved = false;
    public static bool HouseUSolved = false;

    // 2. Permanent completion checkmarks (Locks the house doors)
    public static bool HouseAComplete = false;
    public static bool HouseEComplete = false;
    public static bool HouseIComplete = false;
    public static bool HouseOComplete = false;
    public static bool HouseUComplete = false;

    // 3. Permanent stone tracking (Used by Pip's guiding path)
    public static bool HasVowelAStone = false;
    public static bool HasVowelEStone = false;
    public static bool HasVowelIStone = false;
    public static bool HasVowelOStone = false;
    public static bool HasVowelUStone = false;

    // True only when the player has earned all five - unlocks the ending ritual
    public static bool HasAllVowelStones =>
        HasVowelAStone && HasVowelEStone && HasVowelIStone && HasVowelOStone && HasVowelUStone;

    // --- ADD THIS HELPER FUNCTION TO FIX THE ERROR ---
    public static bool IsHouseComplete(string houseLetter)
    {
        switch (houseLetter.ToUpper())
        {
            case "A": return HouseAComplete;
            case "E": return HouseEComplete;
            case "I": return HouseIComplete;
            case "O": return HouseOComplete;
            case "U": return HouseUComplete;
            default: return false;
        }
    }
}