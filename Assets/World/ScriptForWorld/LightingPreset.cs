using JetBrains.Annotations;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "Lighting Preset", menuName = "Scriptables/Lighting Preset", order =1) ]
public class LightingPreset : ScriptableObject
{
    //Day and Night in Unity
    //test
    public Gradient AmbientColor;
    public Gradient DirectionalColor;
    public Gradient FogColor;
}
