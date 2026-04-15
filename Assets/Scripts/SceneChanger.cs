using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("The name of the scene to load when the button is clicked.")]
    public string sceneName;

    [Header("Audio Settings")]
    [Tooltip("The AudioMixer snapshot to transition to when the button is clicked.")]
    public AudioMixerSnapshot audioSnapshot;

    [Tooltip("Time in seconds to transition to the audio snapshot.")]
    public float snapshotTransitionTime = 1f;

    public void ChangeSceneAndSnapshot()
    {
        if (audioSnapshot != null)
        {
            audioSnapshot.TransitionTo(snapshotTransitionTime);
        }
        else
        {
            Debug.LogWarning("SceneChanger: No audio snapshot assigned.");
        }

        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning("SceneChanger: No scene name assigned.");
        }
    }
}
