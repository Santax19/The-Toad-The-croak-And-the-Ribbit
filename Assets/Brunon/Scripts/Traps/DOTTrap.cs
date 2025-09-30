using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DOTTrap : Trap
{
    [SerializeField] private float _duration = 3f;
    [SerializeField] private float _tickInterval = 1f;

    private Coroutine _damageCoroutine;

    protected override void OnEnterTrap(PlayerHealth health, GameObject player)
    {
        if (_damageCoroutine != null) StopCoroutine(_damageCoroutine);
        _damageCoroutine = StartCoroutine(ApplyDamageOverTime(health));
    }

    protected override void OnExitTrap(PlayerHealth health, GameObject player)
    {
        // dejar que la corutina continúe sola
    }

    private IEnumerator ApplyDamageOverTime(PlayerHealth health)
    {
        float elapsed = 0f;
        while (elapsed < _duration && health != null && !health.IsDead)
        {
            health.TakeDamage(_damage);
            yield return new WaitForSeconds(_tickInterval);
            elapsed += _tickInterval;
        }
        _damageCoroutine = null;
    }
}
