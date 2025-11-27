using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
public class PlayerAnimatorManager : NetworkBehaviour
{
    [Header("Referencias")]
    [SerializeField] private NetworkMecanimAnimator _netAnimator; // El animator del cuerpo TP
    [SerializeField] private Animator _fpAnimator;
    [SerializeField] private MovementController _movementController;
    [SerializeField] private WeaponManager _weaponManager;
    public override void Render()
    {
        // Render corre en cada frame visual. Aquí actualizamos el Animator.
        if (_movementController == null) return;

        // 1. Leer datos del MovementController
        float moveX = _movementController.AnimInputX;
        float moveY = _movementController.AnimInputY;
        float speed = _movementController.NetworkMoveSpeed;
        bool isJumping = _movementController.IsJumpingBool;
        bool isCharging = _movementController.IsCharging;
        bool isAiming = _weaponManager != null ? _weaponManager.IsAiming : false;
        // 2. Enviar al Animator (Tercera Persona)
        if (_netAnimator != null && _netAnimator.Animator != null)
        {
            UpdateAnimator(_netAnimator.Animator, moveX, moveY, speed, isJumping, isCharging, isAiming);
        }

        if (Object.HasInputAuthority && _fpAnimator != null)
        {
            UpdateAnimator(_fpAnimator, moveX, moveY, speed, isJumping, isCharging, isAiming);
        }
    }

    private void UpdateAnimator(Animator anim, float x, float y, float speed, bool jump, bool charge, bool aim)
    {
        anim.SetFloat("InputX", x);
        anim.SetFloat("InputY", y);
        anim.SetFloat("MoveSpeed", speed);
        anim.SetBool("IsJumping", jump);
        anim.SetBool("IsCharging", charge);
        anim.SetBool("IsAiming", aim);
    }
    public void SetTrigger(string triggerName)
    {
        // Disparamos en el TP (Red)
        if (_netAnimator != null)
        {
            _netAnimator.SetTrigger(triggerName);
        }

        // Disparamos en el FP (Local) solo si somos nosotros
        if (Object.HasInputAuthority && _fpAnimator != null)
        {
            _fpAnimator.SetTrigger(triggerName);
        }
    }
}

