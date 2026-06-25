using UnityEngine;
using System.Collections;

namespace BookChoice
{
    public class HouseUPuzzleManager : MonoBehaviour
    {
        public static HouseUPuzzleManager Instance { get; private set; }

        [Header("References")]
        [Tooltip("The VowelStone instance in the scene.")]
        public VowelStone vowelStone;

        [Header("Bookshelf Configuration")]
        [Tooltip("Drag the letter target transform placeholders on the active world spaces/signs here in order (1, 2, 3).")]
        public Transform[] signTargetSlots;

        private int currentShelfIndex = 0;

        void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }
        }

        public Transform GetActiveSignTargetSlot()
        {
            if (currentShelfIndex < signTargetSlots.Length)
            {
                return signTargetSlots[currentShelfIndex];
            }
            return null;
        }

        public void OnShelfCompleted()
        {
            currentShelfIndex++;

            if (currentShelfIndex >= 3)
            {
                TriggerPuzzleCompletionSequence();
            }
        }

        private void TriggerPuzzleCompletionSequence()
        {
            Debug.Log("[HouseUPuzzleManager] All 3 bookshelves cleared! Starting reward sequence.");

            if (vowelStone != null)
            {
                // Set up what happens right after the VowelStone flies into the player and hides
                vowelStone.OnRewardFinished.AddListener(OnVowelStoneCutsceneFinished);
                vowelStone.GiveReward();
            }
            else
            {
                OnVowelStoneCutsceneFinished();
            }
        }

        private void OnVowelStoneCutsceneFinished()
        {
            if (vowelStone != null)
                vowelStone.OnRewardFinished.RemoveListener(OnVowelStoneCutsceneFinished);

            // --- STEP 1: SET GLOBAL FLAGS ---
            PuzzleProgress.HouseUSolved = true;
            PuzzleProgress.HouseUComplete = true;
            PuzzleProgress.HasVowelUStone = true;

            // --- STEP 2: FINISH UP ---
            // We do nothing else here! Control naturally unlocks back to the player,
            // allowing them to walk up to the NPC and interact with them to leave.
            Debug.Log("[HouseUPuzzleManager] Flags set successfully. Waiting for player to talk to the NPC.");
        }
    }
}