using UnityEngine;

public class CursorParticleFollower : MonoBehaviour
{
    [Header("2D Depth Settings")]
    [Tooltip("Keep this at 0 if your 2D sprites are at Z = 0. Adjust if particles appear behind backgrounds.")]
    public float zPosition = 0f;

    private Camera mainCam;
    private ParticleSystem particleSys;

    void Start()
    {
        mainCam = Camera.main;
        particleSys = GetComponent<ParticleSystem>();

        if (particleSys != null && !particleSys.isPlaying)
        {
            particleSys.Play();
        }
    }

    void Update()
    {
        if (mainCam == null) return;

        // Get mouse position in world space
        Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(Input.mousePosition);

        // Lock the Z axis so it stays perfectly aligned with your 2D layer
        mouseWorldPos.z = zPosition;

        // Update position
        transform.position = mouseWorldPos;
    }
}