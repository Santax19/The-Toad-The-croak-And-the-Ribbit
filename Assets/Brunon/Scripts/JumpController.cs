using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

public class JumpController : MonoBehaviour
{
    [Header("Jump Settings")]
    [SerializeField] private float _maxChargeTime = 2f; // tiempo maximo de carga
    [SerializeField] private float _maxForce = 20f; // fuerza maxima
    [SerializeField] private float _minForce = 5f;  // fuerza minima
    [SerializeField] private float _verticalBoost = 5f; // impulso vertical minimo
    [SerializeField] private float _jumpCooldown = 0.3f; // tiempo entre saltos
    [SerializeField] private AnimationCurve _chargeCurve;

    private Rigidbody _rb;
    private Camera _playerCam;
    private FloorDetector _floorDetector;
    private GrappleBruno _grapple;
    private Vector3 _lastInputDir = Vector3.zero;

    private float _chargeTimer = 0f;
    private bool _isCharging = false;

    private float _lastJumpTime; // control de cooldown

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _grapple = GetComponent<GrappleBruno>();
        _playerCam = GetComponentInChildren<Camera>();
        _floorDetector = GetComponentInChildren<FloorDetector>();
        // el detector va en un hijo con collider trigger
    }

    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        // input actual cada frame
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 moveInput = new Vector3(h, 0f, v).normalized;

        bool canJump = (_floorDetector.IsGrounded || (_grapple != null && _grapple.IsStuckToWall))
                       && Time.time > _lastJumpTime + _jumpCooldown;

        if (moveInput.magnitude > 0f && canJump)
        {
            _isCharging = true;
            _chargeTimer += Time.deltaTime;
            _chargeTimer = Mathf.Clamp(_chargeTimer, 0f, _maxChargeTime);

            _lastInputDir = moveInput; // actualizamos el último input válido
        }

        // al soltar todas las teclas, ejecutamos salto
        if (_isCharging && moveInput.magnitude == 0f)
        {
            ExecuteJump();
        }
    }

    private void ExecuteJump()
    {
        _isCharging = false;
        _lastJumpTime = Time.time;

        // soltamos grapple recién ahora
        if (_grapple != null && _grapple.IsStuckToWall)
            _grapple.ReleaseFromWall();

        // cálculo de dirección relativa a la cámara
        Vector3 camForward = _playerCam.transform.forward;
        camForward.y = 0f;
        camForward.Normalize();

        Vector3 camRight = _playerCam.transform.right;
        camRight.y = 0f;
        camRight.Normalize();

        // combinamos el último input WASD con la cámara
        Vector3 jumpDir = (camForward * _lastInputDir.z + camRight * _lastInputDir.x).normalized;

        // fuerza según carga
        float chargePercent = _chargeTimer / _maxChargeTime;
        float jumpForce = Mathf.Lerp(_minForce, _maxForce, _chargeCurve.Evaluate(chargePercent));

        // aplicamos salto + impulso vertical
        _rb.AddForce(jumpDir * jumpForce + Vector3.up * _verticalBoost, ForceMode.Impulse);

        // reseteamos
        _chargeTimer = 0f;
        _lastInputDir = Vector3.zero;
    }
}
