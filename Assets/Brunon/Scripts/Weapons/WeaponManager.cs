using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class WeaponManager : NetworkBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Camera playerCam;
    [SerializeField] private Transform firePoint;
    [Header("Armas del jugador")]
    public List<WeaponBehaviour> ownedWeapons = new();

    private int currentWeaponIndex = -1;
    private WeaponBehaviour currentWeapon;
    private Coroutine reloadCoroutine;

    public event Action<int, int> OnAmmoChanged;
    [Networked] public NetworkBool IsAiming { get; set; }
    public override void Spawned()
    {
        // Esta es la nueva "Start()"
        // Solo el jugador local debe equipar el arma al inicio
        if (Object.HasInputAuthority)
        {
            foreach (var w in ownedWeapons)
                if (w != null) w.gameObject.SetActive(false);

            if (ownedWeapons.Count > 0)
                EquipWeapon(0);
        }
    }
    public void NetworkedWeaponUpdate(NetworkInputData data)
    {
        // Solo el jugador local (que tiene autoridad)
        // debe leer los inputs de disparo y switch
        if (!Object.HasInputAuthority)
            return;

        // Si no tenemos arma, no hacemos nada
        if (currentWeapon == null) return;
        if (Object.HasInputAuthority)
        {
            IsAiming = data.fire2;
        }
        // Pasamos los datos de red al arma actual
        currentWeapon.HandleInput(Runner, data, playerCam, firePoint);

        // Leemos los datos de switch de arma
        HandleWeaponSwitch(data);
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

    private void HandleWeaponSwitch(NetworkInputData data)
    {
        if (data.alpha1) EquipWeapon(0);
        if (data.alpha2) EquipWeapon(1);
        if (data.alpha3) EquipWeapon(2);
        if (data.alpha4) EquipWeapon(3);
    }

    public void EquipWeapon(int index)
    {
        if (index < 0 || index >= ownedWeapons.Count) return;
        if (index == currentWeaponIndex) return;
        if (ownedWeapons[index] == null) return;
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
    public void NotifyAmmoChanged()
    {
        if (currentWeapon == null) return;
        OnAmmoChanged?.Invoke(currentWeapon.CurrentAmmo, currentWeapon.ReserveAmmo);
    }
    public WeaponBehaviour AddWeaponToSlot(NetworkObject weaponPrefab, int slotIndex)
    {
        if (weaponPrefab == null) return null;

        // 0. Chequeo de seguridad
        if (slotIndex < 0 || slotIndex >= ownedWeapons.Count)
        {
            Debug.LogError($"Slot Index {slotIndex} fuera de rango. La lista 'Owned Weapons' solo tiene {ownedWeapons.Count} elementos.");
            return null;
        }

        // 1. Si ya tenemos un arma en ese slot (ej. una granada vieja), la destruimos
        if (ownedWeapons[slotIndex] != null)
        {
            Runner.Despawn(ownedWeapons[slotIndex].Object);
        }

        // 2. Creamos la instancia del arma
        // --- ARREGLO DE JERARQUÍA: Ahora es hijo de 'firePoint' ---
        NetworkObject weaponNO = Runner.Spawn(
            weaponPrefab,
            Vector3.zero,
            Quaternion.identity,
            Object.InputAuthority // Importante: Le damos autoridad al dueño de este WeaponManager
        ); // <-- HIJO DE firePoint
        weaponNO.transform.SetParent(firePoint, false);

        // Ajustes locales de posición (para que no quede flotando raro)
        weaponNO.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        if (weaponNO.TryGetComponent<WeaponBehaviour>(out var newWeapon))
        {
            ownedWeapons[slotIndex] = newWeapon;
            // Si quieres ocultarla inicialmente:
            newWeapon.gameObject.SetActive(false);

            return newWeapon;
        }
        return null;
    }
    public void RemoveWeapon(WeaponBehaviour weaponToRemove)
    {
        if (weaponToRemove == null) return;
        int weaponIndex = ownedWeapons.IndexOf(weaponToRemove);

        if (weaponIndex != -1)
        {
            if (weaponIndex == currentWeaponIndex)
            {
                EquipWeapon(0);
            }
            ownedWeapons[weaponIndex] = null;
            Destroy(weaponToRemove.gameObject);
        }
    }
    // Buscamos un arma por su nombre (Data)
    public WeaponBehaviour GetWeaponByName(string weaponName)
    {
        foreach (var weapon in ownedWeapons)
        {
            if (weapon != null && weapon.WeaponData.weaponName == weaponName)
                return weapon;
        }
        return null;
    }
}
