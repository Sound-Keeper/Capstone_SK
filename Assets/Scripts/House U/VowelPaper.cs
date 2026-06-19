using UnityEngine;

public class VowelPaper : MonoBehaviour
{
    [Tooltip("The specific vowel this paper represents (e.g., 'A', 'E', 'I', 'O', 'U').")]
    public string assignedVowel;

    [Header("Manager Reference")]
    public UHouseManager houseManager;

    public void OnPaperClicked()
    {
        if (houseManager == null) return;

        // Pass both the character value AND this physical object to the manager
        houseManager.CheckVowelSelection(assignedVowel, this.gameObject);
    }
}