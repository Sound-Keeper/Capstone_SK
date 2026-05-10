using UnityEngine;
using UnityEngine.SceneManagement;

public class HouseManager : MonoBehaviour
{
    public static HouseManager Instance;

    [Header("UI")]
    public GameObject winPanel;

    [Header("Puzzle")]
    public int lettersNeeded = 2;
    private int lettersPlaced = 0;

    [Header("Scene")]
    public string mainSceneName = "MainScene";

    void Awake()
    {
        Instance = this;

        if (winPanel != null)
            winPanel.SetActive(false);
    }

    public void LetterPlaced()
    {
        lettersPlaced++;

        if (lettersPlaced >= lettersNeeded)
        {
            ShowWin();
        }
    }

    void ShowWin()
    {
        if (winPanel != null)
            winPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    public void ReturnToMainScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainSceneName);
    }
}