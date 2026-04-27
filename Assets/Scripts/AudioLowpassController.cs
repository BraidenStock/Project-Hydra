using UnityEngine;
using UnityEngine.Audio;

public class AudioLowpassController : MonoBehaviour
{
    public AudioMixer mixer;

    [Header("Mixer Parameter")]
    public string parameterName = "Lowpass Cutoff Frequency";

    [Header("Cutoff Values")]
    public float normalCutoff = 22000f;
    public float muffledCutoff = 800f;

    [Header("Smoothing")]
    public float smoothSpeed = 5f;

    [Header("Idle Settings")]
    public float idleTimeBeforeMuffling = 2f;

    private Rigidbody rb;
    private float idleTimer;

    // controlled externally
    public static bool inventoryOpen;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (mixer != null)
            mixer.SetFloat(parameterName, normalCutoff);
    }

    private void Update()
    {
        if (mixer == null) return;

        HandleIdleDetection();

        float target = GetTargetCutoff();

        mixer.GetFloat(parameterName, out float current);

        float newValue = Mathf.Lerp(current, target, Time.deltaTime * smoothSpeed);

        mixer.SetFloat(parameterName, newValue);
    }

    private void HandleIdleDetection()
    {
        if (rb == null) return;

        Vector3 vel = rb.linearVelocity;
        vel.y = 0;

        bool isMoving = vel.magnitude > 0.1f;

        if (isMoving)
        {
            idleTimer = 0f;
        }
        else
        {
            idleTimer += Time.deltaTime;
        }
    }

    private float GetTargetCutoff()
    {
        // 🔥 INVENTORY ALWAYS TAKES PRIORITY
        if (inventoryOpen)
            return muffledCutoff;

        // 😴 IDLE ONLY IF NOT MOVING LONG ENOUGH
        if (idleTimer >= idleTimeBeforeMuffling)
            return muffledCutoff;

        return normalCutoff;
    }
}