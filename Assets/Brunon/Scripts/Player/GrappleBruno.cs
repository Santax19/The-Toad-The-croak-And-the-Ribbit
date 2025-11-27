using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class GrappleBruno : NetworkBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Camera _cam;
    [SerializeField] private LayerMask _grappleMask;
    [SerializeField] private LayerMask _wallMask;
    [SerializeField] private LayerMask _pickupMask;

    [Header("Ajustes del Grapple")]
    [SerializeField] private float _grappleRange = 14f;   // distancia máxima
    [SerializeField] private float _pullSpeed = 20f;      // velocidad de arrastre  
    [SerializeField] private float _wallCheckDistance = 0.6f;
    [SerializeField] private float _grappleCooldown = 1f; // segundos de cooldown
    [SerializeField] private Vector3 _wallBoxSize = new(0.5f, 1f, 0.1f);
    [SerializeField] private float _missDuration = 0.3f;
    [SerializeField] public float TongueFlySpeed = 65f;
    private float _dynamicRetractTime = 0f;
    private Rigidbody _rb;
    private Vector3 _grapplePoint;
    private float _lastGrappleTime = -999f;


    [Networked] public NetworkBool IsGrapplingNet { get; set; }
    [Networked] public NetworkBool IsLatchedNet { get; set; }
    [Networked] public Vector3 GrapplePointNet { get; set; }
    [Networked] public NetworkBool IsStuckToWallNet { get; set; }
    public bool IsGrappling => IsGrapplingNet;
    public bool IsStuckToWall => IsStuckToWallNet;
    public Vector3 CurrentGrapplePoint => GrapplePointNet;
    public float MaxRange => _grappleRange;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }
    public override void FixedUpdateNetwork()
    {
        if (IsGrapplingNet && !IsLatchedNet)
        {
            // Usamos la variable _dynamicRetractTime en lugar de una constante fija
            if (Runner.SimulationTime > _dynamicRetractTime)
            {
                ReleaseGrapple();
            }
        }
        if (IsGrapplingNet && IsLatchedNet)
        {
            DoGrapple();
            DetectWall();
        }
        else if (IsStuckToWallNet)
        {
            StickToWall();
        }
        if (GetInput(out NetworkInputData data))
        {
            // Detectar el inicio del disparo
            if (data.grapple && !IsGrapplingNet && !IsStuckToWallNet)
            {
                if (Runner.SimulationTime >= _lastGrappleTime + _grappleCooldown)
                {
                    StartGrappleOnce();
                }
            }
            // Cancelar manualmente
            if (!data.grapple && (IsGrapplingNet || IsStuckToWallNet))
            {
                ReleaseGrapple();
            }
        }
    }

    private void StartGrappleOnce()
    {
        _lastGrappleTime = Runner.SimulationTime;
        IsGrapplingNet = true;
        IsLatchedNet = false;
        Ray ray = _cam.ScreenPointToRay(Input.mousePosition);

        // 1. Primero chequeamos pickups
        if (Physics.Raycast(ray, out RaycastHit hitPickup, _grappleRange, _pickupMask))
        {
            GrapplePointNet = hitPickup.point;
            float distance = Vector3.Distance(ray.origin, hitPickup.point);
            // ¿Cuánto tiempo tarda la lengua en llegar ahí? (Tiempo = Distancia / Velocidad)
            float travelTime = distance / TongueFlySpeed;
            _dynamicRetractTime = Runner.SimulationTime + travelTime;
            // Consumir el objeto
            if (hitPickup.collider.TryGetComponent<Pickup>(out var pickup))
            {
                var health = GetComponent<PlayerHealth>();
                var movement = GetComponent<MovementController>();
                pickup.Consume(health, movement);
            }
        }

        // 2. Si no es pickup, chequeamos grapple normal
        if (Physics.Raycast(ray, out RaycastHit hit, _grappleRange, _grappleMask))
        {
            GrapplePointNet = hit.point;
            IsLatchedNet = true;
            IsGrapplingNet = true;
            _lastGrappleTime = Runner.SimulationTime;
            _rb.useGravity = false;
        }
        else
        {
            // no se enganchó a nada
            GrapplePointNet = ray.origin + ray.direction * _grappleRange;
            IsLatchedNet = false;
            float maxTime = _grappleRange / TongueFlySpeed;
            _dynamicRetractTime = Runner.SimulationTime + maxTime;
        }
    }

    private void DoGrapple()
    {
        Vector3 dir = (GrapplePointNet - transform.position).normalized;
        _rb.velocity = dir * _pullSpeed;
    }

    private void AttachToWall()
    {
        // este Stop es SOLO por pared
        IsGrapplingNet = false;
        IsLatchedNet = false;
        IsStuckToWallNet = true;
        _rb.useGravity = false;
        _rb.velocity = Vector3.zero;
    }

    private void ReleaseGrapple()
    {
        IsGrapplingNet = false;
        IsLatchedNet = false;
        IsStuckToWallNet = false;
        _rb.useGravity = true;
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
                AttachToWall();
                return;
            }
        }
    }
    private void StickToWall()
    {
        // básicamente "flotar" pegado
        _rb.velocity = Vector3.zero;
    }
    public void ReleaseFromWall() => ReleaseGrapple();
}

