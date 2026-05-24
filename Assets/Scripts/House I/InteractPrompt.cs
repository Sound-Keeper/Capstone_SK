using System;
using UnityEngine;

public class InteractPrompt : MonoBehaviour
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
    void Start()
    {
        if (promptCanvas != null) 
         promptCanvas.alpha = 0f; 

        if (player != null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }
    void Update()
    {
        if (player == null || promptCanvas == null) return;

        // Chinecheck neto distance of the player and npc
        float distance = Vector3.Distance(transform.position, player.position);
        playerInRange = distance <= interactionRange;

        //test if it slowly fade in and fade out Reference sa unitywebsite
        float targetAlpha = playerInRange ? 1f : 0f;
        promptCanvas.alpha = Mathf.Lerp(promptCanvas.alpha, targetAlpha, fadeSpeed * Time.deltaTime);
        
        // Test ng E canvas if pop out
        if (playerInRange && UnityEngine.InputSystem.Keyboard.current.eKey.wasPressedThisFrame)
        { Interact(); }

    }

     void Interact()
    {
        Debug.Log("Talking to " + gameObject.name); // test if the player press e it talks to the npc

        hasTriggered = true;
        if (DialogueHouseI.Instance != null) 
        {
            DialogueHouseI.Instance.StartDialogue(
                npcName,
                dialogueLines,
                dialogueCamera,
                OnDialogueComplete
                );
        }
        
    }

    private void OnDialogueComplete()
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            if (player != null)
            {
                Charactercontroller cc = player.GetComponent<Charactercontroller>();
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
        }
        SceneController.Instance
            .NewTransition()
            .Load(SceneDatabase.Slots.SessionContent, sceneToLoad, setActive: true);
            .WithOverlay()
            .WithClearUnusedAsset()
            .Perform();

    }
}
