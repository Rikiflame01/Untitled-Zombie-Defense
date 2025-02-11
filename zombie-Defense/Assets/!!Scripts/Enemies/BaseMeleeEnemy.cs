using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class BaseMeleeEnemy : MonoBehaviour
{
    private enum AIState
    {
        CheckPath,
        ChasePlayer,
        AttackObstacle,
        AttackPlayer
    }

    [Header("Grid Utility Reference")]
    [SerializeField] private AIGridUtility gridUtility;

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
    [SerializeField] private int attackDamage = 10;

    [Header("Destructible Obstacle Settings")]
    [SerializeField] private float destructibleObjectCheckRate = 10f;
    [SerializeField] private float destructibleCheckDistance = 1f; 
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Player Layer")]
    [SerializeField] private LayerMask playerLayer;

    [Header("Raycast Eye Settings")]
    [SerializeField, Tooltip("Vertical offset for the raycast origin (eye height)")]
    private float raycastEyeHeight = 1f;

    [Header("Debug - Destructible Walls")]
    [SerializeField] private List<Transform> destructibleObjects = new List<Transform>();

    private Vector3 lastRayOrigin;
    private Vector3 lastRayDirection;

    private NavMeshAgent navMeshAgent;
    private AIState currentState = AIState.CheckPath;
    private float lastAttackTime = -1f;
    private Transform currentObstacleTarget;
    private Coroutine destructibleCheckCoroutine;

    private float stuckStartTime = 0f;
    private const float STUCK_THRESHOLD = 5f;

    private float nextPathResetTime = 0f;
    private const float pathResetCooldown = 1f;

    private float lastDestinationUpdateTime = 0f;

    private void Awake()
    {
        gridUtility = FindFirstObjectByType<AIGridUtility>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.autoBraking = false;
        navMeshAgent.autoRepath = true;
        navMeshAgent.speed = moveSpeed;
        navMeshAgent.stoppingDistance = stoppingDistance;

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
                player = p.transform;
        }
    }

    private void Start()
    {
        TransitionToState(AIState.CheckPath);
    }

        private void OnEnable()
    {
        ActionManager.OnWallDestroyed += OnWallDestroyedHandler;
    }


    private void OnDisable()
    {
        ActionManager.OnWallDestroyed -= OnWallDestroyedHandler;
    }
    private void OnWallDestroyedHandler()
    {
        Debug.Log("Wall destroyed event received, checking player surround status.");
        if (gridUtility != null)
        {
            if (!gridUtility.IsPlayerSurrounded(gridUtility.GetPlayerCellX(), gridUtility.GetPlayerCellZ()))
            {
                Debug.Log("Player is no longer surrounded, updating path.");
                TransitionToState(AIState.CheckPath);
            }
            else
            {
                StartCoroutine(CheckForDestructibleObjects());
            }
        }
    }

    private void Update()
    {
        if (player == null)
            return;

        switch (currentState)
        {
            case AIState.CheckPath:
                UpdateCheckPath();
                break;
            case AIState.ChasePlayer:
                UpdateChasePlayer();
                break;
            case AIState.AttackObstacle:
                UpdateAttackObstacle();
                break;
            case AIState.AttackPlayer:
                UpdateAttackPlayer();
                break;
        }
    }
    private void UpdateCheckPath()
    {
        if (gridUtility != null)
        {
            if (gridUtility.IsPlayerSurrounded(gridUtility.GetPlayerCellX(), gridUtility.GetPlayerCellZ()))
            {
                Debug.Log("Player is surrounded, looking for obstacles to attack.");
                StartCoroutine(CheckForDestructibleObjects());
                MoveTowardsPotentialObstacle();
                return;
            }
        }

        navMeshAgent.SetDestination(player.position);
        TransitionToState(AIState.ChasePlayer);
        Debug.Log("Player is not surrounded, transitioning to ChasePlayer.");
    }    
    private void MoveTowardsPotentialObstacle()
    {
        if (destructibleObjects.Count > 0)
        {
            Transform nearestObstacle = GetNearestDestructibleObject();
            if (nearestObstacle != null)
            {
                navMeshAgent.SetDestination(nearestObstacle.position);
                Debug.Log("Moving towards nearest obstacle.");
            }
            else
            {
                Vector3 randomPoint = transform.position + new Vector3(Random.Range(-10f, 10f), 0, Random.Range(-10f, 10f));
                navMeshAgent.SetDestination(randomPoint);
                Debug.Log("No specific obstacle found, moving randomly.");
            }
        }
        else
        {
            Debug.Log("No destructible objects detected. Moving randomly.");
            Vector3 randomPoint = transform.position + new Vector3(Random.Range(-10f, 10f), 0, Random.Range(-10f, 10f));
            navMeshAgent.SetDestination(randomPoint);
        }
    }

    private Transform GetNearestDestructibleObject()
    {
        Transform closest = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = transform.position;
        foreach (Transform potentialTarget in destructibleObjects)
        {
            if (potentialTarget != null)
            {
                Vector3 directionToTarget = potentialTarget.position - currentPosition;
                float dSqrToTarget = directionToTarget.sqrMagnitude;
                if (dSqrToTarget < closestDistanceSqr)
                {
                    closestDistanceSqr = dSqrToTarget;
                    closest = potentialTarget;
                }
            }
        }
        return closest;
    }
    private void UpdateChasePlayer()
    {
        if (!navMeshAgent.pathPending && 
        Vector3.Distance(navMeshAgent.destination, player.position) > 0.5f)
        {
            navMeshAgent.SetDestination(player.position);
        }

        // if (navMeshAgent.remainingDistance < navMeshAgent.stoppingDistance + 0.4f)
        // {
        //     if (stuckStartTime == 0f)
        //     {
        //         stuckStartTime = Time.time;
        //     }
        //     else if (Time.time - stuckStartTime > STUCK_THRESHOLD)
        //     {
        //         Debug.Log("Enemy seems stuck. Resetting path.");
        //         stuckStartTime = 0f;

        //         if (gridUtility != null)
        //         {
        //             if (gridUtility.IsPlayerSurrounded(gridUtility.GetPlayerCellX(), gridUtility.GetPlayerCellZ()))
        //             {
        //                 Debug.Log("Player is surrounded while chasing, looking for obstacles.");
        //                 TransitionToState(AIState.CheckPath); // Or directly to AttackObstacle if possible
        //                 return;
        //             }
        //         }
        //     }
        // }
        // else
        // {
        //     stuckStartTime = 0f;
        // }

        float distToPlayer = Vector3.Distance(transform.position, player.position);
        if (distToPlayer <= attackRange && Time.time - lastAttackTime >= attackCooldown)
        {
            TransitionToState(AIState.AttackPlayer);
        }
    }

    private IEnumerator CheckForDestructibleObjects()
    {
            WaitForSeconds wait = new WaitForSeconds(1f / destructibleObjectCheckRate);
            Vector3[] corners = new Vector3[2];

                if (currentState != AIState.AttackObstacle && currentState != AIState.AttackPlayer)
                {
                    int cornerCount = navMeshAgent.path.GetCornersNonAlloc(corners);
                    if (cornerCount > 1)
                    {
                    lastRayOrigin = corners[0] + Vector3.up * raycastEyeHeight;
                    lastRayDirection = (corners[1] - corners[0]).normalized;

                    Debug.DrawRay(lastRayOrigin, lastRayDirection * destructibleCheckDistance, Color.magenta, 0.5f);

                    if (Physics.Raycast(lastRayOrigin, lastRayDirection, out RaycastHit hit, destructibleCheckDistance, obstacleLayer, QueryTriggerInteraction.Collide))
                    {
                        Health potential = hit.collider.GetComponentInParent<Health>();
                        if (potential != null)
                        {
                            Debug.Log($"Destructible object found: {potential.gameObject.name}");
                            currentObstacleTarget = potential.transform;
                            SubscribeToObstacleDeath(currentObstacleTarget);
                            TransitionToState(AIState.AttackObstacle);
                            yield break;
                        }
                    }

                    if (gridUtility != null && gridUtility.IsPlayerSurrounded(gridUtility.GetPlayerCellX(), gridUtility.GetPlayerCellZ()))
                    {
                        Debug.Log("Player surrounded, continuing to look for obstacles to destroy.");
                    }
                }
            yield return wait;
        }
    }
    private void UpdateAttackPlayer()
    {
        float distToPlayer = Vector3.Distance(transform.position, player.position);
        if (distToPlayer <= attackRange && Time.time - lastAttackTime >= attackCooldown)
        {
            AttackPlayer();
        }
        else if (distToPlayer > attackRange)
        {
            TransitionToState(AIState.ChasePlayer);
        }
    }

