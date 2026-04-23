using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float damage = 10f;

    private void OnCollisionEnter(Collision collision)
    {
        Health health = collision.collider.GetComponent<Health>();

        if (health != null)
        {
            health.TakeDamage(damage);
            Debug.Log("Hit: " + collision.collider.name);
        }

        Destroy(gameObject);
    }
}