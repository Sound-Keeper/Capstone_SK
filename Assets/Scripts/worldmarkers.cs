using UnityEngine;

public class worldmarkers : MonoBehaviour
{
    // GD1 Drawing :> — Terrain 150 x 150 x 30, centre (75, 0, 75)

    static readonly Vector3 Plaza = new Vector3(75, 8, 75);
    static readonly Vector3 Archmage = new Vector3(75, 8, 45);
    static readonly Vector3 Police = new Vector3(101, 8, 50);
    static readonly Vector3 Judge = new Vector3(105, 8, 75);
    static readonly Vector3 Playground = new Vector3(75, 8, 105);
    static readonly Vector3 Grandma = new Vector3(45, 8, 100);
    static readonly Vector3 Library = new Vector3(45, 8, 75);

    void OnDrawGizmos()
    {
        DrawMarker("Plaza / Fountain", Plaza, Color.white);
        DrawMarker("Archmage Hall", Archmage, Color.yellow);
        DrawMarker("Police Station (A)", Police, Color.blue);
        DrawMarker("Judge Hall (E)", Judge, Color.green);
        DrawMarker("Playground (I)", Playground, Color.magenta);
        DrawMarker("Grandma Phonic (O)", Grandma, Color.red);
        DrawMarker("Library (U)", Library, Color.cyan);

        DrawPathway(Plaza, Archmage, Color.yellow);
        DrawPathway(Plaza, Police, Color.blue);
        DrawPathway(Plaza, Judge, Color.green);
        DrawPathway(Plaza, Playground, Color.magenta);
        DrawPathway(Plaza, Grandma, Color.red);
        DrawPathway(Plaza, Library, Color.cyan);

        DrawPathway(Archmage, Police, new Color(0.5f, 0.5f, 1f));
        DrawPathway(Police, Judge, new Color(0.2f, 0.8f, 0.4f));
        DrawPathway(Judge, Playground, new Color(0.9f, 0.4f, 0.9f));
        DrawPathway(Playground, Grandma, new Color(1f, 0.5f, 0.3f));
        DrawPathway(Grandma, Library, new Color(0.3f, 0.9f, 0.9f));
        DrawPathway(Library, Archmage, new Color(1f, 1f, 0.3f));
    }

    void DrawMarker(string label, Vector3 pos, Color col)
    {
        Gizmos.color = col;
        Gizmos.DrawWireSphere(pos, 2f);
        Gizmos.DrawLine(pos, pos + Vector3.up * 4f);
#if UNITY_EDITOR
        UnityEditor.Handles.Label(pos + Vector3.up * 4.2f, label);
#endif
    }

    void DrawPathway(Vector3 from, Vector3 to, Color col,
                     int segments = 30,
                     float waveAmp = 1.8f,
                     float waveFreq = 2.5f,
                     float arrowSize = 2.5f,
                     float yOffset = 0.05f)
    {
        Gizmos.color = col;

        Vector3 dir = to - from;
        float length = dir.magnitude;
        Vector3 forward = dir.normalized;
        Vector3 side = Vector3.Cross(forward, Vector3.up).normalized;

        Vector3 prev = from + Vector3.up * yOffset;
        for (int i = 1; i <= segments; i++)
        {
            float t = i / (float)segments;
            float wave = Mathf.Sin(t * Mathf.PI * waveFreq) * waveAmp;
            Vector3 pt = from + forward * (t * length)
                              + side * wave
                              + Vector3.up * yOffset;
            Gizmos.DrawLine(prev, pt);
            prev = pt;
        }
        
        Vector3 tip = to + Vector3.up * yOffset;
        Vector3 left = Quaternion.AngleAxis(30f, Vector3.up) * (-forward) * arrowSize;
        Vector3 right = Quaternion.AngleAxis(-30f, Vector3.up) * (-forward) * arrowSize;
        Gizmos.DrawLine(tip, tip + left);
        Gizmos.DrawLine(tip, tip + right);
        Gizmos.DrawLine(tip + left, tip + right);
    }
}