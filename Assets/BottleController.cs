using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class BottleController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 0.5f;
    [SerializeField] private float rotateForwardSpeed = 40;
    [SerializeField] private float rotateBackwardSpeed = 50;
    [SerializeField] private Vector2 xBounds = new(-2.2f, -1.2f);
    [SerializeField] private Vector2 rotateBounds = new(85, 130);
    [SerializeField] private Transform startingRail;
    [SerializeField] private Transform[] rails;
    [SerializeField] private float railTransferTime = 0.1f;

    private LiquidContainer _container;
    private InputAction _moveAction;
    private InputAction _rotateAction;
    private InputAction _moveRailAction;
    private int _currentRailIndex;

    private void Awake()
    {
        _container = gameObject.GetComponentInChildren<LiquidContainer>();
    }

    private void Start()
    {
        _moveAction = InputSystem.actions.FindAction("Move");
        _rotateAction = InputSystem.actions.FindAction("Rotate");
        _moveRailAction = InputSystem.actions.FindAction("MoveRail");

        _currentRailIndex = Array.IndexOf(rails, startingRail);
    }

    private void Update()
    {
        if (!_container.CompareTag("activeBottle")) return;

        // Position
        var moveValue = _moveAction.ReadValue<Vector2>().x * moveSpeed * Time.deltaTime;
        transform.Translate(moveValue, 0, 0, Space.World);

        var clampedPosition = transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, xBounds.x, xBounds.y);
        transform.position = clampedPosition;

        // Rotation
        var isPressed = _rotateAction.IsPressed();
        var axis = isPressed ? Vector3.forward : Vector3.back;
        var rotateValue = (isPressed ? rotateForwardSpeed : rotateBackwardSpeed) * Time.deltaTime;
        transform.Rotate(axis, rotateValue, Space.World);

        var clampedRotation = Vector3.zero;
        clampedRotation.z = Mathf.Clamp(transform.eulerAngles.z, rotateBounds.x, rotateBounds.y);
        transform.eulerAngles = clampedRotation;

        // Rails
        if (_moveRailAction.WasPressedThisFrame())
        {
            StopAllCoroutines();

            var moveRailValue = (int)_moveRailAction.ReadValue<float>();
            _currentRailIndex = Mathf.Clamp(_currentRailIndex - moveRailValue, 0, rails.Length - 1);

            var pos = transform.position;
            pos.z = rails[_currentRailIndex].position.z;
            var coroutine = Move(transform.position, pos, railTransferTime);
            StartCoroutine(coroutine);
        }
    }

    private IEnumerator Move(Vector3 start, Vector3 end, float time)
    {
        var elapsedTime = 0f;
        while (elapsedTime < time)
        {
            transform.position = Vector3.Slerp(start, end, elapsedTime / time);
            elapsedTime += Time.deltaTime;

            yield return null;
        }

        transform.position = end;
        yield return null;
    }
}