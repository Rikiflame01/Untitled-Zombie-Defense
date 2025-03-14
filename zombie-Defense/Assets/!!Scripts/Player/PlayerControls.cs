using System.Collections;
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

    [Header("Render Texture Settings")]
    [SerializeField] private Camera renderTextureCamera;
    [SerializeField] private RenderTexture renderTexture;

    [Header("Rifle Settings")]
    public bool rifleEnabled = false; 
    [SerializeField] private float rifleFireRate = 0.1f;  
    private float nextRifleFireTime = 0f;

    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private GameObject muzzleFlash;

    private InputSystem_Actions _inputActions;
    private CharacterController _characterController;
    private Vector2 _moveInput;

    public EntityStats playerStats;
    [SerializeField] private GameObject bloodBurstPrefab;

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

        if (rifleEnabled && Mouse.current != null && Mouse.current.leftButton.isPressed && Time.time >= nextRifleFireTime)
        {
            nextRifleFireTime = Time.time + rifleFireRate;
            AttemptShoot();
        }
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
        if (renderTextureCamera == null || Mouse.current == null || renderTexture == null)
            return;

        Vector3 direction;
        Vector2 mousePos = Mouse.current.position.ReadValue();

        float scaleX = (float)renderTexture.width / Screen.width;
        float scaleY = (float)renderTexture.height / Screen.height;
        Vector2 renderMousePos = new Vector2(mousePos.x * scaleX, mousePos.y * scaleY);

        Ray ray = renderTextureCamera.ScreenPointToRay(new Vector3(renderMousePos.x, renderMousePos.y, 0));
        RaycastHit hit;
        int groundLayerMask = LayerMask.GetMask("Ground");

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayerMask))
        {
            direction = (hit.point - transform.position).normalized;
        }
        else
        {
            Vector3 screenPoint = new Vector3(renderMousePos.x, renderMousePos.y, 10f);
            Vector3 worldPoint = renderTextureCamera.ScreenToWorldPoint(screenPoint);
            direction = (worldPoint - transform.position).normalized;
        }

        direction.y = 0f;

        Vector3 basePosition = transform.position + Vector3.up * shootHeightOffset;
        shootingPoint.position = basePosition + direction * shootRadius;
        shootingPoint.rotation = Quaternion.LookRotation(direction);

        UpdatePredictionLine();
    }

    private void UpdatePredictionLine()
    {
        if (predictionLine == null)
            return;
        
        if (GameStateManager.CurrentState == GameStateManager.GameState.Building || 
            GameStateManager.CurrentState == GameStateManager.GameState.ChooseCard|| 
            GameStateManager.CurrentState == GameStateManager.GameState.MainMenu)
        {
            predictionLine.enabled = false;
            return;
        }
        predictionLine.enabled = true;

        Vector3 initialPosition = shootingPoint.position;
        Vector3 initialVelocity = shootingPoint.forward * bulletSpeed;
        
        float bounceFactor = 1f;
        Collider bulletCollider = shootingPoint.GetComponent<Collider>();
        if (bulletCollider != null && bulletCollider.sharedMaterial != null)
        {
            bounceFactor = bulletCollider.sharedMaterial.bounciness;
        }
        
        Vector3[] trajectoryPoints = new Vector3[predictionSegments];
        trajectoryPoints[0] = initialPosition;
        
        Vector3 currentPosition = initialPosition;
        Vector3 currentVelocity = initialVelocity;
        float timeStep = predictionTimeStep;
        
        for (int i = 1; i < predictionSegments; i++)
        {
            Vector3 fullStep = currentVelocity * timeStep + 0.5f * Physics.gravity * timeStep * timeStep;
            float fullStepDistance = fullStep.magnitude;
            Vector3 fullStepDirection = (fullStepDistance > 0f) ? fullStep / fullStepDistance : Vector3.zero;

            RaycastHit hit;
            if (Physics.Raycast(currentPosition, fullStepDirection, out hit, fullStepDistance, obstacleLayer, QueryTriggerInteraction.Collide))
            {
                float tCollision = hit.distance / fullStepDistance;
                float collisionTime = tCollision * timeStep;
                
                Vector3 collisionPosition = currentPosition + currentVelocity * collisionTime + 0.5f * Physics.gravity * collisionTime * collisionTime;
                trajectoryPoints[i] = collisionPosition;
                
                Vector3 velocityAtCollision = currentVelocity + Physics.gravity * collisionTime;
                
                Vector3 reflectedVelocity = Vector3.Reflect(velocityAtCollision, hit.normal) * bounceFactor;
                
                float remainingTime = timeStep - collisionTime;
                
                currentVelocity = reflectedVelocity + Physics.gravity * remainingTime;
                
                currentPosition = collisionPosition + reflectedVelocity * remainingTime + 0.5f * Physics.gravity * remainingTime * remainingTime;
            }
            else
            {
                Vector3 nextPos = currentPosition + fullStep;
                trajectoryPoints[i] = nextPos;
                currentPosition = nextPos;
                currentVelocity += Physics.gravity * timeStep;
            }
        }
        
        predictionLine.positionCount = predictionSegments;
        predictionLine.SetPositions(trajectoryPoints);
    }

    private void AttemptShoot()
    {
        if (GameStateManager.CurrentState == GameStateManager.GameState.Building || 
            GameStateManager.CurrentState == GameStateManager.GameState.ChooseCard|| 
            GameStateManager.CurrentState == GameStateManager.GameState.MainMenu)
            return;

        PlayerReload reload = GetComponent<PlayerReload>();
        if (reload != null)
        {
            if (!reload.TryShoot())
            {
                SoundManager.Instance.PlaySFX("trigger", 2f);
                return;
            }
        }
        
        Shoot();
    }

    private void OnShoot(InputAction.CallbackContext context)
    {
        AttemptShoot();
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

        SoundManager.Instance.PlaySFX("shoot", 1.2f);

        if (muzzleFlash != null)
        {
            ParticleSystem ps = muzzleFlash.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Play();
            }

            Light flashLight = muzzleFlash.GetComponent<Light>();
            if (flashLight != null)
            {
                flashLight.enabled = true;
                StartCoroutine(DisableLightAfterDelay(flashLight, 0.15f));
            }
        }

        if (playerStats.isPiercing)
        {
            Debug.Log("Piercing shot!");
            StartCoroutine(DoSecondaryPiercingDamage(shootingPoint.position, shootingPoint.forward));
        }
        else if (GameManager.Instance == null){
            Debug.Log("Game Manager is null");
        }

    }

    private IEnumerator DisableLightAfterDelay(Light light, float delay)
    {
        yield return new WaitForSeconds(delay);
        light.enabled = false;
    }
    
private IEnumerator DoSecondaryPiercingDamage(Vector3 startPosition, Vector3 direction)
{
    yield return new WaitForSeconds(0.1f);
    int enemyLayerMask = LayerMask.GetMask("Enemy");
    float maxDistance = 40f;
    Ray ray = new Ray(startPosition, direction);
    
    RaycastHit[] hits = Physics.RaycastAll(ray, maxDistance, enemyLayerMask);
    if (hits != null && hits.Length > 0)
    {
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        
        int damage = playerStats.damage;
        
        foreach (RaycastHit hit in hits)
        {
            Health health = hit.collider.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(damage);
                SoundManager.Instance.PlaySFX("bulletHit", 0.7f);

                if (health.currentHealth <= 0)
                {
                    Vector3 spawnPosition = hit.point;
                    GameObject playerObj = GameObject.FindWithTag("Player");
                    if (playerObj != null)
                    {
                        Vector3 directionAwayFromPlayer = (spawnPosition - playerObj.transform.position).normalized;
                        Quaternion spawnRotation = Quaternion.LookRotation(directionAwayFromPlayer);

                        Instantiate(bloodBurstPrefab, spawnPosition, spawnRotation);
                    }
                }
            }
        }
    }
}

}
