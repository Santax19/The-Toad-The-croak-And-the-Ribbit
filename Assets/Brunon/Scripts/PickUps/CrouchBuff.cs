using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Buffs/CrouchBuff")]
public class CrouchBuff : BuffData
{
    public float crouchMultiplier = 2f;

    public override void Apply(PlayerHealth health, MovementController movement)
    {
        movement.ModifyMovement(crouchMultiplier, 1f);
        if (duration > 0) movement.StartCoroutine(ResetAfterTime(movement));
    }

    private System.Collections.IEnumerator ResetAfterTime(MovementController movement)
    {
        yield return new WaitForSeconds(duration);
        movement.ResetMovement();
    }
}
