using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickyTrap : Trap
{
    [SerializeField] private float _multiplier = 0.5f;

    protected override void OnEnterTrap(PlayerHealth health, GameObject player)
    {
        if (player.TryGetComponent<MovementController>(out var movement))
        {
            movement.ModifyMovement(_multiplier, _multiplier);
        }
    }

    protected override void OnExitTrap(PlayerHealth health, GameObject player)
    {
        if (player.TryGetComponent<MovementController>(out var movement))
        {
            movement.ResetMovement();
        }
    }
}
