// Tiny shared flags so the puzzle scene can tell MainWorld what happened.
// Survives the scene swap because it's static.

using System.Collections.Generic;
public static class PuzzleProgress
{
    public static HashSet<string> DiscoveredSignIDs = new HashSet<string>();
    public static int DiscoveredSignsCount => DiscoveredSignIDs.Count;
    public static float GlobalCurrentHealth = 100f;

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

    // --- CALL THIS TO WIPE ALL PROGRESS DATA ON NEW GAME ---
    public static void ResetAllProgress()
    {
        DiscoveredSignIDs.Clear();

        // 1. Reset Solved Triggers
        HouseASolved = false;
        HouseESolved = false;
        HouseISolved = false;
        HouseOSolved = false;
        HouseUSolved = false;

        // 2. Reset Permanent Completion Checkmarks
        HouseAComplete = false;
        HouseEComplete = false;
        HouseIComplete = false;
        HouseOComplete = false;
        HouseUComplete = false;

        // 3. Reset Stone Tracking Flags
        HasVowelAStone = false;
        HasVowelEStone = false;
        HasVowelIStone = false;
        HasVowelOStone = false;
        HasVowelUStone = false;

    }
}