using UnityEngine;
using UnityEngine.Audio;

public class IdleLowpass : MonoBehaviour
{
    public AudioMixer mixer;

    public string parameterName = "Lowpass Cutoff Frequency";

    public float normalCutoff = 22000f;
    public float muffledCutoff = 800f;

    public float smoothSpeed = 3f;
    public float idleTimeBeforeMuffling = 2f;

    private Rigidbody rb;
    private float idleTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // start normal
        mixer.SetFloat(parameterName, normalCutoff);
    }

    void Update()
    {
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

        float target = (idleTimer >= idleTimeBeforeMuffling) ? muffledCutoff : normalCutoff;

        mixer.GetFloat(parameterName, out float current);

        float newValue = Mathf.Lerp(current, target, Time.deltaTime * smoothSpeed);

        mixer.SetFloat(parameterName, newValue);
    }
}