using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement; // 🌟 Required to check current scene names

public class PauseMenu : MonoBehaviour
{
    public static bool GameisPaused = false;
    public GameObject pauseMenu;
    public GameObject pauseMenuUI;
    public GameObject settingsMenuUI;

    void Update()
    {
        if (InputSystem.actions.FindAction("Menu").triggered)
        {
            if (GameisPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(true);
        pauseMenu.SetActive(false);
        settingsMenuUI.SetActive(false);
        Time.timeScale = 1.0f;
        GameisPaused = false;

        // 🌟 Check if we are currently inside one of the UI puzzle scenes
        if (IsInsideUIPuzzleScene())
        {
            // Keep the cursor visible and free for the puzzle UI interaction!
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            // Lock the cursor back up for normal gameplay
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void Pause()
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0;
        GameisPaused = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // 🌟 Helper method to dynamically detect your UI puzzle scenes
    private bool IsInsideUIPuzzleScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        // 🚨 REPLACE these strings with the exact names of your House E and O puzzle scenes
        return currentScene == "HouseE" || currentScene == "HouseO";
    }

    public void LoadMenu()
    {
        Charactercontroller character = FindAnyObjectByType<Charactercontroller>();
        if (character != null && CoreManager.Instance != null)
        {
            CoreManager.Instance.SavePlayerPosition(
                character.transform.position,
                character.transform.rotation
            );
            Debug.Log("Position saved: " + character.transform.position);
        }

        pauseMenuUI.SetActive(false);
        Time.timeScale = 1.0f;
        GameisPaused = false;
        SceneController.Instance
            .NewTransition()
            .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.MainMenu)
            .Unload(SceneDatabase.Slots.Session)
            .Unload(SceneDatabase.Slots.SessionContent)
            .WithClearUnusedAssets()
            .WithOverlay()
            .Perform();
    }
}