using UnityEngine;

public class TrackSliderHandle : MonoBehaviour
{
    [Tooltip("Drag your Text (TMP) object here.")]
    public RectTransform textTransform;

    [Tooltip("Drag the Slider's internal Handle object here.")]
    public RectTransform handleTransform;

    [Tooltip("Offset height if you want the text to float slightly above the knob.")]
    public float yOffset = 50f;

    void LateUpdate()
    {
        if (textTransform != null && handleTransform != null)
        {
            // Lock the text position directly to the handle's sliding X coordinates
            Vector3 targetPosition = handleTransform.position;
            targetPosition.y += yOffset;

            textTransform.position = targetPosition;
        }
    }
}