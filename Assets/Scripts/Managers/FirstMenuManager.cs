using UnityEngine;

public class FirstMenuManager : MonoBehaviour
{
    // Hook this to your "Start Game" button in the FirstMenu Scene
    public void GoToCharacterSelection()
    {
        SceneController.Instance
            .NewTransition()
            .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.SecondMenu)
            .Unload(SceneDatabase.Scenes.FirstMenu)
            .WithOverlay()
            .Perform();
    }
}