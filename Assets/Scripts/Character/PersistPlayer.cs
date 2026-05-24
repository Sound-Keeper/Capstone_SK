using UnityEngine;

public class PersistPlayer : MonoBehaviour
{
    //Purpose of this script is to let the unity knows that its smooth to transition to another scene
    //without destroyin the player charcter
    //reference is the pokemon script found in YT and Unity website
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

}
