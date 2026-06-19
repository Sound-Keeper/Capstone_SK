using UnityEngine;

public class ADialogueEntry : MonoBehaviour
{
    [Header("Scene Configuration")]
    [Tooltip("The exact name of the scene you want to load (e.g., HouseA).")]
    public string sceneToLoad = "HouseA";

    /// <summary>
    /// Call this function from your NpcInteraction's OnDialogueCompleteEvent!
    /// </summary>
    public void LoadTargetScene()
    {
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogWarning($"No scene name assigned on {gameObject.name}'s DialogueSceneLoader!");
            return;
        }

        Debug.Log($"Dialogue complete! Transitioning to scene: {sceneToLoad}");

        // Triggers your custom scene transition framework smoothly
        SceneController.Instance
            .NewTransition()
            .Load(SceneDatabase.Slots.SessionContent, sceneToLoad, setActive: true)
            .WithOverlay()
            .WithClearUnusedAssets()
            .Perform();
    }
}