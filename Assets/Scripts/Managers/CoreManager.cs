using UnityEngine;

public class CoreManager : MonoBehaviour
{
    void Start()
    {
        //Core Setup
        //Load BgAudio, Save system
        SceneController.Instance
            .NewTransition()
            .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.MainMenu)
            .Perform();
    }

}
