using UnityEngine;

public class FirstMenuManager : MonoBehaviour
{
    //for character selection thing - swaps FirstMenu out for MainMenu (same Menu slot auto-unloads FirstMenu)
    public void GoToMainMenu()
    {
        SceneController.Instance
            .NewTransition()
            .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.MainMenu)
            .Perform();
    }
}
