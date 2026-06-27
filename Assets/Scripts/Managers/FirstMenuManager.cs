using UnityEngine;

public class FirstMenuManager : MonoBehaviour
{
    // Hook this to your "Start Game" button in the FirstMenu Scene
    public void GoToCharacterSelection()
    {
        SceneController.Instance
            .NewTransition()
            .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.SecondMenu)
            .Unload(SceneDatabase.Scenes.MainMenu)
            .WithOverlay()
            .Perform();
    }

    public void QuitGame()
    {
        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}