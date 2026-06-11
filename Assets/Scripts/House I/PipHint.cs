using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PipHint : MonoBehaviour
{
    //pip the guide - leads the player through the houses in order (A,E,I,O,U) so they never get lost.
    //each objective fires onReached ONCE when the player arrives (hook the house's forced dialogue there).

    [System.Serializable]
    public class HintObjective
    {
        [Tooltip("Where Pip leads the player (the house NPC / entrance).")]
        public Transform location;
        [Tooltip("The vowel stone earned here - this objective is 'done' once the player has it.")]
        public VowelStone.StoneType stone = VowelStone.StoneType.None;
        [Tooltip("Particle clue at the location (optional).")]
        public ParticleSystem clue;
        [Tooltip("Fires ONCE when the player first reaches this objective (hook the forced house dialogue).")]
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
        //press H to (re)summon Pip to the current objective
        if (Keyboard.current != null && Keyboard.current.hKey.wasPressedThisFrame)
            LeadToCurrent();

        if (!autoGuide) return;

        HintObjective next = NextObjective();

        //objective changed (player finished one, or first run) -> Pip leads to the new one
        if (next != current)
        {
            StopClue(current);
            current = next;

            if (current != null)
                LeadToCurrent();
            else if (pip != null && player != null)
                pip.FollowPlayerStart(player);   //all houses done -> Pip just follows
        }

        if (current == null) return;

        //player reached the current house -> fire its forced dialogue once
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

    //first house (in list order) whose stone the player still doesn't have
    HintObjective NextObjective()
    {
        foreach (HintObjective o in objectives)
            if (!HasStone(o.stone)) return o;
        return null;
    }

    void StopClue(HintObjective o)
    {
        if (o != null && o.clue != null) o.clue.Stop();
    }

    bool HasStone(VowelStone.StoneType s)
    {
        switch (s)
        {
            case VowelStone.StoneType.VowelI: return PuzzleProgress.HasVowelIStone;
            case VowelStone.StoneType.VowelA: return PuzzleProgress.HasVowelAStone;
            case VowelStone.StoneType.VowelE: return PuzzleProgress.HasVowelEStone;
            case VowelStone.StoneType.VowelO: return PuzzleProgress.HasVowelOStone;
            case VowelStone.StoneType.VowelU: return PuzzleProgress.HasVowelUStone;
            default: return false;
        }
    }
}
