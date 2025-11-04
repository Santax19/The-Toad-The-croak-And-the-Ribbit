using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultWeaponBh : WeaponBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 50f;

    public override void OnPrimaryFire(Camera cam, Transform firePoint)
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Vector3 targetPoint = ray.GetPoint(weaponData.range);

        if (Physics.Raycast(ray, out RaycastHit hit, weaponData.range))
            targetPoint = hit.point;

        Vector3 direction = (targetPoint - firePoint.position).normalized;
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(direction));
        if (bullet.TryGetComponent<Rigidbody>(out var rb))
            rb.velocity = direction * bulletSpeed;
        PlayShotParticles();
    }

    public override void OnSecondaryFire(Camera cam, Transform firePoint)
    {
        return;
    }
}