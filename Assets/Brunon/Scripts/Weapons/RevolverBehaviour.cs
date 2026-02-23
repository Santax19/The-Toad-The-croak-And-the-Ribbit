using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class RevolverBehaviour : WeaponBehaviour
{
    [SerializeField] private NetworkObject bulletPrefab;
    [SerializeField] private float burstDelay = 0.1f;
    private bool _wasFire2Pressed = false;
    [Networked] private NetworkButtons _prevButtons { get; set; }
    public override void OnPrimaryFire(NetworkRunner runner, Camera cam, Transform firePoint)
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Vector3 targetPoint = ray.GetPoint(weaponData.range);
        if (Physics.Raycast(ray, out RaycastHit hit, weaponData.range))
            targetPoint = hit.point;

        Vector3 dir = (targetPoint - firePoint.position).normalized;
        if (runner.IsServer)
        {
            runner.Spawn(bulletPrefab, firePoint.position, firePoint.rotation, Object.InputAuthority);
        }
        PlayShotParticles();
    }

    public override void OnSecondaryFire(Camera cam, Transform firePoint)
    {
        if (weaponManager == null) return;
        MonoBehaviour host = weaponManager;
        host.StartCoroutine(FireBurst(cam, firePoint));
    }

    private IEnumerator FireBurst(Camera cam, Transform firePoint)
    {
        int shotsToShoot = CurrentAmmo;

        for (int i = 0; i < shotsToShoot; i++)
        {
            if (TryConsumeAmmo(1))
            {
                if (weaponManager != null)
                    OnPrimaryFire(weaponManager.Runner, cam, firePoint);
                PlayShotParticles();
                weaponManager.NotifyAmmoChanged();
                yield return new WaitForSeconds(burstDelay);            
            }
            else
                break;
        }
    }

    public override void OnEquip(Camera cam)
    {
        return;
    }
    public override void HandleInput(NetworkRunner runner, NetworkInputData data, Camera playerCam, Transform firePoint)
    {
        if (IsReloading) return;

        if (CurrentAmmo <= 0)
        {
            if (ReserveAmmo > 0 && !IsReloading)
                OnReload();
            return;
        }

        // 1. Disparo (lee de 'data')
        if (data.buttons.WasPressed(_prevButtons, MyButtons.Fire1) && Time.time >= nextShootTime)
        {
            if (TryConsumeAmmo(1))
            {
                OnPrimaryFire(runner,playerCam, firePoint);
                nextShootTime = Time.time + WeaponData.fireRate;
                weaponManager.NotifyAmmoChanged();
            }
        }

        // 2. Ráfaga (detecta el "click" desde 'data')
        bool fire2Down = data.buttons.WasPressed(_prevButtons, MyButtons.Fire2) && !_wasFire2Pressed;
        if (fire2Down && Time.time >= nextShootTime)
        {
            OnSecondaryFire(playerCam, firePoint);
        }

        // 3. Recarga (lee de 'data')
        if (data.buttons.WasPressed(_prevButtons, MyButtons.Fire1))
        {
            OnReload();
        }

        // 4. Actualizamos el estado "anterior" de fire2
        _wasFire2Pressed = data.buttons.WasPressed(_prevButtons, MyButtons.Fire2);
    }
}
