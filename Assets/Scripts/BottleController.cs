using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class BottleController : MonoBehaviour
{
    public Transform bottleToControl;

    [SerializeField] private float rotateForwardSpeed = 40;
    [SerializeField] private float rotateBackwardSpeed = 50;
    [SerializeField] private Vector2 xBounds = new(-2.2f, -1.2f);
    [SerializeField] private Vector2 rotateBounds = new(85, 130);
    [SerializeField] private Transform startingRail;
    [SerializeField] private Transform[] rails;
    [SerializeField] private float railTransferTime = 0.1f;
    [SerializeField] private float speed = 0.3f;

    private LiquidContainer _container;
    private InputAction _moveAction;
    private InputAction _rotateAction;
    private InputAction _moveRailAction;
    private int _currentRailIndex;

    // Starts with one in scene
    public int bottlesUsed = 1;

    private void Awake()
    {
        _container = bottleToControl.GetComponentInChildren<LiquidContainer>();
    }

    public void SetActiveBottle(Transform newBottle)
    {
        bottleToControl = newBottle;
        _container = bottleToControl.GetComponentInChildren<LiquidContainer>();
        bottlesUsed++;
    }

    private void Start()
    {

        // Locks the cursor
        Cursor.lockState = CursorLockMode.Locked;

        _moveAction = InputSystem.actions.FindAction("Move");
        _rotateAction = InputSystem.actions.FindAction("Rotate");
        _moveRailAction = InputSystem.actions.FindAction("MoveRail");

        _currentRailIndex = Array.IndexOf(rails, startingRail);

        SetBottleRail(0, railTransferTime);
    }

    private void Update()
    {
        if (!_container || !_container.CompareTag("activeBottle")) return;

        // Position
        var moveValue = _moveAction.ReadValue<float>() * Time.deltaTime * speed;
        bottleToControl.transform.Translate(moveValue, 0, 0, Space.World);

        var clampedPosition = bottleToControl.transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, xBounds.x, xBounds.y);
        bottleToControl.transform.position = clampedPosition;

        // Rotation
        var isPressed = _rotateAction.IsPressed();
        var axis = isPressed ? Vector3.forward : Vector3.back;
        var rotateValue = (isPressed ? rotateForwardSpeed : rotateBackwardSpeed) * Time.deltaTime;
        bottleToControl.transform.Rotate(axis, rotateValue, Space.World);

        var clampedRotation = Vector3.zero;
        clampedRotation.z = Mathf.Clamp(bottleToControl.transform.eulerAngles.z, rotateBounds.x, rotateBounds.y);
        bottleToControl.transform.eulerAngles = clampedRotation;

        // Rails
        if (_moveRailAction.WasPressedThisFrame())
        {
            SetBottleRail((int)_moveRailAction.ReadValue<float>(), railTransferTime);
        }
    }

    public void SetBottleRail(int railValue, float timeToLerp)
    {
        StopAllCoroutines();

        _currentRailIndex = Mathf.Clamp(_currentRailIndex - railValue, 0, rails.Length - 1);

        var pos = bottleToControl.transform.position;
        pos.z = rails[_currentRailIndex].position.z;
        var coroutine = Move(bottleToControl.transform.position, pos, timeToLerp);
        StartCoroutine(coroutine);
    }

    private IEnumerator Move(Vector3 start, Vector3 end, float time)
    {
        var elapsedTime = 0f;
        while (elapsedTime < time)
        {
            bottleToControl.transform.position = Vector3.Slerp(start, end, elapsedTime / time);
            elapsedTime += Time.deltaTime;

            yield return null;
        }

        bottleToControl.transform.position = end;
        yield return null;
    }
}