using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class DamageGrenade : BaseThrownGrenade
{
    [Header("Daño (Específico)")]
    [SerializeField] private float _explosionDamage = 100f;

    [Header("Damage Falloff")]
    [Tooltip("El % del radio que se considera 'epicentro' de daño 100%. (0.2 = 20%)")]
    [Range(0.01f, 1f)]
    [SerializeField] private float _innerRadiusPercent = 0.2f;

    [Tooltip("El % de daño mínimo que se hará en el borde exterior. (0.1 = 10%)")]
    [Range(0f, 1f)]
    [SerializeField] private float _minDamagePercent = 0.1f;
    protected override void ActivateEffect(NetworkRunner runner, Collision collision)
    {
        float innerRadius = explosionRadius * _innerRadiusPercent;
        float minDamage = _explosionDamage * _minDamagePercent;

        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (var hit in hits)
        {
            float distance = Vector3.Distance(transform.position, hit.transform.position);
            float damageToApply;

            if (distance <= innerRadius)
            {
                damageToApply = _explosionDamage;
            }
            else
            {
                float falloffT = Mathf.InverseLerp(explosionRadius, innerRadius, distance);
                damageToApply = Mathf.Lerp(minDamage, _explosionDamage, falloffT);
            }

            if (hit.TryGetComponent<PlayerHealth>(out var playerHealth))
            {
                // playerHealth.TakeDamage(damageToApply);
            }

        }

    }
    protected override void OnDrawGizmosSelected()
    {
        // Dibujar el radio exterior (heredado)
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawSphere(transform.position, explosionRadius);

        // Dibujar el radio interior (propio)
        Gizmos.color = new Color(1, 1, 0, 0.5f);
        Gizmos.DrawSphere(transform.position, explosionRadius * _innerRadiusPercent);
    }
}
