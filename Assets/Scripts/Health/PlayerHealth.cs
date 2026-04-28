using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class PlayerHealth : MonoBehaviour
{
    public Health health;

    [Header("Respawn")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float respawnDelay = 2f;

    [Header("Audio")]
    [SerializeField] private AudioClip hurtClip;
    private AudioSource audioSource;

    private float lastHealth;
    private bool isRespawning;

    private void Awake()
    {
        if (health == null)
            health = GetComponent<Health>();

        audioSource = GetComponent<AudioSource>();
        lastHealth = health.currentHealth;
    }

    private void Start()
    {
        if (spawnPoint != null)
        {
            transform.position = spawnPoint.position;
            transform.rotation = spawnPoint.rotation;
        }
        else
        {
            Debug.LogWarning("⚠ No spawn point assigned!");
        }
    }

    private void Update()
    {
        // Play hurt sound when damaged
        if (!health.isDead && health.currentHealth < lastHealth)
        {
            PlayHurtSound();
        }

        lastHealth = health.currentHealth;

        // Handle death → respawn
        if (health.isDead && !isRespawning)
        {
            StartCoroutine(RespawnRoutine());
        }
    }

    private void PlayHurtSound()
    {
        if (hurtClip != null)
        {
            audioSource.pitch = Random.Range(0.95f, 1.05f);
            audioSource.PlayOneShot(hurtClip);
        }
    }

    private IEnumerator RespawnRoutine()
    {
        isRespawning = true;

        Debug.Log("Player died. Respawning...");

        // Optional: disable movement here
        // GetComponent<PlayerMovement>().enabled = false;

        yield return new WaitForSeconds(respawnDelay);

        // Reset health
        health.currentHealth = health.maxHealth;
        health.isDead = false;

        // Move player to spawn
        if (spawnPoint != null)
        {
            transform.position = spawnPoint.position;
            transform.rotation = spawnPoint.rotation;
        }
        else
        {
            Debug.LogWarning("⚠ No spawn point assigned!");
        }

        // Optional: re-enable movement
        // GetComponent<PlayerMovement>().enabled = true;

        isRespawning = false;

        Debug.Log("Player respawned");
    }
}