using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class Pickup : NetworkBehaviour
{
    [SerializeField] private BuffData _buff;

    private bool _consumedLocal = false;
    public void Consume(PlayerHealth health, MovementController movement)
    {
        if (Object == null || !Object.IsValid) return;
        if (_consumedLocal) return;

        var weaponManager = health.GetComponent<WeaponManager>();
        if (health != null && movement != null && weaponManager != null)
        {
            _consumedLocal = true;

            _buff.Apply(health, movement, weaponManager);
            Runner.Despawn(Object);
        }
    }
}
