using UnityEngine;

public class CharacterSelectMenu : MonoBehaviour
{
    //for character selection thing - hook each character button's OnClick to Select(index)
    public void Select(int characterIndex)
    {
        CharacterSelection.Selected = characterIndex;
    }
}
