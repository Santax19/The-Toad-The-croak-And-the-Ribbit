using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class DefaultWeaponBh : WeaponBehaviour
{
    [SerializeField] private NetworkObject bulletPrefab;

    public override void OnPrimaryFire(NetworkRunner runner, Camera cam, Transform firePoint)
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Vector3 targetPoint = ray.GetPoint(weaponData.range);

        if (Physics.Raycast(ray, out RaycastHit hit, weaponData.range))
            targetPoint = hit.point;

        Vector3 direction = (targetPoint - firePoint.position).normalized;
        NetworkObject bullet = runner.Spawn(
            bulletPrefab,
            firePoint.position,
            Quaternion.LookRotation(direction)
        );
        PlayShotParticles();
    }

    public override void OnSecondaryFire(Camera cam, Transform firePoint)
    {
        return;
    }
}