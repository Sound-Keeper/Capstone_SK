using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectMenu : MonoBehaviour
{
    [Header("Selection Safety Rule")]
    [Tooltip("Drag your UI Confirm Button component here so we can toggle it.")]
    public Button confirmButton;

    private int localSelectedIdx = -1; // Temp placeholder tracking selection state locally

    private void Start()
    {
        // Confirm is completely locked out until a portrait is chosen
        if (confirmButton != null) confirmButton.interactable = false;
    }

    // Linked to Paige's Portrait Button (Index 0)
    public void ClickPortraitPaige()
    {
        localSelectedIdx = 0;
        Debug.Log("Selected Paige (Index 0)");
        if (confirmButton != null) confirmButton.interactable = true;
    }

    // Linked to Penn's Portrait Button (Index 1)
    public void ClickPortraitPenn()
    {
        localSelectedIdx = 1;
        Debug.Log("Selected Penn (Index 1)");
        if (confirmButton != null) confirmButton.interactable = true;
    }

    // Linked to the Master CONFIRM Button
    public void CommitSelectionAndLaunch()
    {
        if (localSelectedIdx == -1) return; // Safeguard guard

        // 1. Commit selection globally
        CharacterSelection.Selected = localSelectedIdx;
        Debug.Log($"Final selection locked: {CharacterSelection.SelectedName}");

        // Clear previous state and saved data for a fresh run
        PlayerPrefs.DeleteKey("IsEndGameCompleted");
        PlayerPrefs.Save();

        if (CoreManager.Instance != null) CoreManager.Instance.ClearSavedPosition();
        PuzzleProgress.ResetAllProgress();

        // 2. Load the specific intro scene based on character choice
        if (CharacterSelection.Selected == 0)
        {
            // Load Paige's Intro Scene
            SceneController.Instance.NewTransition()
                .Load(SceneDatabase.Slots.SessionContent, SceneDatabase.Scenes.IntroPaige, setActive: true)
                .Unload(SceneDatabase.Slots.Menu)
                .WithOverlay()
                .Perform();
        }
        else
        {
            // Load Penn's Intro Scene
            SceneController.Instance.NewTransition()
                .Load(SceneDatabase.Slots.SessionContent, SceneDatabase.Scenes.IntroPenn, setActive: true)
                .Unload(SceneDatabase.Slots.Menu)
                .WithOverlay()
                .Perform();
        }
    }

    public void GoBack()
    {
        SceneController.Instance
            .NewTransition()
            .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.SecondMenu)
            .Unload(SceneDatabase.Scenes.CharSelect)
            .WithOverlay()
            .Perform();
    }
}