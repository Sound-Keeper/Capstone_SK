using UnityEngine;

public class TutorialUIManager : MonoBehaviour
{
    [Header("UI Panel Reference")]
    [Tooltip("Drag the parent Tutorial UI Panel GameObject here.")]
    public GameObject tutorialUIPanel;

    void Start()
    {
        // Ensure the tutorial panel is closed when the scene starts
        if (tutorialUIPanel != null)
        {
            tutorialUIPanel.SetActive(false);
        }
    }

    void Update()
    {
        // Check if the Escape key is pressed and the panel is currently open
        if (Input.GetKeyDown(KeyCode.Escape) && tutorialUIPanel != null && tutorialUIPanel.activeSelf)
        {
            CloseTutorial();
        }
    }

    /// <summary>
    /// Opens the tutorial UI panel. Hook this up to your Tutorial Button OnClick().
    /// </summary>
    public void OpenTutorial()
    {
        if (tutorialUIPanel != null)
        {
            tutorialUIPanel.SetActive(true);
        }
    }

    /// <summary>
    /// Closes the tutorial UI panel. Hook this up to your Close Button OnClick().
    /// </summary>
    public void CloseTutorial()
    {
        if (tutorialUIPanel != null)
        {
            tutorialUIPanel.SetActive(false);
        }
    }
}