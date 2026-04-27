using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class EnemyNoises : MonoBehaviour
{
    [Header("Audio Source")]
    private AudioSource audioSource;

    [Header("Idle Sounds")]
    [SerializeField] private AudioClip[] idleClips;
    [SerializeField] private float minIdleDelay = 3f;
    [SerializeField] private float maxIdleDelay = 8f;

    [Header("Attack Sound")]
    [SerializeField] private AudioClip attackClip;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        StartCoroutine(IdleNoiseLoop());
    }

    private IEnumerator IdleNoiseLoop()
    {
        while (true)
        {
            float delay = Random.Range(minIdleDelay, maxIdleDelay);
            yield return new WaitForSeconds(delay);

            PlayIdleSound();
        }
    }

    private void PlayIdleSound()
    {
        if (idleClips.Length == 0) return;

        AudioClip clip = idleClips[Random.Range(0, idleClips.Length)];
        audioSource.PlayOneShot(clip);
    }

    public void PlayAttackSound()
    {
        if (attackClip == null) return;

        audioSource.PlayOneShot(attackClip);
    }
}