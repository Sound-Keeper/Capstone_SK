using UnityEngine;

public class VowelstoneUIActivator : MonoBehaviour
{
    [Header("Stone GameObjects")]
    [Tooltip("Assign the Astone_Prefab instance from your hierarchy here")]
    public GameObject UIstoneA;
    [Tooltip("Assign the Estone_Prefab instance from your hierarchy here")]
    public GameObject UIstoneE;
    [Tooltip("Assign the Istone_Prefab instance from your hierarchy here")]
    public GameObject UIstoneI;
    [Tooltip("Assign the Ostone_Prefab instance from your hierarchy here")]
    public GameObject UIstoneO;
    [Tooltip("Assign the Ustone_Prefab instance from your hierarchy here")]
    public GameObject UIstoneU;
    void Start()
    {
        UIStoneVisibility();
    }

    public void UIStoneVisibility()
    {
        if (UIstoneA != null) UIstoneA.SetActive(PuzzleProgress.HasVowelAStone);
        if (UIstoneE != null) UIstoneE.SetActive(PuzzleProgress.HasVowelEStone);
        if (UIstoneI != null) UIstoneI.SetActive(PuzzleProgress.HasVowelIStone);
        if (UIstoneO != null) UIstoneO.SetActive(PuzzleProgress.HasVowelOStone);
        if (UIstoneU != null) UIstoneU.SetActive(PuzzleProgress.HasVowelUStone);
    }
}
