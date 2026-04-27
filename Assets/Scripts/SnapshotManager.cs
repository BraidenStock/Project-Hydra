using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class SnapshotManager : MonoBehaviour
{
    [Header("Audio Mixer")]
    public AudioMixer mixer;

    [Header("Snapshots")]
    public AudioMixerSnapshot defaultSnapshot;
    public AudioMixerSnapshot zoneSnapshot;
    public AudioMixerSnapshot[] sceneSnapshots;

    [Header("Transition Time")]
    public float transitionTime = 1.5f;

    private bool inZone = false;

    private void Awake()
    {
        ApplySceneSnapshot();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplySceneSnapshot();
    }

    // -----------------------------
    // CORE SNAPSHOT LOGIC
    // -----------------------------
    private void ApplySceneSnapshot()
    {
        if (inZone && zoneSnapshot != null)
        {
            zoneSnapshot.TransitionTo(transitionTime);
            return;
        }

        string sceneName = SceneManager.GetActiveScene().name;

        // try match scene snapshot by name
        foreach (var snap in sceneSnapshots)
        {
            if (snap != null && snap.name == sceneName)
            {
                snap.TransitionTo(transitionTime);
                return;
            }
        }

        // fallback
        if (defaultSnapshot != null)
        {
            defaultSnapshot.TransitionTo(transitionTime);
        }
    }

    // -----------------------------
    // ZONE CONTROL (same script)
    // -----------------------------
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        inZone = true;

        if (zoneSnapshot != null)
        {
            zoneSnapshot.TransitionTo(transitionTime);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        inZone = false;

        ApplySceneSnapshot();
    }
}