private void UpdateAttackObstacle()
{
    if (currentObstacleTarget == null || !currentObstacleTarget.gameObject.activeInHierarchy)
    {
        Debug.Log("Current obstacle target is invalid or destroyed.");
        TransitionToState(AIState.CheckPath);
        return;
    }

    float distToObstacle = Vector3.Distance(transform.position, currentObstacleTarget.position);
    if (distToObstacle <= attackRange && Time.time - lastAttackTime >= attackCooldown)
    {
        AttackObstacle();
    }
    else
    {
        if (Time.time - lastDestinationUpdateTime >= 3f)
        {
            navMeshAgent.SetDestination(currentObstacleTarget.position);
            lastDestinationUpdateTime = Time.time;
        }
        
        if (navMeshAgent.remainingDistance < 0.1f && navMeshAgent.velocity.sqrMagnitude < 0.1f)
        {
            if (Time.time > nextPathResetTime)
            {
                Debug.Log("Agent seems stuck, remaining distance: " + navMeshAgent.remainingDistance + ", velocity: " + navMeshAgent.velocity);
                navMeshAgent.ResetPath();
                nextPathResetTime = Time.time + pathResetCooldown;
            }
        }
    }
}
    private void AttackPlayer()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, sphereCheckRadius, playerLayer);
        foreach (Collider c in hits)
        {
            if (c.transform == player)
            {
                Health hp = player.GetComponent<Health>();
                if (hp != null)
                {
                    hp.TakeDamage(attackDamage);
                    lastAttackTime = Time.time;
                }
                break;
            }
        }
    }

    private void AttackObstacle()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, sphereCheckRadius, obstacleLayer);
        foreach (Collider c in hits)
        {
            Health oh = c.transform.GetComponentInParent<Health>();
            if (oh != null && oh.transform == currentObstacleTarget)
            {
                oh.TakeDamage(attackDamage);
                lastAttackTime = Time.time;
                if (!currentObstacleTarget.gameObject.activeInHierarchy || oh.currentHealth <= 0)
                {
                    UnsubscribeFromObstacleDeath(currentObstacleTarget);
                    currentObstacleTarget = null;
                    StartCoroutine(DelayedPathCheck());
                }
                break;
            }
        }
    }

    private bool HasValidPathToPlayer()
    {
        NavMeshPath path = new NavMeshPath();
        navMeshAgent.CalculatePath(player.position, path);
        return path.status == NavMeshPathStatus.PathComplete;
    }

    private IEnumerator DelayedPathCheck()
    {
        yield return new WaitForSeconds(0.5f);
        if (HasValidPathToPlayer())
        {
            Debug.Log("Path to player is now clear; resuming chase.");
            TransitionToState(AIState.ChasePlayer);
        }
        else
        {
            Debug.Log("Path to player is still blocked; searching for next obstacle.");
            TransitionToState(AIState.CheckPath);
        }
    }

    private void SubscribeToObstacleDeath(Transform obstacle)
    {
        if (obstacle != null)
        {
            Health health = obstacle.GetComponentInParent<Health>();
            if (health != null)
                health.OnDied += OnObstacleDied;
        }
    }

    private void UnsubscribeFromObstacleDeath(Transform obstacle)
    {
        if (obstacle != null)
        {
            Health health = obstacle.GetComponentInParent<Health>();
            if (health != null)
                health.OnDied -= OnObstacleDied;
        }
    }

    private void OnObstacleDied(GameObject obstacle)
    {
        if (currentObstacleTarget != null && obstacle == currentObstacleTarget.gameObject)
        {
            Debug.Log("Obstacle died, transitioning to CheckPath");
            UnsubscribeFromObstacleDeath(currentObstacleTarget);
            currentObstacleTarget = null;
            TransitionToState(AIState.CheckPath);
        }
    }

    private void TransitionToState(AIState newState)
    {
        if (currentState == AIState.ChasePlayer && destructibleCheckCoroutine != null)
        {
            StopCoroutine(destructibleCheckCoroutine);
            destructibleCheckCoroutine = null;
        }

        currentState = newState;

        if (newState == AIState.ChasePlayer || newState == AIState.CheckPath)
        {
            navMeshAgent.isStopped = false;
            if (destructibleCheckCoroutine == null)
                destructibleCheckCoroutine = StartCoroutine(CheckForDestructibleObjects());
        }
    }

    private IEnumerator DelayedChaseStart()
    {
        yield return new WaitForSeconds(1f);
        if (destructibleCheckCoroutine == null)
            destructibleCheckCoroutine = StartCoroutine(CheckForDestructibleObjects());
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, sphereCheckRadius);

        if (destructibleObjects != null)
        {
            Gizmos.color = Color.cyan;
            foreach (Transform t in destructibleObjects)
            {
                if (t != null)
                    Gizmos.DrawSphere(t.position, 0.3f);
            }
        }

        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(lastRayOrigin, lastRayOrigin + lastRayDirection * destructibleCheckDistance);
    }
}