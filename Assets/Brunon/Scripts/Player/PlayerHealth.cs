using System;
using Fusion;
using UnityEngine;

public class PlayerHealth : NetworkBehaviour
{
    [SerializeField] private PlayerAnimatorManager _animatorManager;
    [Header("Health Settings")]
    [SerializeField] private int _maxHealth = 100;

    // ------- CAMPOS NETWORKED --------
    [Networked,OnChangedRender(nameof(OnHealthChangedNetworked))]
    public int CurrentHealth { get; set; }

    [Networked]
    public bool IsDead { get; set; }

    public int MaxHealth => _maxHealth;

    // ------- EVENTOS LOCALES --------
    public event Action<int, int> OnHealthChanged;
    public event Action OnDeath;


    // --------------------------------------------------------------------
    // INICIALIZACIÓN
    // --------------------------------------------------------------------
    public override void Spawned()
    {
        CurrentHealth = 100;

        if (Object.HasStateAuthority)
        {
            CurrentHealth = _maxHealth;
            IsDead = false;
        }
        // Actualiza UI inicial para este cliente local
        if (Object.HasInputAuthority)
            OnHealthChanged?.Invoke(CurrentHealth, _maxHealth);
    }


    // --------------------------------------------------------------------
    // MÉTODO PARA APLICAR DAÑO (RPC EN STATE AUTHORITY)
    // --------------------------------------------------------------------
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_TakeDamage(int damage, RpcInfo info = default)
    {
        if (!Object.HasStateAuthority) return;
        if (IsDead) return;

        // ► Validación básica
        damage = Mathf.Clamp(damage, 0, 200);
        Debug.Log("El jugador recibió daño");
        int previousHealth = CurrentHealth;
        CurrentHealth = Mathf.Max(0, CurrentHealth - damage);    
        if (CurrentHealth < previousHealth)
        {
            RPC_PlayDamageEffect();
        }
        if (CurrentHealth <= 0)
        {
            IsDead = true;
            HandleDeath(); // Lógica local del server
            RPC_PlayDeathEffect(); // Orden visual a todos
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_PlayDamageEffect()
    {
        // Esto se ejecuta en la máquina de TODOS los jugadores instantáneamente
        // al recibir el mensaje, sin esperar a interpolación de variables.
        if (_animatorManager != null)
        {
            _animatorManager.TriggerDamageVisuals();
        }
    }
    // --------------------------------------------------------------------
    // MÉTODO PARA HEAL (RPC)
    // --------------------------------------------------------------------
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_Heal(int amount, RpcInfo info = default)
    {
        if (!Object.HasStateAuthority) return;
        if (IsDead) return;

        amount = Mathf.Clamp(amount, 0, 200);

        CurrentHealth = Mathf.Min(CurrentHealth + amount, _maxHealth);
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_PlayDeathEffect()
    {
        if (_animatorManager != null) _animatorManager.TriggerDissolve();
    }

    // --------------------------------------------------------------------
    // CALLBACK de cambio de Health sincronizado
    // --------------------------------------------------------------------
    public void OnHealthChangedNetworked()
    {
        if (Object.HasInputAuthority)
        {
            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
        }
    }


    // --------------------------------------------------------------------
    // MANEJO DE MUERTE
    // --------------------------------------------------------------------
    private void HandleDeath()
    {
        Debug.Log($"{gameObject.name} ha muerto.");

        // evento local para animaciones, desactivar controles, etc.
        OnDeath?.Invoke();
    }
}

