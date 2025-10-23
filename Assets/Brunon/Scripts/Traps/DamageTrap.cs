using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageTrap : Trap
{
    protected override void OnEnterTrap(PlayerHealth health, GameObject player)
    {
        health.TakeDamage(_damage);
    }

    protected override void OnExitTrap(PlayerHealth health, GameObject player)
    {
        return;
    }
}
