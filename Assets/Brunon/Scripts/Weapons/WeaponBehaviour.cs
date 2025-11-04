using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WeaponBehaviour : MonoBehaviour
{
    protected WeaponManager weaponManager;

    [Header("Datos")]
    [SerializeField] protected WeaponData weaponData;
    [SerializeField] protected ParticlesMan gunParticles;

    protected int currentAmmo;
    protected int reserveAmmo;
    protected bool isReloading;
    protected float nextShootTime = 0f;

    public WeaponData WeaponData => weaponData;
    public int CurrentAmmo => currentAmmo;
    public int ReserveAmmo => reserveAmmo;
    public bool IsReloading => isReloading;

    public void SetManager(WeaponManager manager)
    {
        weaponManager = manager;
    }

    // Inicializamos los valores de munición (solo una vez al inicio)
    protected virtual void Awake()
    {
        currentAmmo = weaponData.magazineSize;
        reserveAmmo = weaponData.reserveAmmo;
    }

    public void CancelReload() => isReloading = false;

    // Métodos base
    public abstract void OnPrimaryFire(Camera playerCam, Transform firePoint);
    public abstract void OnSecondaryFire(Camera playerCam, Transform firePoint);

    public virtual void OnReload() 
    {
        if (weaponManager != null)
            weaponManager.StartReload(this);
    }
    protected void PlayShotParticles()
    {
        if (gunParticles != null)
            gunParticles.PlayShotParticles();
    }


    public virtual IEnumerator ReloadRoutine()
    {
        if (isReloading || currentAmmo >= weaponData.magazineSize || reserveAmmo <= 0)
            yield break;

        isReloading = true;
        Debug.Log($"{weaponData.weaponName} recargando...");

        yield return new WaitForSeconds(weaponData.reloadTime);

        int bulletsToLoad = weaponData.magazineSize - currentAmmo;
        int bulletsToTake = Mathf.Min(bulletsToLoad, reserveAmmo);

        currentAmmo += bulletsToTake;
        reserveAmmo -= bulletsToTake;

        isReloading = false;
        Debug.Log($"{weaponData.weaponName}: recarga completa ({currentAmmo}/{reserveAmmo})");
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
    public virtual void HandleInput(Camera playerCam, Transform firePoint)
    {
        // 1. Lógica de Recarga (si estamos recargando, no hacer nada más)
        if (IsReloading) return;

        // 2. Lógica de Auto-Recarga si no hay balas
        if (CurrentAmmo <= 0)
        {
            if (ReserveAmmo > 0 && !IsReloading)
                OnReload(); // Inicia la recarga
            return;
        }

        // 3. Lógica de Disparo (Fire1)
        if (Input.GetButton("Fire1") && Time.time >= nextShootTime)
        {
            if (TryConsumeAmmo(1))
            {
                OnPrimaryFire(playerCam, firePoint);
                nextShootTime = Time.time + WeaponData.fireRate;
                weaponManager.NotifyAmmoChanged();
            }
        }

        // 4. Lógica de Acción Secundaria (Fire2)
        if (Input.GetButtonDown("Fire2"))
        {
            OnSecondaryFire(playerCam, firePoint);
        }

        // 5. Lógica de Recarga Manual (R)
        if (Input.GetKeyDown(KeyCode.R))
        {
            OnReload();
        }

        // 6. Lógica de Apuntado (Aiming)
        float targetFOV = Input.GetButton("Fire2") ? weaponData.aimFOV : weaponData.normalFOV;
        playerCam.fieldOfView = Mathf.Lerp(
            playerCam.fieldOfView,
            targetFOV,
            Time.deltaTime * weaponData.aimSpeed
        );
    }
    // --- FIN NUEVA FUNCIÓN ---
}

