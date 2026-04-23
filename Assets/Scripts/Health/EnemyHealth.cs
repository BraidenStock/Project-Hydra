using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyHealth : MonoBehaviour
{
    public Health health;
    private HostileAI ai;

    private void Awake()
    {
        ai = GetComponent<HostileAI>();
    }

    private void Update()
    {
        if (health.currentHealth <= 0)
        {
            if (ai != null)
                ai.enabled = false;
        }
    }
}