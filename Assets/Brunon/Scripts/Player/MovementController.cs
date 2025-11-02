using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

public class MovementController : MonoBehaviour
{
    [Header("Jump Settings")]
    [SerializeField] private float _maxChargeTime = 2f;
    [SerializeField] private float _maxForce = 20f;
    [SerializeField] private float _minForce = 5f;
    [SerializeField] private float _verticalBoost = 2f;
    [SerializeField] private float _jumpCooldown = 0.3f;
    [SerializeField] private float _cameraDropAmount = 0.3f;
    [SerializeField] private float _cameraLerpSpeed = 5f;
    [SerializeField] private AnimationCurve _chargeCurve;
    [SerializeField] private Transform _headTransform;

    [Header("Crouch Walk Settings")]
    [SerializeField] private float _crouchSpeed = 3f; // velocidad lenta
    [SerializeField] private float _moveSmooth = 8f;
    private bool _wantsToCrouch;

    [Header("Air Control")]
    [SerializeField] private float _airSteerStrength = 2.5f; // Fuerza del timón en el aire
    [SerializeField] private float _maxAirSpeed = 15f; // Velocidad horizontal máxima en el aire

    private Rigidbody _rb;
    private Camera _playerCam;
    private FloorDetector _floorDetector;
    private GrappleBruno _grapple;

    private Vector3 _headDefaultLocalPos;
    private Vector3 _lastInputDir = Vector3.zero;
    private Vector3 _moveInput;

    private float _chargeTimer = 0f;
    private bool _isCharging = false;
    private bool _isCrouching = false;
    private float _baseCrouchSpeed;
    private float _baseMaxForce;

    private float _lastJumpTime;

    private void Start()
    {
        if (_headTransform != null)
            _headDefaultLocalPos = _headTransform.localPosition;
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _grapple = GetComponent<GrappleBruno>();
        _playerCam = GetComponentInChildren<Camera>();
        _floorDetector = GetComponentInChildren<FloorDetector>();
        _baseCrouchSpeed = _crouchSpeed;
        _baseMaxForce = _maxForce;
    }
    private void FixedUpdate()
    {
        // El control aéreo debe ir en FixedUpdate porque aplica fuerzas
        HandleCrouching();
        HandleAirSteering();
    }
    private void Update()
    {
        HandleInputReading();
        HandleCameraChargeEffect();
    }

