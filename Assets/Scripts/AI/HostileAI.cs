using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class HostileAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NavMeshAgent navMeshAgent;
    private Transform playerTransform;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject projectilePrefab;

    [Header("Patrol Settings")]
    [SerializeField] private float patrolRadius = 10f;
    private Vector3 currentPatrolPoint;
    private bool hasPatrolPoint;

    [Header("Combat Settings")]
    [SerializeField] private float attackCooldown = 1f;
    private bool isOnAttackCooldown;
    [SerializeField] private float forwardShotForce = 10f;
    [SerializeField] private float verticalShotForce = 5f;

    [Header("Detection Ranges")]
    [SerializeField] private float visionRange = 20f;
    [SerializeField] private float engagementRange = 10f;

    private bool isPlayerVisible;
    private bool isPlayerInRange;

    private void Awake()
    {
        if (navMeshAgent == null)
            navMeshAgent = GetComponent<NavMeshAgent>();

        if (navMeshAgent == null)
        {
            Debug.LogError("❌ NavMeshAgent missing!");
            return;
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            Debug.Log("✔ Player found: " + playerObj.name);
        }
        else
        {
            Debug.LogError("❌ Player not found (check tag)");
        }

        if (!navMeshAgent.isOnNavMesh)
        {
            Debug.LogError("❌ AI is NOT placed on NavMesh at start!");
        }

        Debug.Log("AI initialized");
    }

    private void Update()
    {
        if (playerTransform == null || navMeshAgent == null) return;

        DetectPlayer();
        UpdateBehaviorState();

        Debug.Log("On NavMesh: " + navMeshAgent.isOnNavMesh);
    }

    // -----------------------------
    // DETECTION (FIXED)
    // -----------------------------
    private void DetectPlayer()
    {
        if (playerTransform == null) return;

        float dist = Vector3.Distance(transform.position, playerTransform.position);

        isPlayerVisible = dist <= visionRange;
        isPlayerInRange = dist <= engagementRange;
    }

    // -----------------------------
    // STATE MACHINE
    // -----------------------------
    private void UpdateBehaviorState()
    {
        if (!isPlayerVisible)
        {
            PerformPatrol();
        }
        else if (isPlayerVisible && !isPlayerInRange)
        {
            PerformChase();
        }
        else if (isPlayerVisible && isPlayerInRange)
        {
            PerformAttack();
        }
    }

    // -----------------------------
    // PATROL (FIXED)
    // -----------------------------
    private void PerformPatrol()
    {
        if (!hasPatrolPoint)
        {
            FindPatrolPoint();
        }

        if (hasPatrolPoint)
        {
            navMeshAgent.SetDestination(currentPatrolPoint);

            if (Vector3.Distance(transform.position, currentPatrolPoint) < 1f)
            {
                hasPatrolPoint = false;
            }
        }
    }

    private void FindPatrolPoint()
    {
        for (int i = 0; i < 10; i++)
        {
            float randX = Random.Range(-patrolRadius, patrolRadius);
            float randZ = Random.Range(-patrolRadius, patrolRadius);

            Vector3 point = transform.position + new Vector3(randX, 0, randZ);

            if (NavMesh.SamplePosition(point, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                currentPatrolPoint = hit.position;
                hasPatrolPoint = true;

                Debug.Log("✔ Patrol point: " + hit.position);
                return;
            }
        }

        Debug.Log("❌ Failed to find patrol point");
    }

    // -----------------------------
    // CHASE
    // -----------------------------
    private void PerformChase()
    {
        if (playerTransform == null) return;

        navMeshAgent.SetDestination(playerTransform.position);
    }

    // -----------------------------
    // ATTACK (FIXED)
    // -----------------------------
    private void PerformAttack()
    {
        navMeshAgent.ResetPath(); // FIX: stop properly

        if (playerTransform != null)
        {
            transform.LookAt(playerTransform);
        }

        if (!isOnAttackCooldown)
        {
            FireProjectile();
            StartCoroutine(AttackCooldownRoutine());
        }
    }

    // -----------------------------
    // SHOOTING
    // -----------------------------
    private void FireProjectile()
    {
        if (projectilePrefab == null || firePoint == null)
        {
            Debug.LogWarning("⚠ Missing projectile or firePoint");
            return;
        }

        Rigidbody rb = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity)
            .GetComponent<Rigidbody>();

        rb.AddForce(transform.forward * forwardShotForce, ForceMode.Impulse);
        rb.AddForce(transform.up * verticalShotForce, ForceMode.Impulse);
    }

    private IEnumerator AttackCooldownRoutine()
    {
        isOnAttackCooldown = true;
        yield return new WaitForSeconds(attackCooldown);
        isOnAttackCooldown = false;
    }
}