using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class Uhouse3DManager : MonoBehaviour
{
    [Header("3D Scene References")]
    [Tooltip("The main 3D Book object that pops out.")]
    public Transform bookTransform;
    [Tooltip("An empty GameObject positioned exactly out in front of the bookshelf where the book should float during gameplay.")]
    public Transform bookActiveAnchor;
    [Tooltip("The spot right on the book asset's sign where the missing letter should snap.")]
    public Transform signUnderlineTarget;

    [Header("Vowel Choice Cubes")]
    [Tooltip("Assign your physical 3D vowel blocks here.")]
    public GameObject[] choiceCubes = new GameObject[3];
    [Tooltip("Three empty GameObjects placed in the air in front of the shelf where the blocks float up to.")]
    public Transform[] choiceAirAnchors = new Transform[3];

    [Header("Correct Answer Key")]
    [Tooltip("The exact letter needed to solve this specific book puzzle (e.g., 'I').")]
    public string correctVowel = "I";

    [Header("Settings & Feedback")]
    public float moveSpeed = 8f;
    public float shakeMagnitude = 0.15f;
    public float shakeDuration = 0.4f;

    [Header("Events")]
    public UnityEvent OnChallengeComplete;

    private Dictionary<GameObject, Vector3> cubeHomePositions = new Dictionary<GameObject, Vector3>();
    private bool isAnimating = false;
    private bool isBookActive = false;
    private bool isCleared = false;

    void Start()
    {
        RememberCubeHomes();
    }

    void RememberCubeHomes()
    {
        foreach (GameObject cube in choiceCubes)
        {
            if (cube != null) cubeHomePositions[cube] = cube.transform.position;
        }
    }

    // Triggered when pressing E on your crosshair raycast
    public void InteractWithBook()
    {
        if (isAnimating || isBookActive || isCleared) return;
        StartCoroutine(ActivateBookRoutine());
    }

    private IEnumerator ActivateBookRoutine()
    {
        isAnimating = true;

        // 1. Move the book out directly in front of the bookshelf
        while (Vector3.Distance(bookTransform.position, bookActiveAnchor.position) > 0.02f)
        {
            bookTransform.position = Vector3.MoveTowards(bookTransform.position, bookActiveAnchor.position, moveSpeed * Time.deltaTime);
            yield return null;
        }
        bookTransform.position = bookActiveAnchor.position;

        // 2. Float the 3 choice cubes up into their air anchors
        float progress = 0;
        while (progress < 1f)
        {
            progress += Time.deltaTime * moveSpeed;
            for (int i = 0; i < choiceCubes.Length; i++)
            {
                if (choiceCubes[i] != null)
                {
                    choiceCubes[i].transform.position = Vector3.Lerp(cubeHomePositions[choiceCubes[i]], choiceAirAnchors[i].position, progress);
                }
            }
            yield return null;
        }

        isBookActive = true;
        isAnimating = false;
    }

    public void CheckVowelSelection(string chosenVowel, GameObject cubeObject)
    {
        if (isAnimating || !isBookActive || isCleared) return;

        if (chosenVowel.ToUpper() == correctVowel.ToUpper())
        {
            StartCoroutine(CorrectAnswerRoutine(cubeObject));
        }
        else
        {
            StartCoroutine(WrongAnswerRoutine(cubeObject));
        }
    }

    private IEnumerator CorrectAnswerRoutine(GameObject cube)
    {
        isAnimating = true;
        isCleared = true; // Prevents double clicking anything else

        // Disable collider so it can't be raycasted again
        if (cube.TryGetComponent<Collider>(out Collider col)) col.enabled = false;

        // 1. Smoothly fly the cube over and snap it onto the sign's underline target gap
        while (Vector3.Distance(cube.transform.position, signUnderlineTarget.position) > 0.02f)
        {
            cube.transform.position = Vector3.MoveTowards(cube.transform.position, signUnderlineTarget.position, moveSpeed * Time.deltaTime);
            yield return null;
        }
        cube.transform.position = signUnderlineTarget.position;

        // Make the cube a permanent child of the book so if the book moves later, the letter moves with it
        cube.transform.SetParent(bookTransform);

        yield return new WaitForSeconds(1.2f);

        // 2. Hide the other wrong choice blocks away instantly
        for (int i = 0; i < choiceCubes.Length; i++)
        {
            if (choiceCubes[i] != cube && choiceCubes[i] != null)
            {
                choiceCubes[i].SetActive(false);
            }
        }

        // Fire your success unity event!
        OnChallengeComplete?.Invoke();
        isAnimating = false;
    }

    private IEnumerator WrongAnswerRoutine(GameObject cube)
    {
        isAnimating = true;

        // 1. Snap wrong choice back instantly to its air position
        int index = System.Array.IndexOf(choiceCubes, cube);
        if (index != -1) cube.transform.position = choiceAirAnchors[index].position;

        // 2. Shake the book right in front of the shelf to show rejection
        Vector3 originalBookPos = bookTransform.position;
        float elapsed = 0.0f;
        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;
            bookTransform.position = originalBookPos + new Vector3(x, y, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }
        bookTransform.position = originalBookPos;

        isAnimating = false;
    }
}