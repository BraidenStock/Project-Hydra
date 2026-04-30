using UnityEngine;
using System.Collections.Generic;

public class Healing : MonoBehaviour
{
    [Header("Healing Settings")]
    [SerializeField] private float radius = 3f;
    [SerializeField] private float healPerSecond = 10f;
    [SerializeField] private LayerMask targetLayers;

    private readonly List<Health> targetsInRange = new List<Health>();

    private void Update()
    {
        // Find all colliders in range
        Collider[] hits = Physics.OverlapSphere(transform.position, radius, targetLayers);

        targetsInRange.Clear();

        foreach (var hit in hits)
        {
            Health health = hit.GetComponent<Health>();
            if (health != null && !health.isDead)
            {
                targetsInRange.Add(health);
            }
        }

        // Heal all valid targets
        foreach (var target in targetsInRange)
        {
            Heal(target);
        }
    }

    private void Heal(Health target)
    {
        if (target.currentHealth >= target.maxHealth) return;

        float healAmount = healPerSecond * Time.deltaTime;
        target.currentHealth += healAmount;
        target.currentHealth = Mathf.Clamp(target.currentHealth, 0, target.maxHealth);

        // Optional debug
        Debug.Log($"{target.gameObject.name} healed {healAmount} HP");
    }

    // Draw the radius in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}