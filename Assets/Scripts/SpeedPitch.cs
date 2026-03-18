using UnityEngine;

public class SpeedAtmosphere : MonoBehaviour
{
    public AudioSource atmosphere;
    public float maxSpeed = 10f;
    public float minPitch = 0.95f;
    public float maxPitch = 1.05f;

    private Vector3 lastPosition;

    void Start()
    {
        lastPosition = transform.position;
    }

    void Update()
    {
        float speed = (transform.position - lastPosition).magnitude / Time.deltaTime;
        lastPosition = transform.position;

        float t = Mathf.Clamp01(speed / maxSpeed);
        atmosphere.pitch = Mathf.Lerp(minPitch, maxPitch, t);
    }
}