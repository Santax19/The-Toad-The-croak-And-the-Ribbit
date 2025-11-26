using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
public class PlayerAnimatorManager : NetworkBehaviour
{
    [Header("Referencias")]
    [SerializeField] private NetworkMecanimAnimator _netAnimator; // El animator del cuerpo TP
    [SerializeField] private MovementController _movementController;
    [SerializeField] private WeaponManager _weaponManager;
    public override void Render()
    {
        // Render corre en cada frame visual. Aquí actualizamos el Animator.
        if (_netAnimator == null || _movementController == null) return;

        // 1. Leer datos del MovementController
        float moveX = _movementController.AnimInputX;
        float moveY = _movementController.AnimInputY;
        float speed = _movementController.NetworkMoveSpeed;
        bool isJumping = _movementController.IsJumpingBool;
        bool isCharging = _movementController.IsCharging;

        // 2. Enviar al Animator (Tercera Persona)
        _netAnimator.Animator.SetFloat("InputX", moveX);
        _netAnimator.Animator.SetFloat("InputY", moveY);
        _netAnimator.Animator.SetFloat("MoveSpeed", speed); // Para los brazos

        _netAnimator.Animator.SetBool("IsJumping", isJumping);
        _netAnimator.Animator.SetBool("IsCharging", isCharging);

        // 3. Datos del Arma (Apuntar)
        // Asumimos que el WeaponManager tiene una propiedad pública IsAiming
        // Si no la tiene, la añadiremos en el paso 3 abajo.
        if (_weaponManager != null)
        {
            _netAnimator.Animator.SetBool("IsAiming", _weaponManager.IsAiming);
        }
    }
}
