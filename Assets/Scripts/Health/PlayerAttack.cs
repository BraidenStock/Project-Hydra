using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private Inventory inventory;
    [SerializeField] private float attackRange = 2f;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) // left click
        {
            Attack();
        }
    }

    private void Attack()
    {
        ItemSO item = inventory.GetActiveHotbarItem();

        if (item == null) return;

        // ❗ only damage if allowed
        if (!item.doesDamage) return;

        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, attackRange))
        {
            Health target = hit.collider.GetComponent<Health>();

            if (target != null && !target.isDead)
            {
                target.TakeDamage(item.damageAmount);
                Debug.Log($"Hit {target.name} for {item.damageAmount}");
            }
        }
    }
}