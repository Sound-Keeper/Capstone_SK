using UnityEngine;

public class VowelStoneActivator : MonoBehaviour
{
    [Header("Stone GameObjects")]
    [Tooltip("Assign the Astone_Prefab instance from your hierarchy here")]
    public GameObject stoneA;
    [Tooltip("Assign the Estone_Prefab instance from your hierarchy here")]
    public GameObject stoneE;
    [Tooltip("Assign the Istone_Prefab instance from your hierarchy here")]
    public GameObject stoneI;
    [Tooltip("Assign the Ostone_Prefab instance from your hierarchy here")]
    public GameObject stoneO;
    [Tooltip("Assign the Ustone_Prefab instance from your hierarchy here")]
    public GameObject stoneU;

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
        if (stoneA != null) stoneA.SetActive(PuzzleProgress.HasVowelAStone); 
        if (stoneE != null) stoneE.SetActive(PuzzleProgress.HasVowelEStone); 
        if (stoneI != null) stoneI.SetActive(PuzzleProgress.HasVowelIStone); 
        if (stoneO != null) stoneO.SetActive(PuzzleProgress.HasVowelOStone); 
        if (stoneU != null) stoneU.SetActive(PuzzleProgress.HasVowelUStone); 
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