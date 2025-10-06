using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Camera playerCam;
    [SerializeField] private Transform firePoint;

    [Header("Armas del jugador")]
    [SerializeField] private List<WeaponBehaviour> ownedWeapons = new();

    private int currentWeaponIndex = -1;
    private WeaponBehaviour currentWeapon;
    private Coroutine reloadCoroutine;
    private float nextShootTime = 0f;

    public event Action<int, int> OnAmmoChanged;
    private void Start()
    {
        foreach (var w in ownedWeapons)
            if (w != null) w.gameObject.SetActive(false);

        if (ownedWeapons.Count > 0)
            EquipWeapon(0);
    }

    private void Update()
    {
        if (currentWeapon == null) return;

        HandleShooting();
        HandleAiming();
        HandleWeaponSwitch();
    }

    private void HandleShooting()
    {
        if (currentWeapon.IsReloading) return;

        // no disparar si no hay balas
        if (currentWeapon.CurrentAmmo <= 0)
        {
            if (currentWeapon.ReserveAmmo > 0 && !currentWeapon.IsReloading)
                StartReload(currentWeapon);
            return;
        }

        // Disparo principal
        if (Input.GetButton("Fire1") && Time.time >= nextShootTime)
        {
            if (currentWeapon.TryConsumeAmmo(1))
            {
                currentWeapon.OnPrimaryFire(playerCam, firePoint);
                nextShootTime = Time.time + currentWeapon.WeaponData.fireRate;
                NotifyAmmoChanged();
            }
        }

        // Acción secundaria (click derecho)
        if (Input.GetButtonDown("Fire2"))
            currentWeapon.OnSecondaryFire(playerCam, firePoint);

        // Recarga manual
        if (Input.GetKeyDown(KeyCode.R))
            StartReload(currentWeapon);
    }

    private void HandleAiming()
    {
        var data = currentWeapon.WeaponData;
        float targetFOV = Input.GetButton("Fire2") ? data.aimFOV : data.normalFOV;

        playerCam.fieldOfView = Mathf.Lerp(
            playerCam.fieldOfView,
            targetFOV,
            Time.deltaTime * data.aimSpeed
        );
    }

    public void StartReload(WeaponBehaviour weapon)
    {
        if (reloadCoroutine != null)
            StopCoroutine(reloadCoroutine);

        reloadCoroutine = StartCoroutine(ReloadAndNotify(weapon));
    }

    private IEnumerator ReloadAndNotify(WeaponBehaviour weapon)
    {
        yield return weapon.ReloadRoutine();
        NotifyAmmoChanged(); //al terminar la recarga
    }

    private void HandleWeaponSwitch()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) EquipWeapon(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) EquipWeapon(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) EquipWeapon(2);
    }

    private void EquipWeapon(int index)
    {
        if (index < 0 || index >= ownedWeapons.Count) return;
        if (index == currentWeaponIndex) return;

        // Cancelar recarga actual
        if (reloadCoroutine != null)
        {
            StopCoroutine(reloadCoroutine);
            reloadCoroutine = null;
        }

        if (currentWeapon != null)
        {
            currentWeapon.CancelReload();
            currentWeapon.gameObject.SetActive(false);
        }

        currentWeaponIndex = index;
        currentWeapon = ownedWeapons[index];
        currentWeapon.gameObject.SetActive(true);
        currentWeapon.SetManager(this);
        currentWeapon.OnEquip(playerCam);
        NotifyAmmoChanged();

        Debug.Log($"Equipado: {currentWeapon.WeaponData.weaponName} ({currentWeapon.CurrentAmmo}/{currentWeapon.ReserveAmmo})");
    }
    private void NotifyAmmoChanged()
    {
        if (currentWeapon == null) return;
        OnAmmoChanged?.Invoke(currentWeapon.CurrentAmmo, currentWeapon.ReserveAmmo);
    }
    public void AddWeapon(WeaponBehaviour newWeapon)
    {
        if (!ownedWeapons.Contains(newWeapon))
        {
            ownedWeapons.Add(newWeapon);
            newWeapon.gameObject.SetActive(false);
        }
    }
}
