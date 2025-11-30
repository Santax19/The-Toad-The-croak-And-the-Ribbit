using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public abstract class WeaponBehaviour : NetworkBehaviour
{
    protected WeaponManager weaponManager;

    [Header("Datos")]
    [SerializeField] protected WeaponData weaponData;
    [SerializeField] protected ParticlesMan gunParticles;

    protected int currentAmmo;
    protected int reserveAmmo;
    protected bool isReloading;
    protected float nextShootTime = 0f;
    private ChangeDetector _changes;
    [Networked] private int FireVisualCounter { get; set; }
    [Networked] protected TickTimer FireCooldown { get; set; }
    public WeaponData WeaponData => weaponData;
    public int CurrentAmmo => currentAmmo;
    public int ReserveAmmo => reserveAmmo;
    public bool IsReloading => isReloading;

    public void SetManager(WeaponManager manager)
    {
        weaponManager = manager;
    }

    // Inicializamos los valores de munición (solo una vez al inicio)
    public override void Spawned()
    {
        // Inicializamos datos locales
        currentAmmo = weaponData.magazineSize;
        reserveAmmo = weaponData.reserveAmmo;

        // Inicializamos el detector de cambios
        _changes = GetChangeDetector(ChangeDetector.Source.SimulationState);
    }

    public override void Render()
    {
        // Detectamos si hubo cambios en las variables de red desde el último frame
        foreach (var change in _changes.DetectChanges(this))
        {
            // Si el contador de disparos cambió...
            if (change == nameof(FireVisualCounter))
            {
                // ...Ejecutamos las partículas visualmente
                if (gunParticles != null)
                {
                    gunParticles.PlayShotParticles();
                }
            }
        }
    }
    public void CancelReload() => isReloading = false;

    // Métodos base
    public abstract void OnPrimaryFire(NetworkRunner runner, Camera playerCam, Transform firePoint);
    public abstract void OnSecondaryFire(Camera playerCam, Transform firePoint);

    public virtual void OnReload() 
    {
        if (weaponManager != null)
            weaponManager.StartReload(this);
    }
    protected void PlayShotParticles()
    {
        FireVisualCounter++;
    }


    public virtual IEnumerator ReloadRoutine()
    {
        if (isReloading || currentAmmo >= weaponData.magazineSize || reserveAmmo <= 0)
            yield break;
        var animManager = GetComponentInParent<PlayerAnimatorManager>();
        if (animManager != null) {animManager.SetTrigger("Reload");}
        isReloading = true;

        yield return new WaitForSeconds(weaponData.reloadTime);

        int bulletsToLoad = weaponData.magazineSize - currentAmmo;
        int bulletsToTake = Mathf.Min(bulletsToLoad, reserveAmmo);

        currentAmmo += bulletsToTake;
        reserveAmmo -= bulletsToTake;

        isReloading = false;
        weaponManager.NotifyAmmoChanged();
    }

    public virtual void OnEquip(Camera cam) { }

    public bool TryConsumeAmmo(int amount)
    {
        if (currentAmmo >= amount)
        {
            currentAmmo -= amount;
            return true;
        }
        return false;
    }

    public void RefillAmmo(int mag, int reserve)
    {
        currentAmmo = mag;
        reserveAmmo = reserve;
        if (weaponManager != null)
            weaponManager.NotifyAmmoChanged();
    }
    public virtual void HandleInput(NetworkRunner runner, NetworkInputData data, Camera playerCam, Transform firePoint)
    {
        if (Object == null || !Object.IsValid) return;
        if (IsReloading) return;

        if (CurrentAmmo <= 0)
        {
            if (ReserveAmmo > 0 && !IsReloading)
                OnReload();
            return;
        }

        // 3. Lógica de Disparo (Fire1)
        // --- Usa data.fire1 ---
        if (data.fire1 && FireCooldown.ExpiredOrNotRunning(Runner))
        {
            if (TryConsumeAmmo(1))
            {
                OnPrimaryFire(runner, playerCam, firePoint);

                // Configurar el nuevo tiempo de espera usando el Runner
                FireCooldown = TickTimer.CreateFromSeconds(Runner, WeaponData.fireRate);

                weaponManager.NotifyAmmoChanged();
                PlayShotParticles();
            }
        }

        // 4. Lógica de Acción Secundaria (Fire2)
        if (data.fire2)
        {
            OnSecondaryFire(playerCam, firePoint);
        }

        // 5. Lógica de Recarga Manual (R)
        if (data.reload)
        {
            OnReload();
        }

        // 6. Lógica de Apuntado (Aiming)
        float targetFOV = data.fire2 ? weaponData.aimFOV : weaponData.normalFOV;
        playerCam.fieldOfView = Mathf.Lerp(
            playerCam.fieldOfView,
            targetFOV,
            Time.deltaTime * weaponData.aimSpeed // Time.deltaTime está OK aquí
        );
    }
    // --- FIN NUEVA FUNCIÓN ---
}

