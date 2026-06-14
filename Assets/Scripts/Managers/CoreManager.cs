using UnityEngine;

public class CoreManager : MonoBehaviour
{
    public static CoreManager Instance { get; private set; }



    // Player state
    public Vector3 SavedPlayerPosition { get; private set; }
    public Quaternion SavedPlayerRotation { get; private set; }
    public bool HasSavedPosition { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // There's already one — destroy THIS newcomer, keep the original
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        SceneController.Instance
            .NewTransition()
            .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.FirstMenu)
            .Perform();
    }

    // Call this before ANY scene transition that should return to this position
    public void SavePlayerPosition(Vector3 position, Quaternion rotation)
    {
        SavedPlayerPosition = position;
        SavedPlayerRotation = rotation;
        HasSavedPosition = true;
    }

    // Call this only when going to Main Menu (fresh start next time)
    public void ClearSavedPosition()
    {
        HasSavedPosition = false;
    }
}