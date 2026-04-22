using UnityEngine;
using UnityEngine.Audio;

public class IdleLowpass : MonoBehaviour
{
    public AudioMixer mixer;

    public string parameterName = "Lowpass Cutoff Frequency";

    public float normalCutoff = 22000f;
    public float muffledCutoff = 800f;

    public float smoothSpeed = 3f;

    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        // start normal
        mixer.SetFloat(parameterName, normalCutoff);
    }

    void Update()
    {
        Vector3 vel = controller.velocity;
        vel.y = 0;

        bool isMoving = vel.magnitude > 0.1f;

        float target = isMoving ? normalCutoff : muffledCutoff;

        mixer.GetFloat(parameterName, out float current);

        float newValue = Mathf.Lerp(current, target, Time.deltaTime * smoothSpeed);

        mixer.SetFloat(parameterName, newValue);
    }
}