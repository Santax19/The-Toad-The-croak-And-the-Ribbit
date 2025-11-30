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

    [Header("Efectos Visuales")]
    [SerializeField] private Renderer[] _bodyPartsRenderer; // Arrastra aquí la malla del personaje
    [SerializeField] private string _shaderDamageBool = "_IsActive";

    private Coroutine _damageCoroutine;
    private MaterialPropertyBlock _propBlock;
    public override void Spawned()
    {
        _propBlock = new MaterialPropertyBlock();
    }
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
    public void TriggerDamageVisuals()
    {
        // Validación de seguridad por si olvidaste asignar los renderers
        if (_bodyPartsRenderer == null || _bodyPartsRenderer.Length == 0) return;

        if (_damageCoroutine != null) StopCoroutine(_damageCoroutine);
        _damageCoroutine = StartCoroutine(DamageEffectRoutine());
    }

    private IEnumerator DamageEffectRoutine()
    {
        SetShaderBool(true);
        yield return new WaitForSeconds(1f);
        SetShaderBool(false);
        _damageCoroutine = null;
    }

    private void SetShaderBool(bool state)
    {
        float value = state ? 1f : 0f;

        // CAMBIO 2: Recorremos cada parte del cuerpo
        foreach (var renderer in _bodyPartsRenderer)
        {
            if (renderer == null) continue;

            // 1. Obtenemos el bloque actual de ESTE renderer (para no borrar otras propiedades si las hubiera)
            renderer.GetPropertyBlock(_propBlock);

            // 2. Modificamos solo el valor de daño
            _propBlock.SetFloat(_shaderDamageBool, value);

            // 3. Aplicamos el bloque de nuevo
            renderer.SetPropertyBlock(_propBlock);
        }
    }
}

