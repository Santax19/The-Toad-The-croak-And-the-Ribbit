using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Buffs/JumpBuff")]
public class JumpBuff : BuffData
{
    public float jumpMultiplier = 1.5f;

    public override void Apply(PlayerHealth health, MovementController movement, WeaponManager weaponManager)
    {
        movement.ModifyMovement(1f, jumpMultiplier);
        if (duration > 0) movement.StartCoroutine(ResetAfterTime(movement));
    }

    private System.Collections.IEnumerator ResetAfterTime(MovementController movement)
    {
        yield return new WaitForSeconds(duration);
        movement.ResetMovement();
    }
}
