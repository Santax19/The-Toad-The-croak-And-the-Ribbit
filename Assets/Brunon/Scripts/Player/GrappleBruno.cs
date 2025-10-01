using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrappleBruno : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Camera _cam;
    [SerializeField] private LayerMask _grappleMask;
    [SerializeField] private LayerMask _wallMask;
    [SerializeField] private LayerMask _pickupMask;

    [Header("Ajustes del Grapple")]
    [SerializeField] private float _grappleRange = 30f;   // distancia máxima
    [SerializeField] private float _pullSpeed = 20f;      // velocidad de arrastre  
    [SerializeField] private float _wallCheckDistance = 0.6f;
    [SerializeField] private float _grappleCooldown = 1f; // segundos de cooldown
    [SerializeField] private Vector3 _wallBoxSize = new(0.5f, 1f, 0.1f);

    private Rigidbody _rb;
    private Vector3 _grapplePoint;
    private bool _isGrappling = false;
    private bool _isStuckToWall = false;
    private bool _isDampingVelocity = false;
    private float _lastGrappleTime = -999f;

    public bool IsStuckToWall => _isStuckToWall;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        // inicio grapple (click derecho) con cooldown
        if (Input.GetKeyDown(KeyCode.Space) && Time.time >= _lastGrappleTime + _grappleCooldown)
            TryStartGrapple();

        // cancelar manualmente
        if (Input.GetMouseButtonUp(1) && _isGrappling)
            StopGrappling();
    }

    private void FixedUpdate()
    {
        if (_isGrappling)
        {
            DoGrapple();
            DetectWall();
        }
        else if (_isStuckToWall)
        {
            StickToWall();
        }
        if (_isDampingVelocity)
        {
            // hacemos que la velocidad vaya decayendo hasta un 30%
            _rb.velocity = Vector3.Lerp(_rb.velocity, _rb.velocity * 0.7f, 2f * Time.fixedDeltaTime);

            // si ya está suficientemente cerca del 30%, cortamos
            if (_rb.velocity.magnitude <= (_rb.velocity.magnitude * 0.71f))
                _isDampingVelocity = false;
        }
    }

    private void TryStartGrapple()
    {
        Ray ray = _cam.ScreenPointToRay(Input.mousePosition);

        // 1. Primero chequeamos pickups
        if (Physics.Raycast(ray, out RaycastHit hitPickup, _grappleRange, _pickupMask))
        {
            if (hitPickup.collider.TryGetComponent<Pickup>(out var pickup))
            {
                // Buscar PlayerHealth y MovementController en este jugador
                var health = GetComponent<PlayerHealth>();
                var movement = GetComponent<MovementController>();

                pickup.Consume(health, movement);
                return; // no iniciamos grapple
            }
        }

        // 2. Si no es pickup, chequeamos grapple normal
        if (Physics.Raycast(ray, out RaycastHit hit, _grappleRange, _grappleMask))
        {
            _grapplePoint = hit.point;
            _isGrappling = true;
            _lastGrappleTime = Time.time;
        }
    }

    private void DoGrapple()
    {
        Vector3 dir = (_grapplePoint - transform.position).normalized;
        _ = Vector3.Distance(transform.position, _grapplePoint);

        _rb.velocity = dir * _pullSpeed;
    }

    private void StopGrapple()
    {
        // este Stop es SOLO por pared
        _isGrappling = false;
        _isStuckToWall = true;
        _rb.useGravity = false;
        _rb.velocity = Vector3.zero;
    }

    private void StopGrappling()
    {
        // este Stop es cuando suelta el gancho antes de llegar a pared
        _isDampingVelocity = true;
        _isGrappling = false;
    }
    private void DetectWall()
    {
        // direcciones básicas alrededor del jugador
        Vector3[] directions = { transform.forward, -transform.forward, transform.right, -transform.right };

        foreach (var dir in directions)
        {
            Vector3 center = transform.position + dir * _wallCheckDistance;
            if (Physics.CheckBox(center, _wallBoxSize / 2, Quaternion.identity, _wallMask))
            {
                StopGrapple();
                _isStuckToWall = true;
                _rb.useGravity = false; // ignoramos gravedad
                _rb.velocity = Vector3.zero;
                return;
            }
        }
    }
    private void StickToWall()
    {
        // básicamente "flotar" pegado
        _rb.velocity = Vector3.zero;
    }
    public void ReleaseFromWall()
    {
        if (_isStuckToWall)
        {
            _isStuckToWall = false;
            _rb.useGravity = true;
        }
    }
}

