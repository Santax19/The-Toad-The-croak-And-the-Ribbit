using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class Bullet : NetworkBehaviour
{
    [SerializeField] private float lifeTime = 5f;
    [SerializeField] private int damage = 10;
    [SerializeField] private float speed = 50f;

    private Rigidbody _rb;

    public override void Spawned()
    {
        _rb = GetComponent<Rigidbody>();

        // destrucción automática en red
        Runner.Despawn(Object);
    }

    public void Initialize(Vector3 direction, float speed)
    {
        _rb = GetComponent<Rigidbody>();
        _rb.velocity = direction * speed;
    }


    private void OnCollisionEnter(Collision collision)
    {
        // Si impacta contra algo con "EnemyHealth" (ejemplo)
        EnemyHealth enemy = collision.collider.GetComponent<EnemyHealth>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
        }

        if(Object!= null && Object.IsValid)
        {
            Runner.Despawn(Object);
        }
    }
}

