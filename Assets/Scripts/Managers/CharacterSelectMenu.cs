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

        // 1. Commit layout variables globally into your static dictionary tracker
        CharacterSelection.Selected = localSelectedIdx;
        Debug.Log($"Final selection locked: {CharacterSelection.SelectedName}");

        // --- NEW: WIPE PLAYERPREFS & CUTSCENE COMPLETION STATES ---
        PlayerPrefs.DeleteKey("IsEndGameCompleted"); // Resets daytime visual override back to normal game state
        PlayerPrefs.Save();

        // 2. Since this is a fresh New Game run, wipe clean any residual checkpoint locations
        if (CoreManager.Instance != null) CoreManager.Instance.ClearSavedPosition();

        PuzzleProgress.ResetAllProgress();

        // 3. Complete structural load sequence into gameplay
        SceneController.Instance.NewTransition()
            .Load(SceneDatabase.Slots.Session, SceneDatabase.Scenes.Session)
            .Load(SceneDatabase.Slots.SessionContent, SceneDatabase.Scenes.MainWorld, setActive: true)
            .Unload(SceneDatabase.Slots.Menu) // Wipe out selection frames entirely
            .WithOverlay()
            .Perform();
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