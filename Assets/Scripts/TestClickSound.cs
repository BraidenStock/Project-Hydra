using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TestClickSound : MonoBehaviour
{
    [Header("Camera & Settings")]
    public Camera playerCamera;        // Assign the player camera
    public float interactDistance = 5f; // Max distance to interact

    [Header("Audio")]
    public AudioSource audioSource;    // AudioSource attached to the object
    public AudioClip[] clips;          // Optional: multiple clips for variety

    private void Update()
    {
        // Check for left mouse click
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
            {
                // Check if the hit object is this one
                if (hit.collider.gameObject == gameObject)
                {
                    PlaySound();
                }
            }
        }
    }

    private void PlaySound()
    {
        if (audioSource == null) return;

        if (clips != null && clips.Length > 0)
        {
            int index = Random.Range(0, clips.Length);
            audioSource.PlayOneShot(clips[index]);
        }
        else
        {
            audioSource.Play();
        }
    }
}