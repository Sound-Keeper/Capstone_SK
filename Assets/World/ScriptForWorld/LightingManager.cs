//using Unity.Multiplayer.Center.Common;
//using UnityEngine;

//[ExecuteAlways]
//public class LightingManager : MonoBehaviour
//{
//    [SerializeField] private Light DirectionalLight;
//    [SerializeField] private LightingPreset Light;
//    [SerializeField, Range(0,24)] private float TimeOfDay;

//    private void UpdateLighting(float timePercent)
//    {
//        RenderSettings.ambientLight = Preset.AmbientColor.Evaluate(timePercent);
//        RenderSettings.fog
//    }
//    private void OnValidate()
//    {
//        if(DirectionalLight != null)
//            return;
//        if (RenderSettings.sun != null)
//        {
//            DirectionalLight = RenderSettings.sun;
//        }
//        else 
//        { 
//            Light[] lights = GameObject.FindObjectsOfType<Light>();
//            foreach (Light light in lights)
//            {
//                if (light.type == LightType.Directional)
//                {
//                    DirectionalLight = light;
//                    return;
//                }    
//            }
//        }
//    }
//}
