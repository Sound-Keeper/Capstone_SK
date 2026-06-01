using UnityEngine;
using UnityEngine.InputSystem;

public class WandMechanics : MonoBehaviour
{
    [SerializeField] private ParticleSystem magicEffect;
    public PlayerHold carry;
    public float range = 50f;
    public Transform wandTip; // drag your pointer/wand tip here

    // light
    public Light wandLight;
    public float idleIntensity = 0.5f;
    public float attackIntensity = 3f;
    public float lightSpeed = 8f;

    void Start()
    {
        if (wandLight != null)
            wandLight.intensity = idleIntensity;
    }

    void Update()
    {
        bool isHolding = carry != null && carry.IsHolding();
        bool isAttacking = InputSystem.actions.FindAction("Attack").IsPressed();

        //part na to susundan ng particles yung crosshair/camera-test1
        // Camera.main can be null briefly during a scene swap (the old scene's
        // MainCamera is destroyed) - skip aiming that frame instead of throwing.
        Camera cam = Camera.main;
        if ((isAttacking || isHolding) && cam != null && magicEffect != null && wandTip != null)
        {
            Ray ray = new Ray(cam.transform.position, cam.transform.forward);
            Vector3 targetPoint;

            if (Physics.Raycast(ray, out RaycastHit hit, range))
                targetPoint = hit.point;
            else
                targetPoint = cam.transform.position + cam.transform.forward * range;
            magicEffect.transform.position = wandTip.position;

            Vector3 direction = (targetPoint - wandTip.position).normalized;
            if (direction != Vector3.zero)
            {
                magicEffect.transform.rotation = Quaternion.LookRotation(direction);
                var main = magicEffect.main;
                main.startRotation3D = false;
            }

            if (!magicEffect.isPlaying)
                magicEffect.Play();
        }
        else
        {
            if (magicEffect != null && magicEffect.isPlaying)
                magicEffect.Stop();
        }

        // wand light glow
        if (wandLight != null)
        {
            float targetIntensity = isAttacking ? attackIntensity : idleIntensity;
            wandLight.intensity = Mathf.Lerp(
                wandLight.intensity,
                targetIntensity,
                lightSpeed * Time.deltaTime
            );
        }
    }
}