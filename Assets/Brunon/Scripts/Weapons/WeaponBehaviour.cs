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

    public virtual void OnReload() { }
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
    }
}
