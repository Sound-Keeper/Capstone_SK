using UnityEngine;

public class testhousescript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void TestHouse()
    {
        SceneController.Instance
            .NewTransition()
            .Load(SceneDatabase.Slots.Session, SceneDatabase.Scenes.Session)
            .Load(SceneDatabase.Slots.SessionContent, SceneDatabase.Scenes.HouseE, setActive:true)
            .Unload(SceneDatabase.Slots.Menu)
            .WithClearUnusedAssets()
            .Perform();
    }
}
