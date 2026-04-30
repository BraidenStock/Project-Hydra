using UnityEngine;
using UnityEngine.VFX;
using System.Collections;
using System.Collections.Generic;

public class EnemyHealth : MonoBehaviour
{
    public Health health;
    private HostileAI ai;

    [Header("Death")]
    [SerializeField] private GameObject deathVFXPrefab;
    [SerializeField] private float destroyDelay = 0.2f;

    [Header("Damage Flash")]
    [SerializeField] private Color flashColor = Color.red;
    [SerializeField] private float flashDuration = 0.1f;

    private List<Renderer> renderers = new List<Renderer>();
    private List<Color> originalColors = new List<Color>();

    private float lastHealth;
    private bool hasDied = false;

    private void Awake()
    {
        ai = GetComponent<HostileAI>();

        // Cache renderers
        renderers.AddRange(GetComponentsInChildren<Renderer>());

        foreach (Renderer r in renderers)
        {
            if (r.material.HasProperty("_Color"))
                originalColors.Add(r.material.color);
            else
                originalColors.Add(Color.white);
        }
    }

    private void Start()
    {
        if (health != null)
            lastHealth = health.currentHealth;
    }

    private void Update()
    {
        if (health == null) return;

        // ✅ Damage flash
        if (!hasDied && health.currentHealth < lastHealth)
        {
            StartCoroutine(Flash());
        }

        lastHealth = health.currentHealth;

        // ✅ Death handling
        if (!hasDied && health.isDead)
        {
            Die();
        }
    }

    private void Die()
    {
        hasDied = true;

        if (ai != null)
            ai.enabled = false;

        PlayDeathVFX();

        Destroy(gameObject, destroyDelay);
    }

    private void PlayDeathVFX()
    {
        if (deathVFXPrefab == null)
            return;

        GameObject vfxObj = Instantiate(deathVFXPrefab, transform.position, Quaternion.identity);

        VisualEffect vfx = vfxObj.GetComponent<VisualEffect>();
        if (vfx != null)
        {
            vfx.Play();
        }
    }

    private IEnumerator Flash()
    {
        // Apply flash color
        for (int i = 0; i < renderers.Count; i++)
        {
            if (renderers[i].material.HasProperty("_Color"))
            {
                renderers[i].material.color = flashColor;
            }
        }

        yield return new WaitForSeconds(flashDuration);

        // Restore original colors
        for (int i = 0; i < renderers.Count; i++)
        {
            if (renderers[i].material.HasProperty("_Color"))
            {
                renderers[i].material.color = originalColors[i];
            }
        }
    }
}