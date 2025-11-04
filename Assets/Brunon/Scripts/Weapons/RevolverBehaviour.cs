using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RevolverBehaviour : WeaponBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 60f;
    [SerializeField] private float burstDelay = 0.1f;

    public override void OnPrimaryFire(Camera cam, Transform firePoint)
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Vector3 targetPoint = ray.GetPoint(weaponData.range);
        if (Physics.Raycast(ray, out RaycastHit hit, weaponData.range))
            targetPoint = hit.point;

        Vector3 dir = (targetPoint - firePoint.position).normalized;
        var bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(dir));
        if (bullet.TryGetComponent<Rigidbody>(out var rb))
            rb.velocity = dir * bulletSpeed;
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
                OnPrimaryFire(cam, firePoint);
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
    public override void HandleInput(Camera playerCam, Transform firePoint)
    {
        if (IsReloading) return;

        if (CurrentAmmo <= 0)
        {
            if (ReserveAmmo > 0 && !IsReloading)
                OnReload();
            return;
        }
        if (Input.GetButton("Fire1") && Time.time >= nextShootTime)
        {
            if (TryConsumeAmmo(1))
            {
                OnPrimaryFire(playerCam, firePoint);
                nextShootTime = Time.time + WeaponData.fireRate;
                weaponManager.NotifyAmmoChanged();
            }
        }

        if (Input.GetButtonDown("Fire2"))
        {
            if (Time.time >= nextShootTime)
            {
                OnSecondaryFire(playerCam, firePoint);
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            OnReload();
        }

    }
}
