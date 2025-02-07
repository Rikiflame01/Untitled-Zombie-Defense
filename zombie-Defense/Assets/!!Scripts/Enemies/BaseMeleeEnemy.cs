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

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.autoBraking = false;
        navMeshAgent.autoRepath = false;
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
        float distToPlayer = Vector3.Distance(transform.position, player.position);
        if (distToPlayer > detectionRange)
        {
            navMeshAgent.ResetPath();
            return;
        }
        navMeshAgent.SetDestination(player.position);
        TransitionToState(AIState.ChasePlayer);
    }

    private void UpdateChasePlayer()
    {
        navMeshAgent.SetDestination(player.position);

        float distToPlayer = Vector3.Distance(transform.position, player.position);
        if (distToPlayer <= attackRange && Time.time - lastAttackTime >= attackCooldown)
        {
            TransitionToState(AIState.AttackPlayer);
        }
    }

    private void UpdateAttackPlayer()
    {
        navMeshAgent.isStopped = true;
        float distToPlayer = Vector3.Distance(transform.position, player.position);
        if (distToPlayer <= attackRange && Time.time - lastAttackTime >= attackCooldown)
        {
            AttackPlayer();
        }
        else if (distToPlayer > attackRange)
        {
            navMeshAgent.isStopped = false;
            TransitionToState(AIState.ChasePlayer);
        }
    }

    private void UpdateAttackObstacle()
    {
        navMeshAgent.isStopped = false;
        if (currentObstacleTarget == null || !currentObstacleTarget.gameObject.activeInHierarchy)
        {
            TransitionToState(AIState.CheckPath);
            return;
        }

        float distToObstacle = Vector3.Distance(transform.position, currentObstacleTarget.position);
        if (distToObstacle <= attackRange && Time.time - lastAttackTime >= attackCooldown)
        {
            navMeshAgent.isStopped = true;
            AttackObstacle();
        }
        else
        {
            navMeshAgent.SetDestination(currentObstacleTarget.position);
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

    private IEnumerator CheckForDestructibleObjects()
    {
        WaitForSeconds wait = new WaitForSeconds(1f / destructibleObjectCheckRate);
        Vector3[] corners = new Vector3[2];

        while (currentState == AIState.ChasePlayer)
        {
            int cornerCount = navMeshAgent.path.GetCornersNonAlloc(corners);
            Debug.Log($"Corner count: {cornerCount}");
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
                        if (HasValidPathToPlayer())
                        {
                            Debug.Log("Path to player is clear; no need to attack obstacle.");
                        }
                        else
                        {
                            Debug.Log($"Destructible object found: {potential.gameObject.name}");
                            currentObstacleTarget = potential.transform;
                            SubscribeToObstacleDeath(currentObstacleTarget);
                            TransitionToState(AIState.AttackObstacle);
                            yield break;
                        }
                    }
                }
                else
                {
                    Debug.Log("Raycast did not hit any obstacle.");
                }
            }
            yield return wait;
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
            UnsubscribeFromObstacleDeath(currentObstacleTarget);
            currentObstacleTarget = null;
            StartCoroutine(DelayedPathCheck());
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

        if (newState == AIState.ChasePlayer)
        {
            navMeshAgent.isStopped = false;
            if (destructibleCheckCoroutine == null)
                destructibleCheckCoroutine = StartCoroutine(CheckForDestructibleObjects());
        }
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
