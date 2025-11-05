using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
[RequireComponent(typeof(Rigidbody))]

public class MovementController : NetworkBehaviour
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
    [SerializeField] private float _crouchSpeed; // velocidad lenta
    [SerializeField] private float _moveSmooth;

    [Header("Air Control")]
    [SerializeField] private float _airSteerStrength; // Fuerza del timón en el aire

    private Rigidbody _rb;
    private Camera _playerCam;
    private FloorDetector _floorDetector;
    private GrappleBruno _grapple;
    private WeaponManager _weaponManager;

    [Networked] private float ChargeTimer { get; set; } = 0f;
    [Networked] private NetworkBool IsCharging { get; set; } = false;
    [Networked] private Vector3 LastInputDir { get; set; } = Vector3.zero;
    [Networked] private Vector3 LastJumpRawInput { get; set; } = Vector3.zero;

    // --- Variables Locales (no necesitan red) ---
    private Vector3 _headDefaultLocalPos;
    private bool _isCrouching = false; // El crouch es local, solo afecta el movimiento
    private float _baseCrouchSpeed;
    private float _baseMaxForce;
    private float _lastJumpTime;


    public override void Spawned()
    {
        // Awake() se convierte en la parte de arriba de Spawned()
        _rb = GetComponent<Rigidbody>();
        _grapple = GetComponent<GrappleBruno>(); // Lo adaptaremos después
        _playerCam = GetComponentInChildren<Camera>(); // Ojo: CameraController ya la maneja
        _floorDetector = GetComponentInChildren<FloorDetector>();
        _weaponManager = GetComponent<WeaponManager>();
        _baseCrouchSpeed = _crouchSpeed;
        _baseMaxForce = _maxForce;

        // Start() se convierte en la parte de abajo
        if (_headTransform != null)
            _headDefaultLocalPos = _headTransform.localPosition;

        // IMPORTANTE: Desactivamos la interpolación de Rigidbody para
        // que el NetworkRigidbody (que añadiremos) tome el control.
        _rb.interpolation = RigidbodyInterpolation.None;
    }

    public override void FixedUpdateNetwork()
    {
        // Solo el jugador con autoridad de input puede ejecutar esto
        // El estado (posición, _isCharging) se sincronizará a los demás
        if (!GetInput(out NetworkInputData data))
        {
            // Si no tenemos input, no hacemos nada
            return;
        }
        if (_weaponManager != null)
        {
            _weaponManager.NetworkedWeaponUpdate(data);
        }
        // --- Lógica porteada de HandleInputReading() ---
        Vector3 moveInput = new Vector3(data.moveInput.x, 0f, data.moveInput.y).normalized;
        bool wantsToCrouch = data.crouch;

        bool groundedOrWall = _floorDetector.IsGrounded || (_grapple != null && _grapple.IsStuckToWall);
        bool canJump = groundedOrWall && Runner.SimulationTime > _lastJumpTime + _jumpCooldown;

        if (groundedOrWall) { LastJumpRawInput = Vector3.zero; }

        // Lógica de Crouch
        if (wantsToCrouch && moveInput.magnitude > 0f && groundedOrWall)
        {
            if (IsCharging)
            {
                IsCharging = false;
                ChargeTimer = 0f;
            }
            _isCrouching = true;
        }
        else
        {
            _isCrouching = false;

            // Lógica de Salto Cargado
            if (moveInput.magnitude > 0f && canJump)
            {
                IsCharging = true;
                ChargeTimer += Runner.DeltaTime; // Usamos Runner.DeltaTime
                ChargeTimer = Mathf.Clamp(ChargeTimer, 0f, _maxChargeTime);
                LastInputDir = moveInput;
                LastJumpRawInput = LastInputDir;
            }

            if (IsCharging && moveInput.magnitude == 0f)
            {
                ExecuteJump();
            }
        }

        // --- Lógica porteada de FixedUpdate() ---
        HandleCrouching(moveInput); // Le pasamos el input
        HandleAirSteering();
    }
    public override void Render()
    {
        // Solo el jugador local necesita ver el efecto de "bajar" la cámara
        if (Object.HasInputAuthority)
        {
            HandleCameraChargeEffect();
        }
    }
    private void HandleCrouching(Vector3 moveInput)
    {
        if (!_isCrouching) return;

        Vector3 camForward = _playerCam.transform.forward;
        camForward.y = 0f;
        camForward.Normalize();

        Vector3 camRight = _playerCam.transform.right;
        camRight.y = 0f;
        camRight.Normalize();

        // Usamos el moveInput que nos llegó por red
        Vector3 moveDir = (camForward * moveInput.z + camRight * moveInput.x).normalized;
        Vector3 targetVelocity = moveDir * _crouchSpeed;

        // Usamos Runner.DeltaTime
        Vector3 velocity = Vector3.Lerp(new Vector3(_rb.velocity.x, 0, _rb.velocity.z), targetVelocity, Runner.DeltaTime * _moveSmooth);

        _rb.velocity = new Vector3(velocity.x, _rb.velocity.y, velocity.z);
    }
    private void HandleCameraChargeEffect()
    {
        if (_headTransform == null) return;

        float chargePercent = ChargeTimer / _maxChargeTime;
        Vector3 target = _headDefaultLocalPos + Vector3.down * (_cameraDropAmount * chargePercent);

        // Usamos Time.deltaTime (normal) porque Render() corre en el loop de Unity
        _headTransform.localPosition = Vector3.Lerp(
            _headTransform.localPosition,
            target,
            Time.deltaTime * _cameraLerpSpeed
        );
    }
    private void ExecuteJump()
    {
        IsCharging = false;
        _lastJumpTime = Runner.SimulationTime;

        if (_grapple != null && _grapple.IsStuckToWall)
            _grapple.ReleaseFromWall();

        // dirección cámara
        Vector3 camForward = _playerCam.transform.forward.normalized;
        Vector3 camRight = _playerCam.transform.right.normalized;

        // input proyectado en plano
        Vector3 inputDir = (camForward * LastInputDir.z + camRight * LastInputDir.x);
        inputDir.y = 0f;
        inputDir.Normalize();

        // carga
        float chargePercent = ChargeTimer / _maxChargeTime;
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
        ChargeTimer = 0f;
        LastInputDir = Vector3.zero;
    }
    private void HandleAirSteering()
    {
        bool inAir = !_floorDetector.IsGrounded;
        bool isGrappling = _grapple != null && (_grapple.IsGrappling() || _grapple.IsStuckToWall);

        // Solo aplicamos el timón si estamos en el aire y no estamos usando el gancho
        if (inAir && !isGrappling && !_isCrouching) // Añadí !_isCrouching por si acaso
        {
            // 1. Decidimos el multiplicador basado en el input del salto
            float steerMultiplier = 0f;

            if (LastJumpRawInput.z > 0f) // Salto con W, WA, o WD (eje Z positivo)
            {
                steerMultiplier = 1f; // Timón normal
            }
            else if (LastJumpRawInput.z < 0f) // Salto con S, SA, o SD (eje Z negativo)
            {
                steerMultiplier = -1f; // Timón invertido
            }
            // Si LastJumpRawInput.z == 0 (salto solo con A o D), el multiplicador queda en 0.

            // 2. Si no hay timón (salto lateral), salimos
            if (steerMultiplier == 0f)
            {
                return;
            }
            // 3. Obtenemos la dirección de la cámara (solo horizontal)
            Vector3 horizontalVel = new Vector3(_rb.velocity.x, 0, _rb.velocity.z);
            float currentSpeed = horizontalVel.magnitude;

            // 3. Obtenemos la dirección objetivo (cámara)
            Vector3 camDir = _playerCam.transform.forward;
            camDir.y = 0;
            camDir.Normalize();

            // 4. Creamos el vector de "velocidad objetivo"
            // (Dirección de la cámara * multiplicador * velocidad actual)
            Vector3 targetVel = camDir * steerMultiplier * currentSpeed;

            // 5. Usamos Lerp para mezclar la velocidad actual con la objetivo
            // _airSteerStrength ahora actúa como la "velocidad de giro"
            Vector3 newVel = Vector3.Lerp(
                horizontalVel,
                targetVel,
                _airSteerStrength * Time.fixedDeltaTime
            );

            // 6. Aplicamos la nueva velocidad horizontal, pero mantenemos la vertical (gravedad)
            _rb.velocity = new Vector3(newVel.x, _rb.velocity.y, newVel.z);

        }
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