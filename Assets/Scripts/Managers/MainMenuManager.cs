using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    public void StartSession()
    {


        SceneController.Instance
            .NewTransition()
            .Load(SceneDatabase.Slots.Session, SceneDatabase.Scenes.Session)
            .Load(SceneDatabase.Slots.SessionContent, SceneDatabase.Scenes.MainWorld, setActive: true)
            .Unload(SceneDatabase.Slots.Menu)
            .WithOverlay()
            .WithClearUnusedAssets()
            .Perform();

    }
}
