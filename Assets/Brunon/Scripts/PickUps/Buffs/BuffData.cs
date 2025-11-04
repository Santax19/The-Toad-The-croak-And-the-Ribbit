using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BuffData : ScriptableObject
{
    public string buffName;
    public Color pickupColor; // para debug/visual en escena
    public float duration = 0f; // 0 = instantáneo

    public abstract void Apply(PlayerHealth health, MovementController movement, WeaponManager weaponManager);
}
