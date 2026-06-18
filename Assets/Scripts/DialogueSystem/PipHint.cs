using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PipHint : MonoBehaviour
{
    [System.Serializable]
    public class HintObjective
    {
        [Tooltip("Where Pip leads the player (the house NPC / entrance).")]
        public Transform location;

        [Tooltip("Which house completion flag is this objective waiting for?")]
        public string houseLetter = "A"; // Set to A, E, I, O, or U

        [Tooltip("Particle clue at the location (optional).")]
        public ParticleSystem clue;

        [Tooltip("Fires ONCE when the player first reaches this objective.")]
        public UnityEvent onReached;
        [HideInInspector] public bool reached;
    }

    [Header("References")]
    public PipFly pip;
    public Transform player;

    [Header("Houses in order (A, E, I, O, U)")]
    public List<HintObjective> objectives = new List<HintObjective>();

    [Header("Behaviour")]
    [Tooltip("ON = Pip automatically leads to the next house. OFF = only leads when the player presses H.")]
    public bool autoGuide = true;
    [Tooltip("How close the player must get for Pip to count them as 'arrived'.")]
    public float arriveDistance = 3f;

    HintObjective current;

    void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    void Update()
    {
        // Press H to (re)summon Pip to the current objective[cite: 7]
        if (Keyboard.current != null && Keyboard.current.hKey.wasPressedThisFrame)
            LeadToCurrent();

        if (!autoGuide) return;

        HintObjective next = NextObjective();

        // Objective changed (player finished one) -> Pip leads to the new one[cite: 7]
        if (next != current)
        {
            StopClue(current);
            current = next;

            if (current != null)
                LeadToCurrent();
            else if (pip != null && player != null)
                pip.FollowPlayerStart(player); // All houses done -> Pip just follows[cite: 7]
        }

        if (current == null) return;

        // Player reached the current house -> fire its forced behavior once[cite: 7]
        if (!current.reached && current.location != null && player != null &&
            Vector3.Distance(player.position, current.location.position) <= arriveDistance)
        {
            current.reached = true;
            StopClue(current);
            current.onReached?.Invoke();
        }
    }

    void LeadToCurrent()
    {
        if (current == null) current = NextObjective();
        if (current == null || current.location == null || pip == null) return;

        if (current.clue != null) current.clue.Play();
        pip.MoveToTarget(current.location);
    }

    // Finds the first house objective in the list that isn't fully completed yet
    HintObjective NextObjective()
    {
        foreach (HintObjective o in objectives)
        {
            // If the house is NOT complete yet, this is our next target!
            if (!PuzzleProgress.IsHouseComplete(o.houseLetter))
                return o;
        }
        return null;
    }

    void StopClue(HintObjective o)
    {
        if (o != null && o.clue != null) o.clue.Stop();
    }
}