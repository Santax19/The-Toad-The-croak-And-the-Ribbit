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

    private float _chargeTimer = 0f;
    private bool _isCharging = false;
    private Vector3 _inputDir;

    private float _lastJumpTime; // control de cooldown

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
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
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 moveInput = new Vector3(h, 0, v).normalized;

        // solo cargamos si está en el suelo y no en cooldown
        if (moveInput.magnitude > 0f && _floorDetector.IsGrounded && Time.time > _lastJumpTime + _jumpCooldown)
        {
            if (!_isCharging)
            {
                // empieza carga
                _isCharging = true;
                _chargeTimer = 0f;
                _inputDir = moveInput;
            }
            else
            {
                // sigue cargando
                _chargeTimer += Time.deltaTime;
                _chargeTimer = Mathf.Clamp(_chargeTimer, 0f, _maxChargeTime);
            }
        }

        // si estaba cargando y soltas la tecla -> ejecutar salto
        if (_isCharging && moveInput.magnitude == 0f)
        {
            ExecuteJump();
        }
    }

    private void ExecuteJump()
    {
        _isCharging = false;

        // calcula fuerza segun tiempo cargado y curva
        float chargePercent = _chargeTimer / _maxChargeTime;
        float force = Mathf.Lerp(_minForce, _maxForce, _chargeCurve.Evaluate(chargePercent));

        // calcula direccion en base a la camara
        Vector3 camForward = _playerCam.transform.forward;
        Vector3 camRight = _playerCam.transform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 jumpDir = (camForward * _inputDir.z + camRight * _inputDir.x).normalized;

        if (jumpDir == Vector3.zero)
            jumpDir = Vector3.forward; // default hacia adelante

        // siempre agregamos un impulso vertical mínimo
        jumpDir = new Vector3(jumpDir.x, 1f, jumpDir.z).normalized;

        // aplicamos fuerza de impulso
        _rb.AddForce(jumpDir * force + Vector3.up * _verticalBoost, ForceMode.Impulse);

        // cooldown
        _lastJumpTime = Time.time;

        // reseteamos carga
        _chargeTimer = 0f;
    }
}
