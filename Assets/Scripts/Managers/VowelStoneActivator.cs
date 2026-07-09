using UnityEngine;

public class VowelStoneActivator : MonoBehaviour
{
    [Header("Stone GameObjects")]
    [Tooltip("Assign the Astone_Prefab instance from your hierarchy here")]
    public GameObject stoneA;
    public GameObject vinesA;
    [Tooltip("Assign the Estone_Prefab instance from your hierarchy here")]
    public GameObject stoneE;
    public GameObject vinesE;
    public GameObject boundingboxE;
    [Tooltip("Assign the Istone_Prefab instance from your hierarchy here")]
    public GameObject stoneI;
    public GameObject vinesI;
    public GameObject boundingboxI;
    [Tooltip("Assign the Ostone_Prefab instance from your hierarchy here")]
    public GameObject stoneO;
    public GameObject vinesO;
    public GameObject boundingboxO;
    [Tooltip("Assign the Ustone_Prefab instance from your hierarchy here")]
    public GameObject stoneU;
    public GameObject vinesU;
    public GameObject boundingboxU;

    [Header("Animation Settings")]
    [Tooltip("How fast the stones rotate (degrees per second)")]
    public float spinSpeed = 30f;

    private void Start()
    {
        // Check the status immediately when the scene loads
        RefreshStoneVisibility();
    }

    private void Update()
    {
        // Spin the stones continuously if they are active in the scene
        SpinStoneIfActive(stoneA);
        SpinStoneIfActive(stoneE);
        SpinStoneIfActive(stoneI);
        SpinStoneIfActive(stoneO);
        SpinStoneIfActive(stoneU);
    }


    public void RefreshStoneVisibility()
    {
        if (stoneA != null)
        {
            stoneA.SetActive(PuzzleProgress.HasVowelAStone);
            vinesA.SetActive(!PuzzleProgress.HasVowelAStone);
        }

        if (stoneE != null)
        {
            stoneE.SetActive(PuzzleProgress.HasVowelEStone);
            vinesE.SetActive(!PuzzleProgress.HasVowelEStone);
            boundingboxE.SetActive(!PuzzleProgress.HasVowelAStone);
        }
 
        if (stoneI != null)
        {
            stoneI.SetActive(PuzzleProgress.HasVowelIStone);
            vinesI.SetActive(!PuzzleProgress.HasVowelIStone);
            boundingboxI.SetActive(!PuzzleProgress.HasVowelEStone);
        }

        if (stoneO != null)
        {
            stoneO.SetActive(PuzzleProgress.HasVowelOStone);
            vinesO.SetActive(!PuzzleProgress.HasVowelOStone);
            boundingboxO.SetActive(!PuzzleProgress.HasVowelIStone);
        }
 
        if (stoneU != null)
        {
            stoneU.SetActive(PuzzleProgress.HasVowelUStone);
            vinesU.SetActive(!PuzzleProgress.HasVowelUStone);
            boundingboxU.SetActive(!PuzzleProgress.HasVowelOStone);
        }

    }

    /// <summary>
    /// Rotates the stone around its Y-axis (upwards) if it is visible.
    /// </summary>
    private void SpinStoneIfActive(GameObject stone)
    {
        if (stone != null && stone.activeSelf)
        {
            // Rotate smoothly over time, independent of frame rate
            stone.transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime);
        }
    }
}