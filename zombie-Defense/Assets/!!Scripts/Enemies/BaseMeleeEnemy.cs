using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class BaseMeleeEnemy : MonoBehaviour
{
    [Header("Player Settings")]
    [SerializeField] private Transform player;          
    [SerializeField] private float detectionRange = 50f;
    
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3.5f; 
    [SerializeField] private float stoppingDistance = 2f;
    
    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float sphereCheckRadius = 2f;
    [SerializeField] private float attackCooldown = 1f;

    [Header("Layer Masks")]
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private LayerMask playerLayer;   

    [Header("Transforms")]
    [SerializeField] private Transform eyePoint;

    private NavMeshAgent navMeshAgent;
    private Transform currentObstacleTarget;
    private float lastAttackTime = -1f;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();

        PlayerControls playerControls = FindFirstObjectByType<PlayerControls>();
        if (playerControls != null)
            player = playerControls.transform;
    }

    private void Start()
    {
        navMeshAgent.speed = moveSpeed;
        navMeshAgent.stoppingDistance = stoppingDistance;
    }

    private void Update()
    {
        if (player == null) return;

        FollowOrAttackObstacle();

        if (Time.time - lastAttackTime >= attackCooldown)
        {
            AttackPlayer();
            AttackObstacle();
        }
    }

    private void FollowOrAttackObstacle()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer > detectionRange)
        {
            currentObstacleTarget = null;
            navMeshAgent.ResetPath();
            return;
        }

        NavMeshPath path = new NavMeshPath();
        navMeshAgent.CalculatePath(player.position, path);

        if (path.status == NavMeshPathStatus.PathComplete)
        {
            currentObstacleTarget = null;
            navMeshAgent.SetDestination(player.position);
        }
        else
        {
            if (currentObstacleTarget == null)
            {
                Vector3 eyePos = (eyePoint != null) ? eyePoint.position : transform.position;
                if (Physics.Linecast(eyePos, player.position, out RaycastHit hit, obstacleLayer))
                {
                    currentObstacleTarget = hit.collider.transform;
                }
            }

            if (currentObstacleTarget != null)
            {
                if (currentObstacleTarget.gameObject.activeInHierarchy)
                {
                    Vector3 nearObstacle = GetNearestNavMeshPoint(currentObstacleTarget.position);
                    navMeshAgent.SetDestination(nearObstacle);
                }
                else
                {
                    currentObstacleTarget = null;
                    navMeshAgent.SetDestination(player.position);
                }
            }
        }
    }

    private Vector3 GetNearestNavMeshPoint(Vector3 targetPosition, float maxDistance = 5f)
    {
        if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, maxDistance, NavMesh.AllAreas))
        {
            return hit.position;
        }
        return transform.position;
    }

    private void AttackPlayer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange)
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, sphereCheckRadius, playerLayer);
            foreach (Collider collider in hitColliders)
            {
                if (collider.transform == player)
                {
                    Health playerHealth = player.GetComponent<Health>();
                    playerHealth.TakeDamage(10);
                    
                    lastAttackTime = Time.time;
                }
            }
        }
    }

    private void AttackObstacle()
    {
        if (currentObstacleTarget == null) return;

        float distanceToObstacle = Vector3.Distance(transform.position, currentObstacleTarget.position);
        if (distanceToObstacle <= attackRange)
        {
            Health obstacleHealth = currentObstacleTarget.GetComponent<Health>();
            if (obstacleHealth != null)
            {
                obstacleHealth.TakeDamage(10);

                lastAttackTime = Time.time;

                if (!currentObstacleTarget.gameObject.activeInHierarchy)
                {
                    currentObstacleTarget = null;
                    navMeshAgent.SetDestination(player.position);
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (player == null) return;

        Vector3 eyePos = (eyePoint != null) ? eyePoint.position : transform.position;

        if (Physics.Linecast(eyePos, player.position, out RaycastHit hit))
        {
            if (hit.transform == player 
                || ((1 << hit.transform.gameObject.layer) & obstacleLayer) == 0)
            {
                Gizmos.color = Color.green;
            }
            else
            {
                Gizmos.color = Color.red;
            }
            Gizmos.DrawLine(eyePos, player.position);
        }
        else
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(eyePos, player.position);
        }
    }
}
