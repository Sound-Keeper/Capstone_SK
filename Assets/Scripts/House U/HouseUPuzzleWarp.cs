using UnityEngine;
using System.Collections;

namespace BookChoice
{
    public class HouseUPuzzleWarp : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("The scene we want to load back into.")]
        public string destinationScene = "MapTest";

        public void StartWarp()
        {
            // Fire the scene transition immediately to return to the MainWorld
            Debug.Log($"[HouseUPuzzleWarp] Puzzle complete! Changing scene to {destinationScene}...");
            SceneController.Instance
                .NewTransition()
                .Load(SceneDatabase.Slots.SessionContent, destinationScene, setActive: true)
                .WithOverlay()
                .WithClearUnusedAssets()
                .Perform();
        }
    }
}