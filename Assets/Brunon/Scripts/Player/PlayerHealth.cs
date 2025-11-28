using System;
using Fusion;
using UnityEngine;

public class PlayerHealth : NetworkBehaviour
{
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
        CurrentHealth = Mathf.Max(0, CurrentHealth - damage);

        if (CurrentHealth <= 0)
        {
            IsDead = true;
            HandleDeath();
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


    // --------------------------------------------------------------------
    // CALLBACK de cambio de Health sincronizado
    // --------------------------------------------------------------------
    public void OnHealthChangedNetworked()
    {

        // UI local
        if (Object.HasInputAuthority)
        {
            // Usamos CurrentHealth y MaxHealth directamente
            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
        }

        // verificar muerte
        if (CurrentHealth <= 0 && !IsDead)
        {
            IsDead = true;
            HandleDeath();
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

        // Respawn lo maneja la StateAuthority desde afuera
        // o podés poner lógica acá si querés.
    }
}

