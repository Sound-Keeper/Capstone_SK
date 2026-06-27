using UnityEngine;
using UnityEngine.UI;

public class SecondMenuManager : MonoBehaviour
{
    [Header("UI Component Link")]
    [Tooltip("Drag your UI Button or CanvasGroup component representing the 'Continue' action here.")]
    public Button continueButton;

    private void Start()
    {
        // Automatically look up your persistent CoreManager state to check if a run exists
        if (continueButton != null)
        {
            if (CoreManager.Instance != null && CoreManager.Instance.HasSavedPosition)
            {
                continueButton.interactable = true; // Clickable if returning from PauseMenu
            }
            else
            {
                continueButton.interactable = false; // Greyed out on standard boots
            }
        }
    }

    // BUTTON 1: Linked to New Game / Character Select
    public void OpenCharacterSelectionScene()
    {
        PlayerPrefs.SetInt("IntroFinished", 0);
        PlayerPrefs.Save();

        DialogueManager.hasPlayedPipIntro = false;
        DialogueManager.hasPlayedPipIntroFinished = false;

        SceneController.Instance.NewTransition()
            .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.SecondMenu) // Reloading or switching
            .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.CharSelect)
            .Unload(SceneDatabase.Scenes.SecondMenu)
            .WithOverlay()
            .Perform();
    }

    public void GotoSettings()
    {
        SceneController.Instance.NewTransition()
            .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.Settings)
            .Unload(SceneDatabase.Scenes.SecondMenu)
            .WithOverlay()
            .Perform();
    }

    // BUTTON 2: Linked to Continue
    public void ActionContinueGame()
    {
        SceneController.Instance.NewTransition()
            .Load(SceneDatabase.Slots.Session, SceneDatabase.Scenes.Session)
            .Load(SceneDatabase.Slots.SessionContent, SceneDatabase.Scenes.MainWorld, setActive: true)
            .Unload(SceneDatabase.Slots.Menu)
            .WithOverlay()
            .Perform();
    }
}