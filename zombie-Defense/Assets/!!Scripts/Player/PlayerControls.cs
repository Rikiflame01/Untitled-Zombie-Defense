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

    private InputSystem_Actions _inputActions;
    private CharacterController _characterController;
    private Vector2 _moveInput;

    private void Awake()
    {
        _inputActions = new InputSystem_Actions();
        _characterController = GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        _inputActions.Enable();
        _inputActions.Player.Move.performed += OnMove;
        _inputActions.Player.Move.canceled += OnMove;
        _inputActions.Player.Shoot.performed += OnShoot;
    }

    private void OnDisable()
    {
        _inputActions.Player.Move.performed -= OnMove;
        _inputActions.Player.Move.canceled -= OnMove;
        _inputActions.Player.Shoot.performed -= OnShoot;
        _inputActions.Disable();
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
    
    private void UpdateShootingPoint()
    {
        if (Camera.main == null || Mouse.current == null) return;

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

        shootingPoint.position = transform.position + direction * shootRadius;
        shootingPoint.rotation = Quaternion.LookRotation(direction);
    }

    private void OnShoot(InputAction.CallbackContext context)
    {
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