    private void HandleInputReading()
    {
        // Inputs básicos
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        _moveInput = new Vector3(h, 0f, v).normalized;
        _wantsToCrouch = Input.GetKey(KeyCode.LeftShift);

        bool groundedOrWall = _floorDetector.IsGrounded || (_grapple != null && _grapple.IsStuckToWall);
        bool canJump = groundedOrWall && Time.time > _lastJumpTime + _jumpCooldown;

        // Lógica de Crouch (agacharse)
        if (_wantsToCrouch && _moveInput.magnitude > 0f && groundedOrWall)
        {
            if (_isCharging)
            {
                _isCharging = false;
                _chargeTimer = 0f;
            }
            _isCrouching = true;

            // --- MOVIDO A FIXEDUPDATE ---
            // Toda la lógica de _rb.velocity se movió
            // --- FIN MOVIDO ---
        }
        else
        {
            _isCrouching = false;

            // Lógica de Salto Cargado
            if (_moveInput.magnitude > 0f && canJump)
            {
                _isCharging = true;
                _chargeTimer += Time.deltaTime;
                _chargeTimer = Mathf.Clamp(_chargeTimer, 0f, _maxChargeTime);
                _lastInputDir = _moveInput;
            }

            if (_isCharging && _moveInput.magnitude == 0f)
            {
                ExecuteJump(); // ExecuteJump usa AddForce, por lo que está OK
            }
        }
    }
    private void HandleCrouching()
    {
        if (!_isCrouching) return;

        // Calculamos la dirección de la cámara (esto es rápido, no impacta)
        Vector3 camForward = _playerCam.transform.forward;
        camForward.y = 0f;
        camForward.Normalize();

        Vector3 camRight = _playerCam.transform.right;
        camRight.y = 0f;
        camRight.Normalize();

        Vector3 moveDir = (camForward * _moveInput.z + camRight * _moveInput.x).normalized;

        // Aplicamos la velocidad
        Vector3 targetVelocity = moveDir * _crouchSpeed;

        // Usamos Time.fixedDeltaTime porque estamos en FixedUpdate
        Vector3 velocity = Vector3.Lerp(new Vector3(_rb.velocity.x, 0, _rb.velocity.z), targetVelocity, Time.fixedDeltaTime * _moveSmooth);

        _rb.velocity = new Vector3(velocity.x, _rb.velocity.y, velocity.z);
    }
    private void HandleAirSteering()
    {
        bool inAir = !_floorDetector.IsGrounded;
        bool isGrappling = _grapple != null && (_grapple.IsGrappling() || _grapple.IsStuckToWall);

        // Solo aplicamos el timón si estamos en el aire y no estamos usando el gancho
        if (inAir && !isGrappling)
        {
            // 1. Obtenemos la dirección de la cámara (solo horizontal)
            Vector3 camDir = _playerCam.transform.forward;
            camDir.y = 0;
            camDir.Normalize();

            // 2. Obtenemos la velocidad horizontal actual
            Vector3 horizontalVel = new Vector3(_rb.velocity.x, 0, _rb.velocity.z);

            // 3. Solo aplicamos fuerza si no hemos alcanzado la velocidad máxima
            if (horizontalVel.magnitude < _maxAirSpeed)
            {
                // 4. Aplicamos una fuerza constante en la dirección de la cámara
                // Esto permite "empujar" la trayectoria de tu salto
                _rb.AddForce(camDir * _airSteerStrength, ForceMode.Acceleration);
            }
        }
    }
    private void HandleCameraChargeEffect()
    {
        if (_headTransform == null) return;

        float chargePercent = _chargeTimer / _maxChargeTime;
        Vector3 target = _headDefaultLocalPos + Vector3.down * (_cameraDropAmount * chargePercent);

        _headTransform.localPosition = Vector3.Lerp(
            _headTransform.localPosition,
            target,
            Time.deltaTime * _cameraLerpSpeed
        );
    }

    private void ExecuteJump()
    {
        _isCharging = false;
        _lastJumpTime = Time.time;

        if (_grapple != null && _grapple.IsStuckToWall)
            _grapple.ReleaseFromWall();

        // dirección cámara
        Vector3 camForward = _playerCam.transform.forward.normalized;
        Vector3 camRight = _playerCam.transform.right.normalized;

        // input proyectado en plano
        Vector3 inputDir = (camForward * _lastInputDir.z + camRight * _lastInputDir.x);
        inputDir.y = 0f;
        inputDir.Normalize();

        // carga
        float chargePercent = _chargeTimer / _maxChargeTime;
        float jumpForce = Mathf.Lerp(_minForce, _maxForce, _chargeCurve.Evaluate(chargePercent));

        // altura en función del ángulo cámara
        float verticalFactor = Mathf.Clamp01(_playerCam.transform.forward.y + 0.5f);
        float verticalForce = _verticalBoost + (verticalFactor * jumpForce);

        // si mira muy arriba -> salto casi vertical
        Vector3 jumpDir = inputDir * jumpForce;
        if (_playerCam.transform.forward.y > 0.8f)
            jumpDir = Vector3.zero;

        // aplicar fuerza
        _rb.AddForce(jumpDir + Vector3.up * verticalForce, ForceMode.Impulse);

        // reset
        _chargeTimer = 0f;
        _lastInputDir = Vector3.zero;
    }



    public void ModifyMovement(float crouchMultiplier, float jumpMultiplier)
    {
        _crouchSpeed = _baseCrouchSpeed * crouchMultiplier;
        _maxForce = _baseMaxForce * jumpMultiplier;
    }

    public void ResetMovement()
    {
        _crouchSpeed = _baseCrouchSpeed;
        _maxForce = _baseMaxForce;
    }
}