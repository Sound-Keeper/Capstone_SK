using UnityEngine;
using UnityEngine.Playables;

public class IntroSceneDirector : MonoBehaviour
{
    [Header("Timeline Settings")]
    [Tooltip("Drag the PlayableDirector running your Intro timeline here.")]
    public PlayableDirector introTimeline;

    private bool isTransitioning = false;

    void Start()
    {
        // Smoothly fade out ambient/menu music so cutscene audio plays clearly
        CoreAudioManager.FadeOutBGM(0.5f);

        if (introTimeline != null)
        {
            introTimeline.stopped += OnTimelineFinished;
        }
    }

    void OnDestroy()
    {
        if (introTimeline != null)
        {
            introTimeline.stopped -= OnTimelineFinished;
        }
    }

    // Optional: Allow the player to press Space to skip the cutscene
    void Update()
    {
        if (!isTransitioning && Input.GetKeyDown(KeyCode.Space))
        {
            ProceedToMainWorld();
        }
    }

    private void OnTimelineFinished(PlayableDirector director)
    {
        ProceedToMainWorld();
    }

    public void ProceedToMainWorld()
    {
        if (isTransitioning) return;
        isTransitioning = true;

        CoreAudioManager.FadeInBGM(1f, 0.5f);

        Debug.Log("[IntroDirector] Cutscene finished. Transitioning to MainWorld...");

        // Unload the current Intro cutscene and switch active scene to MainWorld / MapTest[cite: 6]
        SceneController.Instance.NewTransition()
            .Load(SceneDatabase.Slots.Session, SceneDatabase.Scenes.Session)
            .Load(SceneDatabase.Slots.SessionContent, SceneDatabase.Scenes.MainWorld, setActive: true)
            .WithOverlay()
            .Perform();
    }
}