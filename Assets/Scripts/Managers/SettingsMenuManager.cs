using UnityEngine;

public class SettingsMenuManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    public void GoBack()
    {
        SceneController.Instance
            .NewTransition()
            .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.SecondMenu)
            .Unload(SceneDatabase.Scenes.Settings)
            .WithOverlay()
            .Perform();
    }
}
