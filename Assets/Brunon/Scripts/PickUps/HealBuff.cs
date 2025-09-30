using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Buffs/HealBuff")]
public class HealBuff : BuffData
{
    public int healAmount = 20;

    public override void Apply(PlayerHealth health, MovementController movement)
    {
        health.Heal(healAmount);
    }
}

