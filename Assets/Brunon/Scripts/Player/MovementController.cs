using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Addons.Physics;

[RequireComponent(typeof(NetworkRigidbody3D))]

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
    [SerializeField] private float _crouchSpeed = 3f; // velocidad lenta
    [SerializeField] private float _moveSmooth = 8f;

    [Header("Shooting Settings")]
    [SerializeField] private Transform firePoint; // Empty en la punta del arma o manos
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 50f;
    [SerializeField] private float maxShootDistance = 100f;
    [SerializeField] private float fireRate = 0.25f;

    private float _nextShootTime = 0f;

    private NetworkRigidbody3D _nrb;
    public Camera _playerCam;
    private FloorDetector _floorDetector;
    private GrappleBruno _grapple;

    private Vector3 _headDefaultLocalPos;
    private Vector3 _lastInputDir = Vector3.zero;

    private float _chargeTimer = 0f;
    private bool _isCharging = false;
    private bool _isCrouching = false;
    private float _baseCrouchSpeed;
    private float _baseMaxForce;

    private float _lastJumpTime;

    public override void Spawned()
    {
        _nrb = GetComponent<NetworkRigidbody3D>();
        _playerCam = GetComponentInChildren<Camera>();
        _grapple = GetComponent<GrappleBruno>();
        _floorDetector = GetComponentInChildren<FloorDetector>();
        _baseCrouchSpeed = _crouchSpeed;
        _baseMaxForce = _maxForce;

        // Activar cámara solo para el jugador local
        if (Object.HasInputAuthority)
        {
            if (_headTransform != null)
                _headDefaultLocalPos = _headTransform.localPosition;

            _playerCam.gameObject.SetActive(true);
        }
        else
        {
            _playerCam.gameObject.SetActive(false);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasInputAuthority)
            return;

        HandleInput();
        HandleCameraChargeEffect();
        HandleShooting();
    }

   

    private void Update()
    {
        HandleInput();
        HandleCameraChargeEffect();
        HandleShooting();
    }

    private void HandleInput()
    {
        // Inputs básicos
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 moveInput = new Vector3(h, 0f, v).normalized;

        bool groundedOrWall = _floorDetector.IsGrounded || (_grapple != null && _grapple.IsStuckToWall);
        bool canJump = groundedOrWall && Time.time > _lastJumpTime + _jumpCooldown;

        // --- Crouch (SHIFT + WASD) ---
        if (Input.GetKey(KeyCode.LeftShift) && moveInput.magnitude > 0f && groundedOrWall)
        {
            // si estaba cargando, interrumpimos
            if (_isCharging)
            {
                _isCharging = false;
                _chargeTimer = 0f;
            }

            _isCrouching = true;
            Vector3 camForward = _playerCam.transform.forward;
            camForward.y = 0f;
            camForward.Normalize();

            Vector3 camRight = _playerCam.transform.right;
            camRight.y = 0f;
            camRight.Normalize();

            Vector3 moveDir = (camForward * moveInput.z + camRight * moveInput.x).normalized;

            // movimiento lento
            Vector3 targetVelocity = moveDir * _crouchSpeed;
            Vector3 velocity = Vector3.Lerp(new Vector3(_nrb.Rigidbody.velocity.x, 0, _nrb.Rigidbody.velocity.z), targetVelocity, Time.deltaTime * _moveSmooth);

            _nrb.Rigidbody.velocity = new Vector3(velocity.x, _nrb.Rigidbody.velocity.y, velocity.z);
        }
        else
        {
            _isCrouching = false;

            // --- Salto cargado ---
            if (moveInput.magnitude > 0f && canJump)
            {
                _isCharging = true;
                _chargeTimer += Time.deltaTime;
                _chargeTimer = Mathf.Clamp(_chargeTimer, 0f, _maxChargeTime);

                _lastInputDir = moveInput;
            }

            // Al soltar input -> ejecutar salto
            if (_isCharging && moveInput.magnitude == 0f)
            {
                ExecuteJump();
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
        _nrb.Rigidbody.AddForce(jumpDir + Vector3.up * verticalForce, ForceMode.Impulse);

        // reset
        _chargeTimer = 0f;
        _lastInputDir = Vector3.zero;
    }

    private void HandleShooting()
    {
        if (Input.GetButtonDown("Fire1") && Time.time >= _nextShootTime) 
        {
            Shoot();
            _nextShootTime = Time.time + fireRate;
        }
    }

    private void Shoot()
    {
        if (!Object.HasInputAuthority) return;
        if (bulletPrefab.Equals(default) || firePoint == null) return;

        Ray ray = _playerCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Vector3 targetPoint = Physics.Raycast(ray, out RaycastHit hit, maxShootDistance)
            ? hit.point
            : ray.GetPoint(maxShootDistance);

        Vector3 direction = (targetPoint - firePoint.position).normalized;

        Runner.Spawn(
            bulletPrefab,
            firePoint.position,
            Quaternion.LookRotation(direction),
            Object.InputAuthority,
            (runner, obj) =>
            {
                if (obj.TryGetComponent(out Rigidbody rb))
                {
                    rb.velocity = direction * bulletSpeed;
                }
            });
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