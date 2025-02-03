using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class BaseMeleeEnemy : MonoBehaviour
{
    [Header("Player Settings")]
    [SerializeField] private Transform player;          
    [SerializeField] private float detectionRange = 10f;
    
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3.5f; 
    [SerializeField] private float stoppingDistance = 2f;
    
    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float sphereCheckRadius = 2f;
    
    [Header("Layer Masks")]
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private LayerMask playerLayer;   
    
    private NavMeshAgent navMeshAgent;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        navMeshAgent.speed = moveSpeed;
        navMeshAgent.stoppingDistance = stoppingDistance;
    }

    private void Update()
    {
        if (player == null) return;

        FollowPlayer();
        AttackPlayer();
    }

    private void FollowPlayer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange)
        {
            navMeshAgent.SetDestination(player.position);

            if (navMeshAgent.pathStatus == NavMeshPathStatus.PathInvalid 
                || navMeshAgent.pathStatus == NavMeshPathStatus.PathPartial)
            {
                RaycastHit hit;
                if (Physics.Linecast(transform.position, player.position, out hit, obstacleLayer))
                {
                    if (((1 << hit.collider.gameObject.layer) & obstacleLayer) != 0)
                    {
                        Destroy(hit.collider.gameObject);
                    }
                }
            }
        }
        else
        {
            navMeshAgent.ResetPath();
        }
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
                    Debug.Log("Player has been found by BaseMeleeEnemy!");
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, sphereCheckRadius);
    }
}
