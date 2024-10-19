using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float _followSpeedGround = 5f;
    [SerializeField] private float _followSpeedAir = 10f;
    [SerializeField] private Transform _target;
    [SerializeField] private Vector3 _offset;
    [SerializeField] private float _rotationSpeed = 2f;
    [SerializeField] private float _rotationOffset = 0f;
    [SerializeField] private float _maxDistance = 10f;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private float _snapAngle = 90f;

    [Header("Smooth Follow Mode")]
    [SerializeField] private bool _smoothFollowMode = false;
    [SerializeField] private float _rotationDelay = 0.1f;

    [Header("Tilt Settings")]
    [SerializeField] private bool _enableTilt = true;
    [SerializeField] private float _tiltAngle = 10f;

    [Header("Air Control")]
    [SerializeField] private float _airControlRotationSpeed = 100f;
    [SerializeField] private bool _allowAirControl = true;

    [Header("Rear View Settings")]
    [SerializeField] private PlayerInput _inputAction;
    [SerializeField] private float _rearViewRotationSpeed = 5f;
    [SerializeField] private float _offsetPerPress = 180;

    private Vector3 _lastTargetPosition;
    private Quaternion _targetRotation;

    private bool IsTargetGrounded()
    {
        return Physics.Raycast(_target.position, Vector3.down, 1f, _groundLayer);
    }

    private void Awake()
    {
        _inputAction = new PlayerInput();
    }

    private void Start()
    {
        _lastTargetPosition = _target.position;
        _targetRotation = transform.rotation;
    }

    private void OnEnable()
    {
        if (_inputAction != null)
        {
            _inputAction.Player.Enable();
            _inputAction.Player.ToggleRearView.performed += ToggleRearView;
        }
    }

    private void OnDisable()
    {
        if (_inputAction != null)
        {
            _inputAction.Player.ToggleRearView.performed -= ToggleRearView;
            _inputAction.Player.Disable();
        }
    }

    private void ToggleRearView(InputAction.CallbackContext context)
    {
        _rotationOffset += _offsetPerPress;
    }

    private void FixedUpdate()
    {
        if (_target != null)
        {
            Vector3 velocity = (_target.position - _lastTargetPosition) / Time.deltaTime;
            Vector3 desiredPosition = _target.position;
            Vector3 directionToTarget = desiredPosition - _target.position;
            float distanceToTarget = directionToTarget.magnitude;

            if (distanceToTarget > _maxDistance)
            {
                desiredPosition = _target.position + directionToTarget.normalized * _maxDistance;
            }

            float followSpeed = IsTargetGrounded() ? _followSpeedGround : _followSpeedAir;
            transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);


            if (IsTargetGrounded())
            {
                if (_smoothFollowMode)
                {
                    UpdateCameraRotationSmoothFollow(velocity);
                }
                else
                {
                    UpdateCameraRotationGrounded(velocity);
                }
            }
            else
            {
                if (_allowAirControl)
                {
                    ManualAirControl();
                }
                else
                {
                    UpdateCameraRotationInAir(velocity);
                }
            }

            // Maintain the correct offset in all modes
            transform.position = _target.position + transform.rotation * _offset;
            _lastTargetPosition = _target.position;
        }
    }

    private void UpdateCameraRotationRearView()
    {
        // Rotate the camera by 180 degrees around the Y axis relative to the target's forward direction
        Quaternion rearViewRotation = Quaternion.Euler(0, 180f + _rotationOffset, 0);
        Quaternion targetRotation = _target.rotation * rearViewRotation;

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rearViewRotationSpeed * Time.deltaTime);
    }

    private void UpdateCameraRotationGrounded(Vector3 velocity)
    {
        Vector3 forward = _target.forward;
        float targetAngle = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
        float snappedTargetAngle = Mathf.Round(targetAngle / _snapAngle) * _snapAngle;
        snappedTargetAngle += _rotationOffset;
        Quaternion targetRotation = Quaternion.Euler(0, snappedTargetAngle, 0);

        if (_enableTilt)
        {
            float tilt = -CalculateTilt(velocity);
            targetRotation *= Quaternion.Euler(0, 0, tilt);
        }

        _targetRotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
        transform.rotation = _targetRotation;
    }

    private void UpdateCameraRotationSmoothFollow(Vector3 velocity)
    {
        Quaternion targetRotation = Quaternion.LookRotation(_target.forward);
        targetRotation *= Quaternion.Euler(0, _rotationOffset, 0);

        if (_enableTilt)
        {
            float tilt = CalculateTilt(velocity);
            targetRotation *= Quaternion.Euler(0, 0, tilt);
        }

        _targetRotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationDelay);
        transform.rotation = Quaternion.Slerp(transform.rotation, _targetRotation, _rotationSpeed * Time.deltaTime);
    }

    private float CalculateTilt(Vector3 velocity)
    {
        if (Mathf.Abs(velocity.y) > 0.5f)
        {
            return Mathf.Clamp(-velocity.x * _tiltAngle, -_tiltAngle, _tiltAngle);
        }
        return 0f;
    }

    private void UpdateCameraRotationInAir(Vector3 velocity)
    {
        if (velocity.magnitude > 0.1f)
        {
            velocity.y = 0;
            if (velocity != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(velocity, Vector3.up);
                targetRotation *= Quaternion.Euler(0, _rotationOffset, 0);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
            }
        }
    }

    private void ManualAirControl()
    {
        float horizontal = Input.GetAxis("Mouse X") * _airControlRotationSpeed * Time.deltaTime;
        float vertical = -Input.GetAxis("Mouse Y") * _airControlRotationSpeed * Time.deltaTime;

        transform.RotateAround(_target.position, Vector3.up, horizontal);
        transform.RotateAround(_target.position, transform.right, vertical);
    }
}
