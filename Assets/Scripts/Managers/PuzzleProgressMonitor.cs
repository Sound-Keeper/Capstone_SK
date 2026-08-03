#region//working

//using UnityEngine;

//public class PuzzleProgressMonitor : MonoBehaviour
//{
//    [Header("One-Shot Solved Flags")]
//    public bool HouseASolved;
//    public bool HouseESolved;
//    public bool HouseISolved;
//    public bool HouseOSolved;
//    public bool HouseUSolved;

//    [Header("Permanent Completion Flags")]
//    public bool HouseAComplete;
//    public bool HouseEComplete;
//    public bool HouseIComplete;
//    public bool HouseOComplete;
//    public bool HouseUComplete;

//    [Header("Vowel Stone Tracking")]
//    public bool HasVowelAStone;
//    public bool HasVowelEStone;
//    public bool HasVowelIStone;
//    public bool HasVowelOStone;
//    public bool HasVowelUStone;

//    // Update is called once per frame
//    void Update()
//    {
//        // Copy the hidden static parameters into these public variables every frame
//        HouseASolved = PuzzleProgress.HouseASolved;
//        HouseESolved = PuzzleProgress.HouseESolved;
//        HouseISolved = PuzzleProgress.HouseISolved;
//        HouseOSolved = PuzzleProgress.HouseOSolved;
//        HouseUSolved = PuzzleProgress.HouseUSolved;

//        HouseAComplete = PuzzleProgress.HouseAComplete;
//        HouseEComplete = PuzzleProgress.HouseEComplete;
//        HouseIComplete = PuzzleProgress.HouseIComplete;
//        HouseOComplete = PuzzleProgress.HouseOComplete;
//        HouseUComplete = PuzzleProgress.HouseUComplete;

//        HasVowelAStone = PuzzleProgress.HasVowelAStone;
//        HasVowelEStone = PuzzleProgress.HasVowelEStone;
//        HasVowelIStone = PuzzleProgress.HasVowelIStone;
//        HasVowelOStone = PuzzleProgress.HasVowelOStone;
//        HasVowelUStone = PuzzleProgress.HasVowelUStone;
//    }
//}
#endregion

#region //changeable

using UnityEngine;

public class PuzzleProgressMonitor : MonoBehaviour
{
    [Header("One-Shot Solved Flags")]
    public bool HouseASolved;
    public bool HouseESolved;
    public bool HouseISolved;
    public bool HouseOSolved;
    public bool HouseUSolved;

    [Header("Permanent Completion Flags")]
    public bool HouseAComplete;
    public bool HouseEComplete;
    public bool HouseIComplete;
    public bool HouseOComplete;
    public bool HouseUComplete;

    [Header("Vowel Stone Tracking")]
    public bool HasVowelAStone;
    public bool HasVowelEStone;
    public bool HasVowelIStone;
    public bool HasVowelOStone;
    public bool HasVowelUStone;

    // Pull data from the core class into the inspector while playing
    void Update()
    {
        if (!Application.isPlaying) return;

        HouseASolved = PuzzleProgress.HouseASolved;
        HouseESolved = PuzzleProgress.HouseESolved;
        HouseISolved = PuzzleProgress.HouseISolved;
        HouseOSolved = PuzzleProgress.HouseOSolved;
        HouseUSolved = PuzzleProgress.HouseUSolved;

        HouseAComplete = PuzzleProgress.HouseAComplete;
        HouseEComplete = PuzzleProgress.HouseEComplete;
        HouseIComplete = PuzzleProgress.HouseIComplete;
        HouseOComplete = PuzzleProgress.HouseOComplete;
        HouseUComplete = PuzzleProgress.HouseUComplete;

        HasVowelAStone = PuzzleProgress.HasVowelAStone;
        HasVowelEStone = PuzzleProgress.HasVowelEStone;
        HasVowelIStone = PuzzleProgress.HasVowelIStone;
        HasVowelOStone = PuzzleProgress.HasVowelOStone;
        HasVowelUStone = PuzzleProgress.HasVowelUStone;
    }

    // NEW: Push inspector edits back to the game data in real-time
    private void OnValidate()
    {
        if (!Application.isPlaying) return;

        PuzzleProgress.HouseASolved = HouseASolved;
        PuzzleProgress.HouseESolved = HouseESolved;
        PuzzleProgress.HouseISolved = HouseISolved;
        PuzzleProgress.HouseOSolved = HouseOSolved;
        PuzzleProgress.HouseUSolved = HouseUSolved;

        PuzzleProgress.HouseAComplete = HouseAComplete;
        PuzzleProgress.HouseEComplete = HouseEComplete;
        PuzzleProgress.HouseIComplete = HouseIComplete;
        PuzzleProgress.HouseOComplete = HouseOComplete;
        PuzzleProgress.HouseUComplete = HouseUComplete;

        PuzzleProgress.HasVowelAStone = HasVowelAStone;
        PuzzleProgress.HasVowelEStone = HasVowelEStone;
        PuzzleProgress.HasVowelIStone = HasVowelIStone;
        PuzzleProgress.HasVowelOStone = HasVowelOStone;
        PuzzleProgress.HasVowelUStone = HasVowelUStone;
    }
}
#endregion