using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class NpcInteraction : MonoBehaviour
{
    //test for interaction with PennyCil
    [Header("Interaction Range")]
    public float interactionRange = 3f;
    public float fadeSpeed = 5f;

    [Header("References")]
    public CanvasGroup promptCanvas;
    public Transform player;

    [Header("Dialogue")]
    public string npcName = "Penny Cil";
    [TextArea(2, 5)]
    public string[] dialogueLines = new string[]
    {
        "Hi! I forgot how to spell....",
        "Can you help me put the letters back?",
        "Are you ready?"
    };
    public Camera dialogueCamera;

    [Header("Scene Transition")]
    public string sceneToLoad;
    public Vector3 spawnPoint;

    bool playerInRange = false;
    bool hasTriggered = false;
    public Camera npcDialogueCamera;

    void Start()
    {
        if (promptCanvas != null)
            promptCanvas.alpha = 0f;

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (dialogueCamera != null)
            dialogueCamera.enabled = false;
    }

    void Update()
    {
        if (player == null || promptCanvas == null) return;

        // Chinecheck neto distance of the player and npc
        float distance = Vector3.Distance(transform.position, player.position);
        playerInRange = distance <= interactionRange;

        //test if it slowly fade in and fade out - also hide kapag mid-dialogue na
        float targetAlpha = (playerInRange && !hasTriggered) ? 1f : 0f;
        promptCanvas.alpha = Mathf.Lerp(promptCanvas.alpha, targetAlpha, fadeSpeed * Time.deltaTime);

        // Test ng E canvas if pop out
        if (playerInRange && !hasTriggered &&
            Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            Interact();
        }
    }

    void Interact()
    {
        Debug.Log("Talking to " + gameObject.name); // test if the player press e it talks to the npc

        hasTriggered = true;

        Charactercontroller controller = player.GetComponent<Charactercontroller>();
        if (controller != null)
            controller.enabled = false;

        if (DialogueHouseI.Instance == null)
        {
            Debug.LogError("DialogueHouseI.Instance is gone. Script not in scene or not attached to a GameObject!" +
                "may mali ka check mo ulit");

            if (controller != null)
                controller.enabled = true;

            hasTriggered = false;
            return;
        }

        Debug.Log("DialogueHouseI found, calling StartDialogue...");
        DialogueHouseI.Instance.StartDialogue(
            npcName,
            dialogueLines,
            dialogueCamera,
            OnDialogueComplete
        );
    }

    private void OnDialogueComplete()
    {
        Charactercontroller cc = player.GetComponent<Charactercontroller>();

        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            if (player != null)
            {
                if (cc != null)
                {
                    cc.enabled = false;
                    player.position = spawnPoint;
                    cc.enabled = true;
                }
                else
                {
                    player.position = spawnPoint;
                }
            }

            SceneController.Instance
                .NewTransition()
                .Load(SceneDatabase.Slots.SessionContent, sceneToLoad, setActive: true)
                .WithOverlay()
                .WithClearUnusedAssets()
                .Perform();
        }
        else
        {
            // If no scene transition yet, player can walk again after dialogue
            if (cc != null)
                cc.enabled = true;

            hasTriggered = false;
        }
    }
}