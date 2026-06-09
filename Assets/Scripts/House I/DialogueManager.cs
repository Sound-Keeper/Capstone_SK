using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI")]
    public GameObject dialoguePanel;
    public TMP_Text nameText;
    public TMP_Text bodyText;
    public TMP_Text hintText;

    [Header("Cameras")]
    public Camera gameplayCamera;
    public Camera dialogueCamera;
    public Camera pennyDialogueCamera;

    [Header("Pip Default Dialogue")]
    public string speakerName = "Pip";
    [TextArea(2, 5)]
    public string[] lines;

    [Header("Pip Movement")]
    public PipFly pipFly;
    public Transform pennyCilTarget;

    [Header("Intro Sequence")]
    [Tooltip("ON = the intro plays every time you press Play (for testing). OFF = only the first time.")]
    public bool forceIntroEveryPlay = false;
    [Tooltip("The player. Leave empty to auto-find the 'Player' tag.")]
    public Transform introPlayer;
    [Tooltip("How close the player must get to Pip's spot before the next lines play.")]
    public float arriveDistance = 3f;

    [Tooltip("Pip flies ahead here (the water fountain) so the player can follow the trail.")]
    public Transform fountainTarget;
    [TextArea(2, 5)]
    [Tooltip("Pip explains everything (the vowel stones / the world) at the fountain.")]
    public string[] vowelStoneLines;

    [Tooltip("Pip then leads the player here (House A). Optional.")]
    public Transform houseATarget;
    [TextArea(2, 5)]
    [Tooltip("Pip's lines once they reach House A. Optional.")]
    public string[] houseALines;

    [HideInInspector]
    public System.Action OnDialogueEnd;

    int currentLine = 0;
    bool isTalking = false;

    void Awake()
    {
        Instance = this;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (dialogueCamera != null)
            dialogueCamera.gameObject.SetActive(false);

        if (pennyDialogueCamera != null)
            pennyDialogueCamera.gameObject.SetActive(false);
    }

    void Start()
    {
        //TEMP diagnostics - remove once the intro is confirmed working
        Debug.Log($"[DialogueManager] Start. forceIntroEveryPlay={forceIntroEveryPlay}, IntroDone={PlayerPrefs.GetInt("WordValleyIntroDone", 0)}, welcomeLines={(lines == null ? 0 : lines.Length)}, dialoguePanel={(dialoguePanel != null)}, bodyText={(bodyText != null)}");

        // Force the intro on the player: welcome -> walk to fountain -> explain -> walk to House A
        if (forceIntroEveryPlay || PlayerPrefs.GetInt("WordValleyIntroDone", 0) == 0)
        {
            Debug.Log("[DialogueManager] Playing intro.");
            StartCoroutine(IntroSequence());
        }
        else
        {
            Debug.Log("[DialogueManager] Intro SKIPPED (already done). Tick 'Force Intro Every Play' to replay.");
        }
    }

    void Update()
    {
        if (!isTalking) return;

        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            NextLine();
        }
    }

    IEnumerator IntroSequence()
    {
        // 1. Pip forces the player to talk (welcome)
        PlayPip(lines);
        yield return new WaitUntil(() => !isTalking);

        // 2. Pip flies ahead to the fountain; player follows the trail on their own
        yield return LeadTo(fountainTarget);

        // 3. introduce everything once the player catches up
        PlayPip(vowelStoneLines);
        yield return new WaitUntil(() => !isTalking);

        // 4. Pip flies ahead to House A; player follows
        yield return LeadTo(houseATarget);
        PlayPip(houseALines);
        yield return new WaitUntil(() => !isTalking);

        FinishIntro();
    }

    //pip flies ahead to dest, then we wait for the PLAYER to walk over to it
    IEnumerator LeadTo(Transform dest)
    {
        if (dest == null) yield break;

        if (pipFly != null) pipFly.MoveToTarget(dest);

        Transform pl = GetPlayer();
        if (pl == null) yield break;

        while (Vector3.Distance(pl.position, dest.position) > arriveDistance)
            yield return null;
    }

    void PlayPip(string[] l)
    {
        if (l != null && l.Length > 0)
            StartDialogue("Pip", l);
    }

    public void StartDialogue()
    {
        StartDialogue(speakerName, lines);
    }

    public void StartDialogue(string speaker, string[] newLines)
    {
        speakerName = speaker;
        lines = newLines;

        if (lines == null || lines.Length == 0) return;

        currentLine = 0;
        isTalking = true;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        //which dialogue camera (if any) this speaker uses
        Camera cam = (speaker == "Penny Cil") ? pennyDialogueCamera : dialogueCamera;

        //only switch off the gameplay camera if there's a dialogue cam to show -
        //otherwise just overlay the dialogue box on the live view (no black screen)
        if (cam != null && gameplayCamera != null)
            gameplayCamera.gameObject.SetActive(false);

        if (dialogueCamera != null)
            dialogueCamera.gameObject.SetActive(cam == dialogueCamera);

        if (pennyDialogueCamera != null)
            pennyDialogueCamera.gameObject.SetActive(cam == pennyDialogueCamera);

        if (nameText != null)
            nameText.text = speakerName;

        if (bodyText != null)
            bodyText.text = Format(lines[currentLine]);

        if (hintText != null)
            hintText.text = "Press Space to continue";

        Time.timeScale = 0f;
    }

    void NextLine()
    {
        currentLine++;

        if (currentLine >= lines.Length)
        {
            EndDialogue();
            return;
        }

        if (bodyText != null)
            bodyText.text = Format(lines[currentLine]);
    }

    void EndDialogue()
    {
        isTalking = false;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (dialogueCamera != null)
            dialogueCamera.gameObject.SetActive(false);

        if (pennyDialogueCamera != null)
            pennyDialogueCamera.gameObject.SetActive(false);

        if (gameplayCamera != null)
            gameplayCamera.gameObject.SetActive(true);

        Time.timeScale = 1f;

        // Fire the callback (NPC trigger uses this to activate its puzzle)
        if (OnDialogueEnd != null)
        {
            OnDialogueEnd.Invoke();
            OnDialogueEnd = null; // Clear so it doesn't fire again
        }
    }

    void FinishIntro()
    {
        PlayerPrefs.SetInt("WordValleyIntroDone", 1);
        PlayerPrefs.Save();

        // pip trail - start flying beside the player
        if (pipFly != null)
        {
            Transform who = GetPlayer();
            if (who != null) pipFly.FollowPlayerStart(who);
        }
    }

    Transform GetPlayer()
    {
        if (introPlayer != null) return introPlayer;
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        return (p != null) ? p.transform : null;
    }

    // for character selection thing - {player} becomes the chosen character's name
    string Format(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        return s.Replace("{player}", CharacterSelection.SelectedName);
    }
}
