using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float lifeTime = 5f; // tiempo antes de destruirse sola
    public int damage = 10;     // daño que hace la bala

    private void Start()
    {
        Destroy(gameObject, lifeTime); // la bala se destruye sola después de un tiempo
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Si impacta contra algo con "EnemyHealth" (ejemplo)
        EnemyHealth enemy = collision.collider.GetComponent<EnemyHealth>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
        }

        // Destruimos la bala al chocar con cualquier cosa
        Destroy(gameObject);
    }
}

