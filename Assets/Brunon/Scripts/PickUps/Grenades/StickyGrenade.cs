using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;  

public class StickyGrenade : BaseThrownGrenade
{
    [Header("Pegajosa (Específico)")]
    [SerializeField] private NetworkObject _slowZonePrefab; // Arrastra tu prefab de zona aquí
    [SerializeField] private float _maxGroundDistance = 20f;
    [SerializeField] private LayerMask _groundLayer;
    // ¡Aquí implementamos la lógica de la zona!
    protected override void ActivateEffect(NetworkRunner runner, Collision collision)
    {
        if (_slowZonePrefab == null)
        {
            Debug.LogError("StickyGrenade no tiene un _slowZonePrefab asignado.");
            return;
        }

        // --- LÓGICA DE BÚSQUEDA DE SUELO ---

        // 1. Definimos el punto de inicio (donde explotó la granada)
        Vector3 startPoint = transform.position;

        // 2. Lanzamos un rayo invisible hacia abajo (Vector3.down)
        bool foundGround = Physics.Raycast(
            startPoint,
            Vector3.down,
            out RaycastHit hit,
            _maxGroundDistance,
            _groundLayer
        );

        if (foundGround)
        {
            Vector3 deployPosition = hit.point;
            Quaternion deployOrientation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            // Usamos runner.Instantiate para crear la zona en la red
            runner.Spawn(_slowZonePrefab, deployPosition, deployOrientation);

            // La clase base se encargará de destruir este proyectil
        }

    }
}