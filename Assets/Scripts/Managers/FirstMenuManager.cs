using UnityEngine;

public class FirstMenuManager : MonoBehaviour
{
    public void GoToMainMenu()
    {

        SceneController.Instance
            .NewTransition()
            .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.SecondMenu)
            .Unload(SceneDatabase.Scenes.FirstMenu)
            .Perform();
    }
}