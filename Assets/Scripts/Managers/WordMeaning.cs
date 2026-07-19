using System;
using System.Collections;
using UnityEngine;

[System.Serializable]
public class WordDefinition
{
    public string wordName;          // e.g., "AID"
    [TextArea(3, 5)]
    public string wordMeaning;       // e.g., "To provide assistance..."

    [Header("Audio")]
    [Tooltip("Drag the custom audio/voiceover clip for this specific word here (Optional).")]
    public AudioClip definitionSFX;
}

public class WordMeaning : MonoBehaviour
{
    [Header("Definitions List")]
    [Tooltip("Add as many words and definitions as this specific house needs!")]
    public WordDefinition[] definitions;

    /// <summary>
    /// Call this function from your VowelStone's OnRewardFinished() Unity Event!
    /// </summary>
    public void PlayRewardDialogue()
    {
        if (definitions == null || definitions.Length == 0)
        {
            Debug.LogWarning($"[WordMeaning] No definitions assigned on {gameObject.name}!");
            return;
        }

        StartCoroutine(PlayDefinitionsSequence());
    }

    private IEnumerator PlayDefinitionsSequence()
    {
        // 1. Block player controls safely while talking
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.SetPlayerControlState(false);
        }

        // 2. Loop through every single definition assigned to this house
        for (int i = 0; i < definitions.Length; i++)
        {
            bool isCurrentLineActive = true;
            WordDefinition currentDef = definitions[i];

            if (DialogueManager.Instance != null)
            {
                // Force portraits off, update text fields, and start typing
                DialogueManager.Instance.StartDialogueWithoutPortraits(
                    currentDef.wordName,
                    currentDef.wordMeaning,
                    () => { isCurrentLineActive = false; } // Callback loops to next entry on interaction input
                );
            }
            else
            {
                // Safety cleanup if dialogue manager goes missing mid-loop
                RestoreBGMWithFade();
                yield break;
            }

            // --- Wait 0.5 seconds before playing the definition audio track ---
            yield return new WaitForSeconds(0.5f);

            // Play the unique sound effect matched strictly with this specific word index
            if (currentDef.definitionSFX != null)
            {
                CoreAudioManager.PlaySFX(currentDef.definitionSFX);
            }

            // Wait until player presses 'E' or clicks to finish the current definition box
            while (isCurrentLineActive)
            {
                yield return null;
            }

            // Short grace delay before pulling up the next definition box (if any are left)
            yield return new WaitForSeconds(0.1f);
        }

        // 3. All definitions finished! Give control back smoothly
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.SetPlayerControlState(true);
        }

        // --- FIXED: Fades back in safely using tracked pre-cutscene layout settings ---
        RestoreBGMWithFade();
    }

    private void RestoreBGMWithFade()
    {
        // Safety check: if our tracking float recorded 0 or was broken, default to full volume (1f)
        float targetVolume = VowelStone.PreCutsceneVolume;
        if (targetVolume <= 0.01f)
        {
            targetVolume = 1f;
        }

        // Call the fade function over 1.0 second
        CoreAudioManager.FadeInBGM(targetVolume, 1.0f);
    }
}