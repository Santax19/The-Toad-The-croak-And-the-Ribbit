using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeBehaviour : WeaponBehaviour
{
    [Header("Lógica de Granada")]
    [SerializeField] private NetworkObject thrownGrenadePrefab; // El prefab que se LANZA

    [Networked] private NetworkBool _isCooking { get; set; } = false;
    [Networked] private float _cookStartTime { get; set; } = 0;

    // Variables locales para detectar "click"
    private bool _wasFire1Pressed = false;

    private GrenadeData _grenadeData;
    protected override void Awake()
    {
        _grenadeData = weaponData as GrenadeData;
        if (_grenadeData == null)
        {
            Debug.LogError($"¡Error en {gameObject.name}! GrenadeBehaviour necesita un 'GrenadeData' SO, no un 'WeaponData' normal. Asigna el SO correcto en el prefab.");
        }
        currentAmmo = 0;
        reserveAmmo = 0;
    }

    public override void HandleInput(NetworkRunner runner, NetworkInputData data, Camera playerCam, Transform firePoint)
    {
        // Detectamos los eventos "Down" y "Up" desde el estado de red
        bool fire1Pressed = data.fire1 && !_wasFire1Pressed;
        bool fire1Released = !data.fire1 && _wasFire1Pressed;

        // 1. Iniciar "cocinado" (LMB Presionado)
        if (fire1Pressed && !_isCooking && currentAmmo > 0)
        {
            _isCooking = true;
            _cookStartTime = runner.SimulationTime; // <-- Usamos el tiempo de red
            Debug.Log("Cocinando granada...");
        }

        // 2. Lanzar granada (LMB Suelto)
        if (fire1Released && _isCooking)
        {
            _isCooking = false;

            if (TryConsumeAmmo(1))
            {
                ThrowGrenade(runner, playerCam, firePoint);
                weaponManager.NotifyAmmoChanged();
            }
            if (currentAmmo == 0)
            {
                weaponManager.RemoveWeapon(this);
            }
        }

        // 3. Apuntado (RMB Hold)
        if (data.fire2) // <-- Lee de 'data'
        {
            // Lógica de trayectoria
        }

        // 4. Auto-explosión en mano
        if (_isCooking && (runner.SimulationTime - _cookStartTime) >= _grenadeData.cookTime) // <-- Usamos el tiempo de red
        {
            Debug.Log("¡BOOM! Explotó en la mano.");
            _isCooking = false;
            TryConsumeAmmo(1);
            // Lógica de daño al jugador aquí
            weaponManager.NotifyAmmoChanged();
        }

        // 5. Actualizamos el estado "anterior" de fire1
        _wasFire1Pressed = data.fire1;
    }

    private void ThrowGrenade(NetworkRunner runner, Camera playerCam, Transform firePoint)
    {
        Debug.Log("Lanzando granada");

        // --- ¡ARREGLO! Usa 'runner' (minúscula) ---
        NetworkObject grenadeInstance = runner.Spawn(
            thrownGrenadePrefab,
            firePoint.position,
            playerCam.transform.rotation
        );

        // --- ¡ARREGLO! Usa 'runner' (minúscula) ---
        float timeCooked = runner.SimulationTime - _cookStartTime;
        float remainingTime = _grenadeData.cookTime - timeCooked;

        BaseThrownGrenade thrownScript = grenadeInstance.GetComponent<BaseThrownGrenade>(); ;
        if (thrownScript != null)
        {
            // ¡Le pasamos el runner a la granada lanzada!
            thrownScript.Initialize(runner, remainingTime);
        }

        Rigidbody rb = grenadeInstance.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(playerCam.transform.forward * _grenadeData.throwForce, ForceMode.VelocityChange);
        }
    }

    public override void OnReload()
    {
        // Las granadas no se recargan
    }

    public override IEnumerator ReloadRoutine()
    {
        yield break;
    }
    public override void OnPrimaryFire(NetworkRunner runner, Camera playerCam, Transform firePoint)
    {
        // No hacemos nada aquí.
    }

    public override void OnSecondaryFire(Camera playerCam, Transform firePoint)
    {
        // No hacemos nada aquí.
    }
}
