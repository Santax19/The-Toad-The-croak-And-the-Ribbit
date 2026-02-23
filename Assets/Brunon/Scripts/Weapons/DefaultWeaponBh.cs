using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;

public class DefaultWeaponBh : WeaponBehaviour
{
    [SerializeField] private NetworkObject bulletPrefab;
    [SerializeField] private GameObject dummyBulletPrefab;
    public override void OnPrimaryFire(NetworkRunner runner, Camera cam, Transform firePoint)
    {
        PlayShotParticles();
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Vector3 targetPoint = ray.GetPoint(weaponData.range);

        if (Physics.Raycast(ray, out RaycastHit hit, weaponData.range))
            targetPoint = hit.point;
        Quaternion lookRotation = Quaternion.LookRotation((targetPoint - firePoint.position).normalized);
        if (runner.IsServer)
        {
            var bulletObj = runner.Spawn(bulletPrefab, firePoint.position, lookRotation, Object.InputAuthority);
            // Inicializamos datos extra si hace falta
            if (bulletObj.TryGetComponent<Bullet>(out var bulletScript))
            {
                bulletScript.Initialize(lookRotation * Vector3.forward, weaponData.bulletSpeed, Object.InputAuthority);
            }
        }

        if (!runner.IsServer)
        {
            // Instancia un prefab que SOLO tenga MeshRenderer y TrailRenderer (sin NetworkObject, sin Collider)
            // O usa el mismo prefab pero quítale los componentes de red al instanciarlo (más sucio).
            if (bulletPrefab != null)
            {
                GameObject dummy = Instantiate(dummyBulletPrefab, firePoint.position, lookRotation);

                // Le damos velocidad visual
                if (dummy.TryGetComponent<Rigidbody>(out var rb))
                {
                    rb.velocity = firePoint.forward * weaponData.bulletSpeed;
                }

                Destroy(dummy, 1.2f); // Autodestruir en 2 segs
            }
        }
    }

    public override void OnSecondaryFire(Camera cam, Transform firePoint)
    {
        return;
    }
}