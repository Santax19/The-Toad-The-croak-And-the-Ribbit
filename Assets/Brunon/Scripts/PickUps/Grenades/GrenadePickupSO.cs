using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "GrenadePickup_Nombre", menuName = "Buffs/Grenade Pickup")]
public class GrenadePickupSO : BuffData
{
    [Header("Configuración de Granada")]
    [SerializeField] private GameObject grenadeWeaponPrefab; // El prefab del ARMA (la esfera que sostienes)
    [SerializeField] private int amountToAdd = 1;
    [SerializeField] private string grenadeWeaponName;
    [SerializeField] private int slotIndex = 3;
    public override void Apply(PlayerHealth health, MovementController movement, WeaponManager weaponManager)
    {
        // 1. Buscamos si el jugador ya tiene esta granada
        WeaponBehaviour existingGrenade = weaponManager.GetWeaponByName(grenadeWeaponName);

        if (existingGrenade == null)
        {
            // 2. Si no la tiene, la creamos y añadimos al slot 4 (o el siguiente)
            existingGrenade = weaponManager.AddWeaponToSlot(grenadeWeaponPrefab, slotIndex);
            if (existingGrenade == null) return; // Falló la creación
            existingGrenade.RefillAmmo(amountToAdd, 0);
        }
        else
        { 
            existingGrenade.RefillAmmo(existingGrenade.CurrentAmmo + amountToAdd, 0);
        }

        //Equipamos la granada
        int grenadeIndex = weaponManager.ownedWeapons.IndexOf(existingGrenade);
        if (grenadeIndex != -1)
            weaponManager.EquipWeapon(grenadeIndex);
    }
}
