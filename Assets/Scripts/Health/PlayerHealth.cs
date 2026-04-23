using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public Health health;

    private void Awake()
    {
        if (health == null)
            health = GetComponent<Health>();
    }

    private void Update()
    {
        if (health.isDead)
        {
            Debug.Log("Game Over / Respawn logic here");
        }
    }
}