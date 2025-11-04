using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeBehaviour : WeaponBehaviour
{
    [Header("Lógica de Granada")]
    [SerializeField] private GameObject thrownGrenadePrefab; // El prefab que se LANZA

    private bool isCooking = false;
    private float cookStartTime;
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

    public override void HandleInput(Camera playerCam, Transform firePoint)
    {
        // No hay recarga, no hay auto-recarga.

        // 1. Iniciar "cocinado" (LMB Presionado)
        if (Input.GetButtonDown("Fire1") && !isCooking && currentAmmo > 0)
        {
            isCooking = true;
            cookStartTime = Time.time;
            Debug.Log("Cocinando granada...");
        }

        // 2. Lanzar granada (LMB Suelto)
        if (Input.GetButtonUp("Fire1") && isCooking)
        {
            isCooking = false;

            if (TryConsumeAmmo(1)) // Usa una granada
            {
                ThrowGrenade(playerCam, firePoint);
                weaponManager.NotifyAmmoChanged();
            }
            if (currentAmmo == 0)
            {
                // Le decimos al manager que nos quite
                weaponManager.RemoveWeapon(this);
            }
        }

        // 3. Apuntado (RMB Hold)
        if (Input.GetButton("Fire2"))
        {
            // Lógica de trayectoria
        }

        // 4. Auto-explosión en mano
        if (isCooking && (Time.time - cookStartTime) >= _grenadeData.cookTime)
        {
            Debug.Log("¡BOOM! Explotó en la mano.");
            isCooking = false;
            TryConsumeAmmo(1);
            // Lógica de daño al jugador aquí
            weaponManager.NotifyAmmoChanged();
        }
    }

    private void ThrowGrenade(Camera playerCam, Transform firePoint)
    {
        Debug.Log("Lanzando granada");
        GameObject grenadeInstance = Instantiate(thrownGrenadePrefab, firePoint.position, playerCam.transform.rotation);

        float timeCooked = Time.time - cookStartTime;
        float remainingTime = _grenadeData.cookTime - timeCooked;

        BaseThrownGrenade thrownScript = grenadeInstance.GetComponent<BaseThrownGrenade>();
        if (thrownScript != null)
        {
            thrownScript.Initialize(remainingTime);
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
    public override void OnPrimaryFire(Camera playerCam, Transform firePoint)
    {
        // No hacemos nada aquí.
    }

    public override void OnSecondaryFire(Camera playerCam, Transform firePoint)
    {
        // No hacemos nada aquí.
    }
}
