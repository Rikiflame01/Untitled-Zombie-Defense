using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerControls : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    
    [Header("Shooting Settings")]
    [SerializeField] private Transform shootingPoint;
    [SerializeField] private string bulletPoolTag = "Bullet";
    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField] private float shootRadius = 1.5f; 
    [SerializeField] private float shootHeightOffset = 1f;

    [Header("Prediction Line Settings")]
    [SerializeField] private LineRenderer predictionLine;
    [SerializeField, Tooltip("Number of segments for the prediction line")] private int predictionSegments = 20;
    [SerializeField, Tooltip("Time step (in seconds) between prediction segments")] private float predictionTimeStep = 0.1f;

    private InputSystem_Actions _inputActions;
    private CharacterController _characterController;
    private Vector2 _moveInput;
    [SerializeField] private LayerMask obstacleLayer;

    private void Awake()
    {
        _inputActions = new InputSystem_Actions();
        _characterController = GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        Health health = GetComponent<Health>();
        health.OnDied += DisableScript;
        _inputActions.Enable();
        _inputActions.Player.Move.performed += OnMove;
        _inputActions.Player.Move.canceled += OnMove;
        _inputActions.Player.Shoot.performed += OnShoot;
        _inputActions.Player.SkipBuild.performed += SkipBuildEarly;
    }

    private void OnDisable()
    {
        _inputActions.Player.Move.performed -= OnMove;
        _inputActions.Player.Move.canceled -= OnMove;
        _inputActions.Player.Shoot.performed -= OnShoot;
        _inputActions.Disable();
        _inputActions.Player.SkipBuild.performed -= SkipBuildEarly;
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }

    private void Update()
    {
        Vector3 move = new Vector3(_moveInput.x, 0f, _moveInput.y);
        _characterController.Move(move * (moveSpeed * Time.deltaTime));

        UpdateShootingPoint();
    }

    private void DisableScript(GameObject obj)
    {
        if (obj.CompareTag("Player"))
        {
            this.enabled = false;
        }
    }

    private void SkipBuildEarly(InputAction.CallbackContext context)
    {
        ActionManager.InvokeBuildSkip();
    }
    
    private void UpdateShootingPoint()
    {
        if (Camera.main == null || Mouse.current == null)
            return;

        Vector3 direction;
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;
        int groundLayerMask = LayerMask.GetMask("Ground");

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayerMask))
        {
            direction = (hit.point - transform.position).normalized;
        }
        else
        {
            Vector3 mousePos = Mouse.current.position.ReadValue();
            Vector3 screenPoint = new Vector3(mousePos.x, mousePos.y, 10f);
            Vector3 worldPoint = Camera.main.ScreenToWorldPoint(screenPoint);
            direction = (worldPoint - transform.position).normalized;
        }

        direction.y = 0f;

        Vector3 basePosition = transform.position + Vector3.up * shootHeightOffset;
        shootingPoint.position = basePosition + direction * shootRadius;
        shootingPoint.rotation = Quaternion.LookRotation(direction);

        UpdatePredictionLine();
    }

    /// <summary>
    /// Updates the prediction line using a simple projectile simulation.
    /// </summary>
private void UpdatePredictionLine()
{
    if (predictionLine == null)
        return;
    
    if (GameStateManager.CurrentState == GameStateManager.GameState.Building){
        predictionLine.enabled = false;
        return;
    }
    predictionLine.enabled = true;

    Vector3 initialPosition = shootingPoint.position;
    Vector3 initialVelocity = shootingPoint.forward * bulletSpeed;
    
    // Fetch the bullet's physics material to get bounciness.
    float bounceFactor = 1f; // Default to 1 (perfect bounce) if none is found.
    Collider bulletCollider = shootingPoint.GetComponent<Collider>();
    if (bulletCollider != null && bulletCollider.sharedMaterial != null)
    {
        bounceFactor = bulletCollider.sharedMaterial.bounciness;
    }
    
    // Prepare the array of trajectory points.
    Vector3[] trajectoryPoints = new Vector3[predictionSegments];
    trajectoryPoints[0] = initialPosition;
    
    Vector3 currentPosition = initialPosition;
    Vector3 currentVelocity = initialVelocity;
    float timeStep = predictionTimeStep;
    
    // Simulate the projectile trajectory.
    for (int i = 1; i < predictionSegments; i++)
    {
        // Predict where we would be after the full timeStep if no collision occurred.
        Vector3 fullStep = currentVelocity * timeStep + 0.5f * Physics.gravity * timeStep * timeStep;
        float fullStepDistance = fullStep.magnitude;
        Vector3 fullStepDirection = (fullStepDistance > 0f) ? fullStep / fullStepDistance : Vector3.zero;

        RaycastHit hit;
        // Check if we hit something along the path.
        if (Physics.Raycast(currentPosition, fullStepDirection, out hit, fullStepDistance, obstacleLayer, QueryTriggerInteraction.Collide))
        {
            // Determine how far into the timeStep the collision occurs.
            float tCollision = hit.distance / fullStepDistance;
            float collisionTime = tCollision * timeStep;
            
            // Calculate the position at collision.
            Vector3 collisionPosition = currentPosition + currentVelocity * collisionTime + 0.5f * Physics.gravity * collisionTime * collisionTime;
            trajectoryPoints[i] = collisionPosition;
            
            // Calculate the velocity at the moment of collision.
            Vector3 velocityAtCollision = currentVelocity + Physics.gravity * collisionTime;
            
            // Reflect the velocity using the hit normal and apply bounce factor.
            Vector3 reflectedVelocity = Vector3.Reflect(velocityAtCollision, hit.normal) * bounceFactor;
            
            // Calculate the remaining time in this time step.
            float remainingTime = timeStep - collisionTime;
            
            // Update velocity with gravity for the remainder.
            currentVelocity = reflectedVelocity + Physics.gravity * remainingTime;
            
            // Update the current position using the reflected velocity over the remaining time.
            currentPosition = collisionPosition + reflectedVelocity * remainingTime + 0.5f * Physics.gravity * remainingTime * remainingTime;
        }
        else
        {
            // No collision: use normal projectile motion.
            Vector3 nextPos = currentPosition + fullStep;
            trajectoryPoints[i] = nextPos;
            currentPosition = nextPos;
            currentVelocity += Physics.gravity * timeStep;
        }
    }
    
    predictionLine.positionCount = predictionSegments;
    predictionLine.SetPositions(trajectoryPoints);
}


    private void OnShoot(InputAction.CallbackContext context)
    {
        if (GameStateManager.CurrentState == GameStateManager.GameState.Building)
            return;

        Shoot();
    }

    private void Shoot()
    {
        if (shootingPoint == null)
        {
            Debug.LogWarning("Shooting Point is not assigned!");
            return;
        }

        GameObject bullet = ObjectPooler.Instance.GetPooledObject(
            bulletPoolTag,
            shootingPoint.position,
            shootingPoint.rotation
        );

        if (bullet == null)
        {
            Debug.LogWarning("No bullet available in the pool.");
            return;
        }

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.linearVelocity = shootingPoint.forward * bulletSpeed;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        Collider bulletCollider = bullet.GetComponent<Collider>();
        Collider playerCollider = GetComponent<Collider>();
        if (bulletCollider != null && playerCollider != null)
        {
            Physics.IgnoreCollision(bulletCollider, playerCollider);
        }
    }
}
