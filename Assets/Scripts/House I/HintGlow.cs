using UnityEngine;

public class HintGlow : MonoBehaviour
{
    //makes the right answer glow once the player keeps getting the puzzle wrong

    [Tooltip("Optional: a highlight/outline/particle object switched ON while glowing. If set, the emission pulse below is ignored.")]
    public GameObject highlight;
    [Tooltip("Emission glow colour (used when no highlight object is set).")]
    public Color glowColor = Color.yellow;
    public float pulseSpeed = 3f;

    Material mat;
    bool glowing = false;

    void Awake()
    {
        if (highlight != null) highlight.SetActive(false);

        Renderer rend = GetComponentInChildren<Renderer>();
        if (rend != null) mat = rend.material; //own instance so we don't tint the shared material
    }

    public void StartGlow()
    {
        glowing = true;
        if (highlight != null) { highlight.SetActive(true); return; }
        if (mat != null) mat.EnableKeyword("_EMISSION");
    }

    public void StopGlow()
    {
        glowing = false;
        if (highlight != null) { highlight.SetActive(false); return; }
        if (mat != null) mat.SetColor("_EmissionColor", Color.black);
    }

    void Update()
    {
        if (!glowing || highlight != null || mat == null) return;

        float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
        mat.SetColor("_EmissionColor", glowColor * t);
    }
}
