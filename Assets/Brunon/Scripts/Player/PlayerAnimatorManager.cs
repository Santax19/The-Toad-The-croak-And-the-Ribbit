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

    [SerializeField] private string _shaderDissolveBool = "_IsDissolving";
    [SerializeField] private string _shaderDissolveFloat = "_DissolveAmount"; 
    [SerializeField] private float _dissolveDuration = 1.5f;

    private Coroutine _damageCoroutine;
    private Coroutine _dissolveCoroutine;
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
        ApplyDamageProperties(true);
        yield return new WaitForSeconds(1f);
        ApplyDamageProperties(false);
        _damageCoroutine = null;
    }
    public void TriggerDissolve()
    {
        if (_bodyPartsRenderer == null || _bodyPartsRenderer.Length == 0) return;

        // Si ya se está disolviendo, no hacemos nada (o podrías reiniciarlo)
        if (_dissolveCoroutine != null) return;

        _dissolveCoroutine = StartCoroutine(DissolveRoutine());
    }
    public void RestoreDissolve()
    {
        if (_bodyPartsRenderer == null || _bodyPartsRenderer.Length == 0) return;
        SetShaderFloatGlobal(_shaderDissolveFloat, 1f);
    }
    private IEnumerator DissolveRoutine()
    {
        float currentTime = 0f;

        // 1. Activamos el Bool para que el shader use la rama del Dissolve
        SetShaderBoolGlobal(_shaderDissolveBool, true);

        // 2. Hacemos el Lerp del valor de 0 a 1
        while (currentTime < _dissolveDuration)
        {
            currentTime += Time.deltaTime;
            float lerpValue = Mathf.Clamp01(currentTime / _dissolveDuration);
            float dissolveValue = 1f - lerpValue;
            // Aplicamos el valor flotante progresivo
            SetShaderFloatGlobal(_shaderDissolveFloat, dissolveValue);
            yield return null; // Esperar al siguiente frame
        }

        // 3. Aseguramos que termine en 1 exacto
        SetShaderFloatGlobal(_shaderDissolveFloat, 0f);

        // Opcional: Aquí podrías desactivar el GameObject si quieres
        // gameObject.SetActive(false); 
    }

    // ==============================================================================
    // HELPERS PARA PROPERTY BLOCKS
    // ==============================================================================

    // Helper específico para el daño (booleano simple)
    private void ApplyDamageProperties(bool state)
    {
        float value = state ? 1f : 0f;
        foreach (var renderer in _bodyPartsRenderer)
        {
            if (renderer == null) continue;
            renderer.GetPropertyBlock(_propBlock);
            _propBlock.SetFloat(_shaderDamageBool, value);
            renderer.SetPropertyBlock(_propBlock);
        }
    }
    private void SetShaderBoolGlobal(string propertyName, bool state)
    {
        float value = state ? 1f : 0f;
        SetShaderFloatGlobal(propertyName, value);
    }
    private void SetShaderFloatGlobal(string propertyName, float value)
    {
        foreach (var renderer in _bodyPartsRenderer)
        {
            if (renderer == null) continue;

            // Obtenemos el bloque actual para no sobrescribir el estado del daño si estuviera activo
            renderer.GetPropertyBlock(_propBlock);

            // Modificamos
            _propBlock.SetFloat(propertyName, value);

            // Aplicamos
            renderer.SetPropertyBlock(_propBlock);
        }
    }
}

