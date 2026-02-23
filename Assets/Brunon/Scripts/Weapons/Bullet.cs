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

    public PlayerRef Owner;
    public override void Spawned()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.velocity = transform.forward * speed;
        // destrucción automática en red
        _lifeTimer = TickTimer.CreateFromSeconds(Runner, lifeTime);
    }

    public void Initialize(Vector3 direction, float speed, PlayerRef owner)
    {
        Owner = owner;
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
        if (!Object.HasStateAuthority) return;


        var health = collision.collider.GetComponentInParent<PlayerHealth>();

        if (health != null)
        {
            // Evitar fuego amigo / suicidio
            if (health.Object.InputAuthority == Owner)
            {
                return;
            }
            health.RPC_TakeDamage(damage);
        }

        // Destruir bala si es válida
        if (Object.IsValid)
        {
            Runner.Despawn(Object);
        }

    }
}
