using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
[RequireComponent(typeof(Rigidbody))]
public class Bullet : NetworkBehaviour
{
    [SerializeField] private float lifeTime = 5f;
    [SerializeField] private int damage = 10;
    [SerializeField] private float speed = 50f;

    private Rigidbody _rb;
    [Networked] private TickTimer _lifeTimer { get; set; }
    public override void Spawned()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.velocity = transform.forward * speed;
        // destrucción automática en red
        _lifeTimer = TickTimer.CreateFromSeconds(Runner, lifeTime);
    }

    public void Initialize(Vector3 direction, float speed)
    {
        _rb = GetComponent<Rigidbody>();
        _rb.velocity = direction * speed;
    }
    public override void FixedUpdateNetwork()
    {
        // Si el timer de red se acabó, despawneamos la bala
        if (_lifeTimer.Expired(Runner))
        {
            Runner.Despawn(Object);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Si impacta contra algo con "EnemyHealth" (ejemplo)
        EnemyHealth enemy = collision.collider.GetComponent<EnemyHealth>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
        }

        if (Object != null && Object.IsValid)
        {
            Runner.Despawn(Object);
        }
    }
}
