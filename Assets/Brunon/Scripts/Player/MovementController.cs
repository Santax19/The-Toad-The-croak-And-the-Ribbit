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
    [SerializeField] private float _rotationSpeed = 80f;

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
    private CameraController _camController;

    [Networked] private float JumpLaunchTime { get; set; }
    [Networked] public float ChargeTimer { get; set; } = 0f;
    [Networked] public NetworkBool IsCharging { get; set; } = false;
    [Networked] public Vector3 LastInputDir { get; set; } = Vector3.zero;
    [Networked] public Vector3 LastJumpRawInput { get; set; } = Vector3.zero;
    [Networked] public float NetworkMoveSpeed { get; set; }
    [Networked] public float AnimInputX { get; set; }
    [Networked] public float AnimInputY { get; set; }
    // Sincronizamos si está en el aire
    [Networked] public NetworkBool IsJumpingBool { get; set; }
    [Networked] private float _currentYRotation { get; set; }

    // --- Variables Locales
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
        _playerCam = GetComponentInChildren<Camera>();
        _floorDetector = GetComponentInChildren<FloorDetector>();
        _weaponManager = GetComponent<WeaponManager>();
        _camController = GetComponent<CameraController>();
        _baseCrouchSpeed = _crouchSpeed;
        _baseMaxForce = _maxForce;
        if (_headTransform != null)
            _headDefaultLocalPos = _headTransform.localPosition;
        _rb.interpolation = RigidbodyInterpolation.None;
    }

    public override void FixedUpdateNetwork()
    {
        if (!GetInput(out NetworkInputData data)) return;
        if(data.mouseX != 0)
        {
            // Acumulamos el input en una variable de estado
            _currentYRotation += data.mouseX * _rotationSpeed * Runner.DeltaTime;
            // Creamos la rotación final
            Quaternion targetRotation = Quaternion.Euler(0, _currentYRotation, 0);
            // Aplicamos rotación absoluta al Rigidbody
            _rb.MoveRotation(targetRotation);
        }
        if (_weaponManager != null)
            _weaponManager.NetworkedWeaponUpdate(data);

        // 1. Estado de Salto
        bool groundedOrWall = _floorDetector.IsGrounded || (_grapple != null && _grapple.IsStuckToWall);
        bool isTakingOff = Runner.SimulationTime < (JumpLaunchTime + 0.2f);
        IsJumpingBool = !groundedOrWall || isTakingOff;

        // 2. Inputs de Movimiento
        Vector3 moveInputRaw = new Vector3(data.moveInput.x, 0f, data.moveInput.y).normalized;

        // 3. Actualizar Variables para el Animador
        if (IsJumpingBool || IsCharging)
        {
            AnimInputX = 0f;
            AnimInputY = 0f;
            NetworkMoveSpeed = 0f;
        }
        else
        {
            AnimInputX = Mathf.Round(moveInputRaw.x);
            AnimInputY = Mathf.Round(moveInputRaw.z);

            // Calculamos velocidad real horizontal para el Blend Tree de brazos
            bool isMoving = moveInputRaw.magnitude > 0.1f;
            // Si hay input, mandamos velocidad, si no, 0 (para evitar deslizamiento visual)
            NetworkMoveSpeed = isMoving ? 1f : 0f;
        }

        // Si no estamos en suelo ni pared, estamos saltando/cayendo
        IsJumpingBool = !groundedOrWall || isTakingOff;
        bool wantsToCrouch = data.buttons.IsSet(MyButtons.Crouch);
        bool canJump = groundedOrWall && Runner.SimulationTime > _lastJumpTime + _jumpCooldown;

        if (groundedOrWall) { LastJumpRawInput = Vector3.zero; }

        // Lógica de Crouch
        if (wantsToCrouch && moveInputRaw.magnitude > 0f && groundedOrWall)
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
            if (moveInputRaw.magnitude > 0f && canJump)
            {
                IsCharging = true;
                ChargeTimer += Runner.DeltaTime; // Usamos Runner.DeltaTime
                ChargeTimer = Mathf.Clamp(ChargeTimer, 0f, _maxChargeTime);
                LastInputDir = moveInputRaw;
                LastJumpRawInput = LastInputDir;
                HandleCameraChargeEffect();
            }

            if (IsCharging && moveInputRaw.magnitude == 0f)
            {
                ExecuteJump();
            }
        }

        // --- Lógica porteada de FixedUpdate() ---
        HandleCrouching(moveInputRaw); // Le pasamos el input
        HandleAirSteering();
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
        JumpLaunchTime = Runner.SimulationTime;
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
        LastJumpRawInput = LastInputDir;
        LastInputDir = Vector3.zero;
    }
    private void HandleAirSteering()
    {
        bool inAir = IsJumpingBool;
        bool isGrappling = _grapple != null && (_grapple.IsGrapplingNet || _grapple.IsStuckToWall);

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
            Vector3 horizontalVel = new(_rb.velocity.x, 0, _rb.velocity.z);
            float currentSpeed = horizontalVel.magnitude;

            // 3. Obtenemos la dirección objetivo (cámara)
            Vector3 camDir = _playerCam.transform.forward;
            camDir.y = 0;
            camDir.Normalize();

            // 4. Creamos el vector de "velocidad objetivo"
            // (Dirección de la cámara * multiplicador * velocidad actual)
            Vector3 targetVel = currentSpeed * steerMultiplier * camDir;

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