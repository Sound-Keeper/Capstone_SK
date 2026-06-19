using UnityEngine;

public class VowelCube3D : MonoBehaviour
{
    [Tooltip("The letter this block represents (e.g., 'I' for Un_corn).")]
    public string assignedVowel;

    [Header("Manager Reference")]
    public Uhouse3DManager houseManager;

    // Called by your crosshair raycast script when clicked
    public void OnCubeClicked()
    {
        if (houseManager == null) return;
        houseManager.CheckVowelSelection(assignedVowel, this.gameObject);
    }
}