using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerControls : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

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
    }

    private void OnDisable()
    {
        _inputActions.Player.Move.performed -= OnMove;
        _inputActions.Player.Move.canceled -= OnMove;

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
    }
}
