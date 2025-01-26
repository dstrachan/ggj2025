using UnityEngine;
using UnityEngine.InputSystem;

public class BottleController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 0.5f;
    [SerializeField] private float rotateForwardSpeed = 40;
    [SerializeField] private float rotateBackwardSpeed = 50;
    [SerializeField] private Vector2 xBounds = new(-2.2f, -1.2f);
    [SerializeField] private Vector2 zBounds = new(7, 8);
    [SerializeField] private Vector2 rotateBounds = new(85, 130);

    private LiquidContainer _container;
    private InputAction _moveAction;
    private InputAction _rotateAction;

    private void Awake()
    {
        _container = gameObject.GetComponentInChildren<LiquidContainer>();
    }

    private void Start()
    {
        _moveAction = InputSystem.actions.FindAction("Move");
        _rotateAction = InputSystem.actions.FindAction("Rotate");
    }

    private void Update()
    {
        if (!_container.CompareTag("activeBottle")) return;

        var moveValue = _moveAction.ReadValue<Vector2>() * (moveSpeed * Time.deltaTime);
        transform.Translate(moveValue.x, 0, moveValue.y, Space.World);
        var clampedPosition = transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, xBounds.x, xBounds.y);
        clampedPosition.z = Mathf.Clamp(clampedPosition.z, zBounds.x, zBounds.y);
        transform.position = clampedPosition;

        var isPressed = _rotateAction.IsPressed();
        var axis = isPressed ? Vector3.forward : Vector3.back;
        var rotateValue = (isPressed ? rotateForwardSpeed : rotateBackwardSpeed) * Time.deltaTime;
        transform.Rotate(axis, rotateValue, Space.World);

        var angles = transform.eulerAngles;
        angles.z = Mathf.Clamp(transform.eulerAngles.z, rotateBounds.x, rotateBounds.y);
        angles.x = 0;
        angles.y = 0;
        transform.eulerAngles = angles;

        // var clampedRotation = transform.rotation.eulerAngles;
        // clampedRotation.z = Mathf.Clamp(clampedRotation.z, rotateBounds.x, rotateBounds.y);
        // transform.rotation = Quaternion.Euler(clampedRotation);
    }
}