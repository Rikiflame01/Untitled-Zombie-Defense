using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class BaseMeleeEnemy : MonoBehaviour
{
    private enum AIState
    {
        CheckPath,
        ChasePlayer,
        FindObstacle,
        MoveToObstacle,
        AttackObstacle
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

    [Header("Layer Masks")]
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private LayerMask playerLayer;

    [Header("Optional EyePoint")]
    [SerializeField] private Transform eyePoint;

    private AIState currentState = AIState.CheckPath;
    private NavMeshAgent navMeshAgent;

    private Transform currentObstacleTarget;
    private Vector3 obstacleNavmeshPoint;

    private float lastAttackTime = -1f;
    private float moveToObstacleStartTime = 0f;
    private Vector3 lastPlayerDestination;

    private float pathCheckCooldown = 1.5f;
    private float lastPathCheckTime = 0f;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();

        navMeshAgent.autoBraking = false;
        navMeshAgent.autoRepath = false;
        navMeshAgent.speed = moveSpeed;
        navMeshAgent.stoppingDistance = stoppingDistance;

        if (!player)
        {
            PlayerControls pc = FindFirstObjectByType<PlayerControls>();
            if (pc)
                player = pc.transform;
        }
    }

    private void Start()
    {
        TransitionToState(AIState.CheckPath);
    }

    private void Update()
    {
        if (!player) return;

        switch (currentState)
        {
            case AIState.CheckPath:
                UpdateCheckPath();
                break;
            case AIState.ChasePlayer:
                UpdateChasePlayer();
                break;
            case AIState.FindObstacle:
                UpdateFindObstacle();
                break;
            case AIState.MoveToObstacle:
                UpdateMoveToObstacle();
                break;
            case AIState.AttackObstacle:
                UpdateAttackObstacle();
                break;
        }
    }

    private void TransitionToState(AIState newState)
    {
        if (currentState == AIState.MoveToObstacle || currentState == AIState.AttackObstacle)
        {
            if (currentObstacleTarget != null)
            {
                UnsubscribeFromObstacleDeath(currentObstacleTarget);
            }
        }

        if (currentState == AIState.MoveToObstacle)
        {
            moveToObstacleStartTime = 0f;
        }

        if (newState != AIState.AttackObstacle) 
        {
            navMeshAgent.isStopped = false;
        }

        currentState = newState;
    }


    #region State Updates

    private void UpdateCheckPath()
    {
        float distToPlayer = Vector3.Distance(transform.position, player.position);
        if (distToPlayer > detectionRange)
        {
            navMeshAgent.ResetPath();
            return;
        }

        if (HasFullyValidPathToPlayer())
            TransitionToState(AIState.ChasePlayer);
        else
            TransitionToState(AIState.FindObstacle);
    }

    private void UpdateChasePlayer()
    {

        if (Vector3.Distance(lastPlayerDestination, player.position) > 1f)
        {
            lastPlayerDestination = player.position;
            navMeshAgent.SetDestination(lastPlayerDestination);
        }
        navMeshAgent.isStopped = false;

        float distToPlayer = Vector3.Distance(transform.position, player.position);
        if (distToPlayer <= attackRange && Time.time - lastAttackTime >= attackCooldown)
        {
            AttackPlayer();
        }


        if (!HasFullyValidPathToPlayer())
            TransitionToState(AIState.FindObstacle);
    }

    private void UpdateFindObstacle()
    {

        Transform obstacle = BestPathManager.GetNearestBestPathObstacle(transform.position);


        if (obstacle == null)
        {
            obstacle = FindNearestObstacle(transform.position, detectionRange);
        }

        if (obstacle)
        {
            if (currentObstacleTarget != obstacle)
            {
                if (currentObstacleTarget != null)
                    UnsubscribeFromObstacleDeath(currentObstacleTarget);

                currentObstacleTarget = obstacle;
                SubscribeToObstacleDeath(currentObstacleTarget);
            }
            obstacleNavmeshPoint = GetNavmeshPointNear(obstacle.position, 5f);
            TransitionToState(AIState.MoveToObstacle);
        }
        else
        {
            TransitionToState(AIState.CheckPath);
        }
    }

    private void UpdateMoveToObstacle()
    {
        if (!currentObstacleTarget || !currentObstacleTarget.gameObject.activeInHierarchy)
        {
            TransitionToState(AIState.CheckPath);
            return;
        }

        if (Time.time - lastPathCheckTime > pathCheckCooldown)
        {
            lastPathCheckTime = Time.time;
            if (HasFullyValidPathToPlayer())
            {
                TransitionToState(AIState.ChasePlayer);
                return;
            }
        }

        if (moveToObstacleStartTime == 0f)
            moveToObstacleStartTime = Time.time;
        else if (Time.time - moveToObstacleStartTime > 5f)
        {
            TransitionToState(AIState.FindObstacle);
            moveToObstacleStartTime = 0f;
            return;
        }

        navMeshAgent.isStopped = false;
        navMeshAgent.SetDestination(obstacleNavmeshPoint);

        if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance + 0.2f)
        {
            float distToObstacle = Vector3.Distance(transform.position, currentObstacleTarget.position);
            if (distToObstacle <= attackRange)
                TransitionToState(AIState.AttackObstacle);
            else
                navMeshAgent.SetDestination(currentObstacleTarget.position);
        }
    }
   
   
   
    private void UpdateAttackObstacle()
    {
        navMeshAgent.isStopped = true;

        if (!currentObstacleTarget || !currentObstacleTarget.gameObject.activeInHierarchy)
        {
            TransitionToState(AIState.CheckPath);
            return;
        }

        if (Vector3.Distance(transform.position, currentObstacleTarget.position) <= attackRange &&
            Time.time - lastAttackTime >= attackCooldown)
        {
            AttackObstacle();
        }
    }

    #endregion

    #region Attack Methods

    private void AttackPlayer()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, sphereCheckRadius, playerLayer);
        foreach (Collider c in hits)
        {
            if (c.transform == player)
            {
                Health hp = player.GetComponent<Health>();
                if (hp)
                    hp.TakeDamage(10);
                lastAttackTime = Time.time;
                break;
            }
        }
    }

    private void AttackObstacle()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, sphereCheckRadius, obstacleLayer);
        foreach (Collider c in hits)
        {
            if (c.transform == currentObstacleTarget)
            {
                Health oh = currentObstacleTarget.GetComponent<Health>();
                if (oh)
                {
                    oh.TakeDamage(10);
                    lastAttackTime = Time.time;

                    if (!currentObstacleTarget.gameObject.activeInHierarchy || oh.currentHealth <= 0)
                    {
                        UnsubscribeFromObstacleDeath(currentObstacleTarget);
                        currentObstacleTarget = null;
                        TransitionToState(AIState.CheckPath);
                    }
                }
                break;
            }
        }
    }

    #endregion

    #region Utility Methods

    private bool HasFullyValidPathToPlayer()
    {
        if (!player)
            return false;

        NavMeshPath path = new NavMeshPath();
        navMeshAgent.CalculatePath(player.position, path);
        return path.status == NavMeshPathStatus.PathComplete;
    }

    private Transform FindNearestObstacle(Vector3 center, float range)
    {
        Collider[] cols = Physics.OverlapSphere(center, range, obstacleLayer);
        float minDist = Mathf.Infinity;
        Transform nearest = null;
        foreach (Collider col in cols)
        {
            float dist = Vector3.Distance(center, col.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = col.transform;
            }
        }
        return nearest;
    }

    private Vector3 GetNavmeshPointNear(Vector3 pos, float maxDist)
    {
        if (NavMesh.SamplePosition(pos, out NavMeshHit hit, maxDist, NavMesh.AllAreas))
            return hit.position;

        return pos;
    }

    private void SubscribeToObstacleDeath(Transform obstacle)
    {
        if (obstacle != null)
        {
            Health health = obstacle.GetComponent<Health>();
            if (health != null)
                health.OnDied += OnObstacleDied;
        }
    }

    private void UnsubscribeFromObstacleDeath(Transform obstacle)
    {
        if (obstacle != null)
        {
            Health health = obstacle.GetComponent<Health>();
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
            TransitionToState(AIState.CheckPath);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, sphereCheckRadius);
    }

    #endregion

}
