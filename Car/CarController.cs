using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CarController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody _carRb;
    [SerializeField] private Transform[] _wheels;
    [SerializeField] private LayerMask _drivable;
    [SerializeField] private Transform _accelerationPoint;

    [Header("Suspension Settings")]
    [SerializeField] private float _springStrength, _dampeningStrength;
    [SerializeField] private float _restLength;
    [SerializeField] private float _springMaxTravel;
    [SerializeField] private float _wheelRadius;

    [Header("Input")]
    private PlayerInput _playerInput;
    private float _moveInput, _steerInput;

    [Header("Movement")]
    [SerializeField] private float _normalAcceleration = 25f;
    private float _currentAcceleration;
    [SerializeField] private float _maxSpeed = 100f;
    [SerializeField] private float _friction = 10f;

    private Vector3 _carLocalVelocity = Vector3.zero;
    private float _carVelocityRatio = 0;

    private int[] _wheelIsGrounded = new int[4];
    [SerializeField] private bool _isGrounded;
    [Tooltip("Angle At which it's not grounded anymore, despite touching ground")]
    [Range(0, 180)]
    [SerializeField] private int _groundedAngle = 60;

    [Header("Steering")]
    [SerializeField] private float _steerStrength = 15f;
    [SerializeField] private AnimationCurve _turningCurve;
    [SerializeField] private float _dragCoefficient = 1f;
    private float groundedAngularDrag;

    [Header("Air Control")]
    [SerializeField] private float _airTiltStrength = 10f;
    [SerializeField] private float _airRotationStrength = 5f;
    [SerializeField] private float _airAngularDrag = .1f;
    [SerializeField] private bool _mouseBasedRotation;

    private Vector2 _screenCenter;
    private Vector3 _currentAngularVelocity;
    private Vector2 _mouseDelta;
    public float maxRotationSpeed = 5f;
    public float deadzoneRadius = 50f;
    [SerializeField] private bool _isMouseLocked = true;


    [Header("Rocket Boost")]
    [SerializeField] private float _boostMultiplier;
    [SerializeField] private float _boostMaxDuration = 5f;
    [SerializeField] private float _boostRefillRate = 1f;
    [SerializeField] private float _boostDrainMultiplier = 1f;
    private float _currentBoostDuration;
    private bool _isBoosting;
    private bool _isBoostFull = false;

    [Header("Rocket Boost - UI")]
    [SerializeField] private Image _boostFillImage;
    [SerializeField] private RectTransform _boostUIRectTransform;
    [SerializeField] private Vector2 _animationOffset;
    private Vector2 offScreenPosition;
    private Vector2 onScreenPosition;
    [SerializeField] private float _slideDuration = 0.5f;
    private Coroutine _slideCoroutine;
    private float _refillTimer = 0f;
    [SerializeField] private float _refillDelay = 1f;

    [Header("Rocket Boost - Effects")]
    [SerializeField] private float _boostFOV = 60f;
    [SerializeField] private Camera _camera;
    [SerializeField] private float _zoomSpeed = 2f;
    [SerializeField] private GameObject _speedlines;
    [SerializeField] private GameObject _boostExhaustEffect;

    [Header("FOV Zoom Settings")]
    [SerializeField] private float _maxZoomOutFOV = 70f; // Maximum FOV when car is at max speed
    [SerializeField] private float _fovZoomSpeed = 2f; // Speed at which FOV changes

    [Header("Speed-Based Effects")]
    [SerializeField] private float _speedThreshold = 50f;  // Speed at which boost effect kicks in


    private float _normalFOV;
    private Coroutine _zoomCoroutine;


    [Header("Self-Correction")]
    [SerializeField] private float _correctionDuration = 1f;
    [SerializeField] private float _flipCorrectionDelay = 1f;
    private float elapsedTime;
    private Quaternion startingRotation;
    private Coroutine _correctionCoroutine;

    private float _flipTimeElapsed = 0f;
    private GameObject _lastCollisionObject;

    [Header("Jumping")]
    [SerializeField] private float _jumpForce;


    public bool _airControlIsActive, _boosterIsActive = true, _jumpIsActive;
    private bool _airControlWasActive;

    [SerializeField] private bool _isDecoration;


    [SerializeField] private CameraController _cameraController;

    [Header("Wheel Animation")]
    [SerializeField] private Transform[] _wheelMeshes; // Front and rear wheels
    [SerializeField] private float _wheelRotationSpeed = 360f; // Speed at which wheels rotate based on movement
    [SerializeField] private float _maxSteerAngle = 90f;
    [SerializeField] private float _steerSpeed = 90f;



    private void Start()
    {
        // Calculate screen center once at the start
        _screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);

    }
    private void Awake()
    {
        if (!_isDecoration)
        {
            _playerInput = new PlayerInput();
            _carRb = GetComponent<Rigidbody>();

            _currentAcceleration = _normalAcceleration;

            _normalFOV = _camera.fieldOfView;

            _currentBoostDuration = _boostMaxDuration;
            _boostFillImage.fillAmount = 1f;
            onScreenPosition = _boostUIRectTransform.anchoredPosition;
            offScreenPosition = onScreenPosition + _animationOffset;
            _boostUIRectTransform.anchoredPosition = offScreenPosition;

            startingRotation = transform.rotation;
            groundedAngularDrag = _carRb.angularDrag;
        }
    }

    bool CanUseEverything()
    {
        if (_isDecoration || Time.timeScale == 0)
            return false;
        else if (!_isDecoration && Time.timeScale != 0)
        {
            return true;
        }
        else return false;
    }

    public void OnEnable()
    {
        if (_playerInput != null)
        {
            _playerInput.Player.Enable();

            _playerInput.Player.Gas.performed += AddGas;
            _playerInput.Player.Gas.canceled -= StopGas;

            _playerInput.Player.Jump.performed += Jump;

            _playerInput.Player.Boost.performed += TurnOnBoost;
            _playerInput.Player.Boost.canceled += TurnOffBoost;
        }
    }

    public void OnDisable()
    {
        if (_playerInput != null)
        {
            _playerInput.Player.Gas.performed -= AddGas;
            _playerInput.Player.Gas.canceled -= StopGas;

            _playerInput.Player.Jump.performed -= Jump;

            _playerInput.Player.Boost.performed -= TurnOnBoost;
            _playerInput.Player.Boost.canceled -= TurnOffBoost;

            _playerInput.Player.Disable();
        }
    }


    private void AddGas(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>().y;
        _steerInput = context.ReadValue<Vector2>().x;
    }

    private void StopGas(InputAction.CallbackContext context)
    {
        _moveInput = 0;
        _steerInput = 0;
    }
    private void TurnOnBoost(InputAction.CallbackContext context)
    {
        if (_boosterIsActive)
        {
            _isBoosting = true;
        }
    }

    private void TurnOffBoost(InputAction.CallbackContext context)
    {
        _isBoosting = false;
    }
    private void Jump(InputAction.CallbackContext context)
    {
        if (_jumpIsActive)
            if (_isGrounded)
                _carRb.AddForce(transform.up * _jumpForce, ForceMode.Impulse);
    }

    private void FixedUpdate()
    {
        Suspension();
        if (CanUseEverything())
        {
            GroundCheck();
            CalculateCarVelocity();
            Move();
        }
    }

    private void Update()
    {
        if (CanUseEverything())
        {
            var boostedSpeed = _normalAcceleration * _boostMultiplier;
            _currentAcceleration = _isBoosting ? boostedSpeed : _normalAcceleration;

            HandleBoosting();
            HandleRefill();

            _boostFillImage.fillAmount = _currentBoostDuration / _boostMaxDuration;
            HandleBoostingEffects();

            HandleSelfCorrection();
        }
    }
    private void HandleBoostingEffects()
    {
        float targetFOV;

        // Get the car's forward speed in local space
        float currentSpeed = Mathf.Abs(_carLocalVelocity.z);

        // Speedlines effect triggered based on speed
        if (currentSpeed >= _speedThreshold)
        {
            _speedlines.SetActive(true);   // Activate speedlines effect when above speed threshold
        }
        else
        {
            _speedlines.SetActive(false);  // Deactivate speedlines effect when below speed threshold
        }

        // Check if boosting is active
        if (_isBoosting)
        {
            // Activate the effect if the car is moving forward
            if (Mathf.Abs(_carLocalVelocity.z) > 0.1f) // Small threshold to account for minor movements
            {
                targetFOV = _boostFOV;
                _boostExhaustEffect.SetActive(true);
            }
            else
            {
                targetFOV = _normalFOV;
            }
        }
        else
        {
            // Calculate target FOV based on speed during normal driving
            float speedRatio = Mathf.Abs(_carLocalVelocity.z) / _maxSpeed;
            targetFOV = Mathf.Lerp(_normalFOV, _maxZoomOutFOV, speedRatio);

            _boostExhaustEffect.SetActive(false);
        }

        // Smoothly zoom the camera to the desired FOV
        if (_camera.fieldOfView != targetFOV)
        {
            StartZoom(targetFOV);
        }
    }



    private void HandleSelfCorrection()
    {
        if (_correctionCoroutine == null)
        {
            if (Input.GetKeyDown(KeyCode.K))
            {
                _correctionCoroutine = StartCoroutine(SelfCorrectCoroutine(false));
            }
            // Check if the car is flipped and colliding with the same object while not grounded
            if (_lastCollisionObject != null && !_isGrounded)
            {
                _flipTimeElapsed += Time.deltaTime;

                if (_flipTimeElapsed >= _flipCorrectionDelay)
                {
                    if (_correctionCoroutine == null)
                    {
                        _correctionCoroutine = StartCoroutine(SelfCorrectCoroutine(false));
                    }
                    _flipTimeElapsed = 0f; // Reset flip timer after correction
                }
            }
            else
            {
                _flipTimeElapsed = 0f; // Reset the timer if the car is not flipped or not colliding with the same object
            }
        }
    }


    #region Boosting
    private void HandleBoosting()
    {
        if (_isBoosting)
        {
            if (_currentBoostDuration > 0)
            {
                _currentBoostDuration -= Time.deltaTime * _boostDrainMultiplier;
                if (_slideCoroutine == null)
                {
                    _slideCoroutine = StartCoroutine(SlideUI(onScreenPosition));
                }
            }
            else
            {
                _isBoosting = false;
            }
        }
    }

    private void HandleRefill()
    {
        if (!_isBoosting)
        {
            _refillTimer += Time.deltaTime;
            if (_refillTimer >= _refillDelay)
            {
                _currentBoostDuration += Time.deltaTime * _boostRefillRate;
                if (_currentBoostDuration >= _boostMaxDuration)
                {
                    _currentBoostDuration = _boostMaxDuration;
                    if (!_isBoostFull)
                    {
                        _isBoostFull = true;
                        if (_slideCoroutine == null)
                        {
                            _slideCoroutine = StartCoroutine(SlideUIOutAfterDelay(1f));
                        }
                    }
                }
            }
        }
        else
        {
            _refillTimer = 0f;
        }
    }

    private IEnumerator SlideUI(Vector2 targetPosition)
    {
        Vector2 startPosition = _boostUIRectTransform.anchoredPosition;
        float elapsedTime = 0f;

        while (elapsedTime < _slideDuration)
        {
            _boostUIRectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, elapsedTime / _slideDuration);
            elapsedTime += Time.deltaTime;
            _slideCoroutine = null;
            yield return null;
        }
        _boostUIRectTransform.anchoredPosition = targetPosition;

        _slideCoroutine = null;
    }

    private IEnumerator SlideUIOutAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartCoroutine(SlideUI(offScreenPosition));
        _isBoostFull = false;
    }
    #endregion

    #region ZoomEffect
    private void StartZoom(float targetFOV)
    {
        if (_zoomCoroutine != null)
        {
            StopCoroutine(_zoomCoroutine);
        }

        _zoomCoroutine = StartCoroutine(ZoomCamera(targetFOV));
    }

    private IEnumerator ZoomCamera(float targetFOV)
    {
        float startFOV = _camera.fieldOfView;
        float elapsedTime = 0f;

        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime * _zoomSpeed;
            _camera.fieldOfView = Mathf.Lerp(startFOV, targetFOV, elapsedTime);
            yield return null;
        }

        _camera.fieldOfView = targetFOV;
    }
    #endregion

    void CalculateCarVelocity()
    {
        _carLocalVelocity = transform.InverseTransformDirection(_carRb.velocity);
        _carVelocityRatio = _carLocalVelocity.z / _maxSpeed;
    }

    private void Move()
    {
        if (_isGrounded)
        {
            SetAngularDrag(groundedAngularDrag);

            Acceleration();
            MovementFriction();
            Steering();
            SteeringFriction();
        }
        else
        {
            SetAngularDrag(_airAngularDrag);

            if (_airControlIsActive)
            {
                KeysAirControl();
            }else if (_airControlWasActive)
            {
                _airControlWasActive = false;

                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
    private void SetAngularDrag(float newDrag)
    {
        _carRb.angularDrag = newDrag;
    }

    #region Control
    void Acceleration()
    {
        if (Mathf.Abs(_carLocalVelocity.z) < _maxSpeed)
        {
            _carRb.AddForceAtPosition(_currentAcceleration * _moveInput * transform.forward, _accelerationPoint.position, ForceMode.Acceleration);

            // Rotate the wheels for forward/backward movement
            foreach (Transform wheel in _wheelMeshes)
            {
                wheel.Rotate(Vector3.right, -_wheelRotationSpeed * _carLocalVelocity.z * Time.deltaTime);
            }
        }
    }


    void MovementFriction()
    {
        _carRb.AddForceAtPosition(_friction * _carVelocityRatio * -transform.forward, _accelerationPoint.position, ForceMode.Acceleration);
    }

    void Steering()
    {
        _carRb.AddRelativeTorque(_steerStrength * _steerInput * _turningCurve.Evaluate(Mathf.Abs(_carVelocityRatio)) * Mathf.Sign(_carVelocityRatio) * _carRb.transform.up, ForceMode.Acceleration);

        // Calculate the target steering angle
        float steeringAngle = _steerInput * _maxSteerAngle;

        // Smoothly rotate front wheels for steering
        Quaternion targetSteeringRotation = Quaternion.Euler(_wheelMeshes[2].localRotation.x, 0, steeringAngle);

        // Maintain forward rotation and combine it with steering
        _wheelMeshes[2].localRotation = Quaternion.Lerp(_wheelMeshes[2].localRotation, targetSteeringRotation, Time.deltaTime * _steerSpeed);
        _wheelMeshes[3].localRotation = Quaternion.Lerp(_wheelMeshes[3].localRotation, targetSteeringRotation, Time.deltaTime * _steerSpeed);
    }




    void SteeringFriction()
    {
        float currentSidewaysSpeed = _carLocalVelocity.x;
        float dragMagnitude = -currentSidewaysSpeed * _dragCoefficient;
        Vector3 dragForce = transform.right * dragMagnitude;
        _carRb.AddForceAtPosition(dragForce, _carRb.worldCenterOfMass, ForceMode.Acceleration);
    }

    private void KeysAirControl()
    {
        if (_isBoosting)
        {
            if (Mathf.Abs(_carLocalVelocity.z) < _maxSpeed)
            {
                _carRb.AddForce(transform.forward * _currentAcceleration, ForceMode.Acceleration);
            }
        }

        float tilt = -_moveInput * _airTiltStrength;
        _carRb.AddTorque(tilt * transform.right, ForceMode.Acceleration);

        float rotation = _steerInput * _airRotationStrength;
        _carRb.AddTorque(rotation * transform.up, ForceMode.Acceleration);
    }
    /*private void MouseAirControl()
    {
        if (_isBoosting)
        {
            if (Mathf.Abs(_carLocalVelocity.z) < _maxSpeed)
            {
                _carRb.AddForce(transform.forward * _currentAcceleration, ForceMode.Acceleration);
            }
        }

        if (!_airControlWasActive)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            _airControlWasActive = true;
        }

        Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);

        // Calculate the offset of the mouse from the screen center
        Vector2 mouseOffset = _mouseDelta;
        float distanceFromCenter = mouseOffset.magnitude;

        // Check if the mouse is outside the deadzone
        if (distanceFromCenter > deadzoneRadius)
        {
            // Normalize the mouse offset to get direction
            Vector2 normalizedOffset = mouseOffset.normalized;

            // Calculate rotation speed based on the distance from the center
            float rotationSpeedFactor = (distanceFromCenter - deadzoneRadius) / (screenCenter.magnitude - deadzoneRadius);
            rotationSpeedFactor = Mathf.Clamp01(rotationSpeedFactor); // Clamp to [0, 1]

            // Calculate the world space rotation based on the mouse movement
            Vector3 targetAngularVelocity = new Vector3(
                -_mouseDelta.y * maxRotationSpeed * rotationSpeedFactor, // Pitch (up/down)
                _mouseDelta.x * maxRotationSpeed * rotationSpeedFactor,  // Yaw (left/right)
                0f
            );

            // Convert the target angular velocity to local space relative to the car's current rotation
            Vector3 localTargetAngularVelocity = transform.TransformDirection(targetAngularVelocity);

            // Smoothly adjust the current angular velocity towards the target velocity
            _currentAngularVelocity = Vector3.Lerp(_currentAngularVelocity, localTargetAngularVelocity, Time.fixedDeltaTime * 5f);

            // Apply the angular velocity to the car's Rigidbody
            _carRb.angularVelocity = _currentAngularVelocity;
        }
        else
        {
            // If inside the deadzone, gradually reduce rotation
            _currentAngularVelocity = Vector3.Lerp(_currentAngularVelocity, Vector3.zero, Time.fixedDeltaTime * 5f);
            _carRb.angularVelocity = _currentAngularVelocity;
        }
    }*/


    #endregion

    #region Self-Correction
    private IEnumerator SelfCorrectCoroutine(bool useDelay)
    {
        if (useDelay)
        {
            float elapsedTime = 0f;

            while (elapsedTime < _flipCorrectionDelay)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }

        if (_isGrounded)
            yield break;

        _cameraController.enabled = false;

        Quaternion targetRotation = Quaternion.Euler(startingRotation.x, transform.rotation.eulerAngles.y, startingRotation.z);
        Vector3 targetPosition = transform.position + new Vector3(0, 1, 0);

        float correctionDuration = _correctionDuration; // Duration of the correction
        elapsedTime = 0f;

        Quaternion initialRotation = transform.rotation;
        Vector3 initialPosition = transform.position;

        while (elapsedTime < correctionDuration)
        {
            transform.rotation = Quaternion.Slerp(initialRotation, targetRotation, elapsedTime / correctionDuration);
            transform.position = Vector3.Lerp(initialPosition, targetPosition, elapsedTime / correctionDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.rotation = targetRotation;
        transform.position = targetPosition;

        _cameraController.enabled = true;
        _correctionCoroutine = null;
    }



    #endregion

    void Suspension()
    {
        int i = 0;
        foreach (var wheel in _wheels)
        {
            RaycastHit hit;
            float maxExtension = _restLength + _springMaxTravel;

            if (Physics.Raycast(wheel.position, -wheel.up, out hit, maxExtension + _wheelRadius, _drivable))
            {
                _wheelIsGrounded[i] = 1;

                float currentSpringLength = hit.distance - _wheelRadius;
                float springCompression = (_restLength - currentSpringLength) / _springMaxTravel;

                float springVelocity = Vector3.Dot(_carRb.GetPointVelocity(wheel.position), wheel.up);

                float dampForce = _dampeningStrength * springVelocity;
                float bruteSpringForce = _springStrength * springCompression;
                float netspringForce = bruteSpringForce - dampForce;

                _carRb.AddForceAtPosition(wheel.up * netspringForce, wheel.position);

                Debug.DrawLine(wheel.position, hit.point, Color.white);
            }
            else
            {
                _wheelIsGrounded[i] = 0;
                Debug.DrawLine(wheel.position, wheel.position + (_wheelRadius + maxExtension) * -wheel.up, Color.black);
            }

            i++;
        }
    }

    void GroundCheck()
    {
        // Normalize the Z angle to the range -180 to 180 degrees
        var angle = transform.localEulerAngles.z;
        if (angle > 180f) angle -= 360f;

        // Check if the absolute value of the angle exceeds the allowed grounded angle
        if (Mathf.Abs(angle) <= _groundedAngle)
        {
            // Define a raycast from the car's center downward
            RaycastHit hit;
            float checkDistance = 1.5f; // Distance to check if grounded

            // Perform the raycast
            if (Physics.Raycast(transform.position, -transform.up, out hit, checkDistance, _drivable))
            {
                _isGrounded = true;
            }
            else
            {
                _isGrounded = false;
            }
        }
        else
        {
            _isGrounded = false;
        }

        if (_isGrounded && _steerInput == 0)
        {
            _wheelMeshes[0].localRotation = Quaternion.identity; // Front-left wheel reset
            _wheelMeshes[1].localRotation = Quaternion.identity; // Front-right wheel reset
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Start tracking the object we're colliding with
        _lastCollisionObject = collision.gameObject;

        if (collision.gameObject.TryGetComponent<Health>(out var health))
        {
            health.Damage(1);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        // Stop tracking if we stop colliding with the object
        if (collision.gameObject == _lastCollisionObject)
        {
            _lastCollisionObject = null;
            _flipTimeElapsed = 0f; // Reset the timer
        }
    }



    /// Sound
    public float GetCarVelocityRatio()
    {
        return _carVelocityRatio;
    }

    public float GetMoveInput()
    {
        return _moveInput;
    }

    public float GetSteerInput()
    {
        return _steerInput;
    }

    public bool IsBoosting()
    {
        return _isBoosting;
    }

    public void ChangeMaxBoostTime(float maxBoostTime)
    {
        _boostMaxDuration = maxBoostTime;
        _currentBoostDuration = maxBoostTime;
    }

    public bool CarIsGrounded()
    {
        return _isGrounded;
    }
}