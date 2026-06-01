using UnityEngine;

public class PersistPlayer : MonoBehaviour
{
    //Purpose of this script is to let the unity knows that its smooth to transition to another scene
    //without destroyin the player charcter
    //reference is the pokemon script found in YT and Unity website

    //singleton: there must only ever be ONE persistent player. The player is baked into
    //MainWorld, so when we return to MainWorld from House I the scene spawns a SECOND player
    //while the original (DontDestroyOnLoad) is still alive -> "two of me". The guard below
    //makes that fresh duplicate destroy itself, keeping the original (already positioned near Penny).
    public static PersistPlayer Instance;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

}
