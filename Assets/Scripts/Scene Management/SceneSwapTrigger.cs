using UnityEngine;
using UnityEngine.InputSystem;

// Put this on a trigger zone (a collider set to "Is Trigger").
// When the player walks in, a prompt shows; pressing the key seamlessly
// swaps to the target scene using your existing SceneController.
//
// Loading into the SAME slot that holds MainWorld makes SceneController
// unload MainWorld and load the new scene in its place - so only one heavy
// map is in memory at a time. The player (PersistPlayer) survives the swap.
[RequireComponent(typeof(Collider))]
public class SceneSwapTrigger : MonoBehaviour
{
    [Header("Target Scene")]
    [Tooltip("Scene to load, e.g. HouseI. Must be added to Build Settings.")]
    public string sceneToLoad = SceneDatabase.Scenes.HouseI;

    [Tooltip("Slot to load into. Use the SAME slot MainWorld lives in so it swaps out.")]
    public string sceneSlot = SceneDatabase.Slots.SessionContent;

    [Tooltip("Move the player to these coords after the swap. Type the position you want from the TARGET scene (you can't drag a Transform from a scene that isn't loaded yet).")]
    public bool useSpawnPoint = true;
    public Vector3 spawnPosition;
    public Vector3 spawnEulerRotation;

    [Header("Transition Feel")]
    [Tooltip("ON = quick black fade hides the load (Pokemon door style). OFF = instant swap, fully seamless if maps line up.")]
    public bool useFade = true;

    [Tooltip("Free memory from the scene that gets unloaded.")]
    public bool clearUnusedAssets = true;

    [Header("Prompt (Optional)")]
    [Tooltip("CanvasGroup of a 'Press E to enter' prompt. Fades in while the player is inside.")]
    public CanvasGroup promptCanvas;
    public float promptFadeSpeed = 5f;

    [Header("Input")]
    public string playerTag = "Player";

    bool playerInside = false;
    bool swapping = false;
    Transform player;

    void Reset()
    {
        //default the collider to a trigger when first added
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    void Start()
    {
        if (promptCanvas != null)
        {
            promptCanvas.alpha = 0f;
            promptCanvas.gameObject.SetActive(true);
        }
    }

    void Update()
    {
        //fade the prompt in/out
        if (promptCanvas != null)
        {
            float target = (playerInside && !swapping) ? 1f : 0f;
            promptCanvas.alpha = Mathf.MoveTowards(promptCanvas.alpha, target, promptFadeSpeed * Time.deltaTime);

            bool visible = promptCanvas.alpha > 0.001f;
            promptCanvas.interactable = visible;
            promptCanvas.blocksRaycasts = visible;
        }

        //confirm with E
        if (playerInside && !swapping &&
            Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            Swap();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerInside = true;
        player = other.transform;
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerInside = false;
    }

    void Swap()
    {
        if (SceneController.Instance == null)
        {
            Debug.LogError("SceneSwapTrigger: SceneController.Instance is NULL!");
            return;
        }

        swapping = true;

        //move the persistent player to the typed spawn coords
        if (player != null && useSpawnPoint)
        {
            Charactercontroller cc = player.GetComponent<Charactercontroller>();
            if (cc != null) cc.enabled = false;

            player.position = spawnPosition;
            player.rotation = Quaternion.Euler(spawnEulerRotation);

            if (cc != null) cc.enabled = true;
        }

        var plan = SceneController.Instance.NewTransition()
            .Load(sceneSlot, sceneToLoad, setActive: true);

        if (useFade) plan = plan.WithOverlay();
        if (clearUnusedAssets) plan = plan.WithClearUnusedAssets();

        plan.Perform();
    }
}
