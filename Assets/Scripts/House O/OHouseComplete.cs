using BookChoice;
using UnityEngine;

public class OHouseComplete : MonoBehaviour
{
    public void GoToGame()
    {
        SceneController.Instance
            .NewTransition()
            .Load(SceneDatabase.Slots.Session, SceneDatabase.Scenes.Session)
            .Load(SceneDatabase.Slots.SessionContent, SceneDatabase.Scenes.MainWorld, setActive: true)
            .Unload(SceneDatabase.Scenes.HouseO) // Updated to unload O
            .WithOverlay()
            .WithClearUnusedAssets()
            .Perform();
    }
}