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

        // --- REMOVED: public Transform[] signTargetSlots; array is no longer needed here! ---

        private int completedShelvesCount = 0; // Changed tracking to a simple counter

        void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }
        }

        // Keep this method here as a backup safety fallback method so old button scripts don't break!
        public Transform GetActiveSignTargetSlot()
        {
            return null;
        }

        public void OnShelfCompleted()
        {
            completedShelvesCount++; // Safely increments whenever ANY book finishes, regardless of order!

            if (completedShelvesCount >= 3)
            {
                TriggerPuzzleCompletionSequence();
            }
        }

        private void TriggerPuzzleCompletionSequence()
        {
            Debug.Log("[HouseUPuzzleManager] All 3 bookshelves cleared! Starting reward sequence.");

            if (vowelStone != null)
            {
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

            PuzzleProgress.HouseUSolved = true;
            PuzzleProgress.HouseUComplete = true;
            PuzzleProgress.HasVowelUStone = true;

            float targetVolume = VowelStone.PreCutsceneVolume;
            if (targetVolume <= 0.01f) targetVolume = 1f;
            CoreAudioManager.FadeInBGM(targetVolume, 1.0f);

            Debug.Log("[HouseUPuzzleManager] Flags set successfully. Waiting for player to talk to the NPC.");
        }
    }
}