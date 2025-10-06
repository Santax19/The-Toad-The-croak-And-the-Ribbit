using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultWeaponBh : WeaponBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 50f;

    private float normalFOV;
    private float aimFOV;
    private float aimLerpSpeed = 0.7f;

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
    }

    public override void OnSecondaryFire(Camera cam, Transform firePoint)
    {
        if (Mathf.Abs(cam.fieldOfView - aimFOV) < 0.1f)
            cam.fieldOfView = normalFOV;
        else
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, aimFOV, Time.deltaTime * aimLerpSpeed);
    }

    public override void OnEquip(Camera cam)
    {
        normalFOV = cam.fieldOfView;
    }
}
