using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Weapons/WeaponData")]
public class WeaponData : ScriptableObject
{
    [Header("General")]
    public string weaponName;
    public GameObject bulletPrefab;
    public Transform weaponPrefab; // opcional, para mostrar en la mano

    [Header("Stats")]
    public int damage = 10;
    public int headDamage = 20;
    public float fireRate = 0.25f;
    public float bulletSpeed = 50f;
    public float range = 100f;

    [Header("Ammo")]
    public int magazineSize = 30;
    public int reserveAmmo = 90;
    public float reloadTime = 2f;

    [Header("Aim")]
    public float normalFOV = 60f;
    public float aimFOV = 40f;
    public float aimSpeed = 10f; // qué tan rápido interpola el FOV
}
