using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewGrenadeData", menuName = "Weapons/GrenadeData")]
public class GrenadeData : WeaponData
{
    [Header("Stats de Granada")]
    [Tooltip("Tiempo total de 'cocinado' antes de que explote en la mano")]
    public float cookTime = 5f;

    [Tooltip("Fuerza con la que se lanza la granada")]
    public float throwForce = 20f;
}
