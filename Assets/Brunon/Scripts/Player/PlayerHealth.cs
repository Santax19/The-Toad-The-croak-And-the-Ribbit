using Fusion;
using Fusion.Addons.Physics;
using System;
using UnityEngine;

public class PlayerHealth : NetworkBehaviour
{
    [SerializeField] private PlayerAnimatorManager _animatorManager;
    [SerializeField] private NetworkRigidbody3D _netRigidbody;
    [SerializeField] private Transform[] _spawnPoints;
    [Header("Health Settings")]
    [SerializeField] private int _maxHealth = 100;
    [SerializeField] private float _respawnTime = 1.3f;

    // ------- CAMPOS NETWORKED --------
    [Networked,OnChangedRender(nameof(OnHealthChangedNetworked))]
    public int CurrentHealth { get; set; }

    [Networked]
    public bool IsDead { get; set; }
    [Networked] private TickTimer _respawnTimer { get; set; }
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
        GameObject spawnRoot = GameObject.Find("SpawnPoint");
        if (spawnRoot) _spawnPoints = spawnRoot.GetComponentsInChildren<Transform>();
        if (Object.HasStateAuthority)
        {
            CurrentHealth = _maxHealth;
            IsDead = false;
        }
        // Actualiza UI inicial para este cliente local
        if (Object.HasInputAuthority)
            OnHealthChanged?.Invoke(CurrentHealth, _maxHealth);
    }
    public override void FixedUpdateNetwork()
    {
        // Lógica de Respawn (Solo el dueño en Shared)
        if (Object.HasStateAuthority)
        {
            if (IsDead && _respawnTimer.Expired(Runner))
            {
                Respawn();
            }
        }
    }
    private void Die()
    {
        IsDead = true;

        _respawnTimer = TickTimer.CreateFromSeconds(Runner, _respawnTime);
        RPC_PlayDeathEffect();
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
            Die();
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

    private void Respawn()
    {
        Debug.Log("Reviviendo...");

        // 1. Resetear Estado
        IsDead = false;
        CurrentHealth = _maxHealth;
        _respawnTimer = default;
        _animatorManager.RestoreDissolve();
        Vector3 respawnPos = Vector3.up * 2;
        // 2. Teletransportar
        if (_spawnPoints != null && _spawnPoints.Length > 0)
        {
            // Usamos Length en lugar de Length > 1 para evitar errores si solo hay 1
            int index = UnityEngine.Random.Range(0, _spawnPoints.Length);
            if (_spawnPoints[index] != null)
                respawnPos = _spawnPoints[index].position;
        }
        if (_netRigidbody != null)
        {
            try
            {
                _netRigidbody.Teleport(respawnPos);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Fallo Teleport seguro: {e.Message}. Usando Transform directo.");
                // Fallback para Shared Mode (Funciona porque somos StateAuthority)
                transform.position = respawnPos;
                if (TryGetComponent<Rigidbody>(out var rb)) rb.velocity = Vector3.zero;
            }
        }
        else
        {
            // Si no hay NetworkRB, movemos el transform (Fallback)
            transform.position = respawnPos;
        }

        // 3. Restaurar Armas (Munición)
        // Buscamos el WeaponManager en este mismo objeto
        if (TryGetComponent<WeaponManager>(out var weaponManager))
        {
            // Recargar todas las armas al máximo
            foreach (var weapon in weaponManager.ownedWeapons)
            {
                if (weapon != null)
                    weapon.RefillAmmo(weapon.WeaponData.magazineSize, weapon.WeaponData.reserveAmmo);
            }
            weaponManager.NotifyAmmoChanged();
        }
        OnHealthChangedNetworked();
    }
}

