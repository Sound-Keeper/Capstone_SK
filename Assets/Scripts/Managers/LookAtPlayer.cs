using UnityEngine;

public class LookAtPlayer : MonoBehaviour
{
    private Transform mainCameraTransform;

    void Start()
    {
        FindMainCamera();
    }

    void LateUpdate()
    {
        // If we don't have the camera yet, try to find it now
        if (mainCameraTransform == null)
        {
            FindMainCamera();
        }

        // Only run the look rotation if the camera has finally been found
        if (mainCameraTransform != null)
        {
            // 1. Get the direction from the UI to the camera
            Vector3 directionToCamera = mainCameraTransform.position - transform.position;

            // 2. FORCE the vertical direction (Y) to be 0 so it doesn't tilt up or down
            directionToCamera.y = 0;

            // 3. Create a rotation facing that flat direction
            // We look away from the camera's forward vector so the text isn't mirrored
            if (directionToCamera != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(-directionToCamera);
            }
        }
    }

    void FindMainCamera()
    {
        if (Camera.main != null)
        {
            mainCameraTransform = Camera.main.transform;
        }
    }
}