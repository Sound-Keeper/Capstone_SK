using BookChoice;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

public class EHouseComplete : MonoBehaviour
{
    public void GoToGame()
    {
        SceneController.Instance
            .NewTransition()
            .Load(SceneDatabase.Slots.Session, SceneDatabase.Scenes.Session)
            .Load(SceneDatabase.Slots.SessionContent, SceneDatabase.Scenes.MainWorld, setActive: true)
            .Unload(SceneDatabase.Scenes.HouseE)
            .WithOverlay()
            .WithClearUnusedAssets()
            .Perform();
    }
}
