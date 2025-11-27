using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TongueAnimator : MonoBehaviour
{
    public enum Axis { X, Y, Z }

    [Header("Conexiones")]
    [SerializeField] private GrappleBruno _grappleScript; // Arrastra el player raíz aquí

    [Header("Bones")]
    [SerializeField] private Transform _boneRoot;
    [SerializeField] private Transform _boneStretch;
    [SerializeField] private Axis _stretchAxis = Axis.Y;
    [SerializeField] private Vector3 _rotationOffset = Vector3.zero;
    [Header("Configuración Visual")]
    [SerializeField] private float _originalLength = 1f;
    [SerializeField] private float _retractSpeed = 80f;
    [SerializeField] private Transform _originPoint; // FP: Debajo de cámara. TP: Boca.

    // Estado interno visual
    private Vector3 _currentTipPosition;
    private bool _isExtending = false;
    private Vector3 _initialScaleStretch;

    private void Start()
    {
        // Inicializamos la punta en el origen
        if(_boneStretch != null)
            _initialScaleStretch = _boneStretch.localScale;

        if (_originPoint != null)
            _currentTipPosition = _originPoint.position;

        SetTongueVisibility(false);
    }

    private void LateUpdate() // LateUpdate para sobreescribir cualquier animación si la hubiera
    {
        if (_grappleScript == null || _originPoint == null || _boneRoot == null) return;

        // 1. Datos del Grapple Lógico
        bool logicIsGrappling = _grappleScript.IsGrappling || _grappleScript.IsStuckToWall;
        Vector3 targetPoint = _grappleScript.CurrentGrapplePoint;
        Vector3 originPos = _originPoint.position;

        // 2. Definir Objetivo y Velocidad
        Vector3 desiredTipPos = logicIsGrappling ? targetPoint : originPos;
        _isExtending = logicIsGrappling;
        float speed = _isExtending ? _grappleScript.TongueFlySpeed : _retractSpeed;

        // 3. Mover la punta virtualmente
        _currentTipPosition = Vector3.MoveTowards(_currentTipPosition, desiredTipPos, speed * Time.deltaTime);

        // 4. CLAMP: Frenar si excede el rango máximo del script lógico
        Vector3 directionVector = _currentTipPosition - originPos;
        float currentDistance = directionVector.magnitude;
        float maxAllowedDistance = _grappleScript.MaxRange; // Leemos el límite del script lógico

        // Si la distancia visual supera el rango máximo, la recortamos
        if (currentDistance > maxAllowedDistance)
        {
            currentDistance = maxAllowedDistance;
            // Recalculamos la posición de la punta para que coincida con el tope
            _currentTipPosition = originPos + directionVector.normalized * maxAllowedDistance;
            directionVector = _currentTipPosition - originPos; // Actualizamos el vector
        }

        // 4. Aplicar transformaciones a los huesos
        if (currentDistance > 0.1f)
        {
            SetTongueVisibility(true);

            // A. POSICIÓN
            _boneRoot.position = originPos;

            // B. ROTACIÓN
            if (directionVector != Vector3.zero)
            {
                Quaternion lookRot = Quaternion.LookRotation(directionVector, Vector3.up);
                _boneRoot.rotation = lookRot * Quaternion.Euler(_rotationOffset);
            }

            // C. ESCALA
            float scaleFactor = (currentDistance / _originalLength);

            Vector3 targetScale = _initialScaleStretch;
            switch (_stretchAxis)
            {
                case Axis.X: targetScale.x = scaleFactor; break;
                case Axis.Y: targetScale.y = scaleFactor; break;
                case Axis.Z: targetScale.z = scaleFactor; break;
            }
            _boneStretch.localScale = targetScale;
        }
        else
        {
            SetTongueVisibility(false);
            _boneRoot.position = originPos;
            _boneStretch.localScale = _initialScaleStretch;
            // Forzamos la posición de la punta al origen para que no quede "flotando" lejos
            _currentTipPosition = originPos;
        }
    }

    private void SetTongueVisibility(bool visible)
    {
        var renderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (var r in renderers) r.enabled = visible;
    }
}
