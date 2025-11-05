using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

[RequireComponent(typeof(Rigidbody))]
public abstract class BaseThrownGrenade : NetworkBehaviour
{
    [Header("Configuración Base")]
    [SerializeField] protected float explosionRadius = 5f; // Radio para el efecto
    [SerializeField] protected GameObject activationEffect; // Tu prefab de partículas/luz
    public void Initialize(NetworkRunner runner, float timeToExplode)
    {
        StartCoroutine(ActivationTimer(runner, Mathf.Max(0.01f, timeToExplode)));
    }

    private IEnumerator ActivationTimer(NetworkRunner runner, float time)
    {
        yield return new WaitForSeconds(time);
        // Le pasamos el runner a Explode
        Explode(runner, null);
    }

    protected virtual void OnTimerFinished()
    {
        // Esta función ya no es necesaria
    }

    protected virtual void Explode(NetworkRunner runner, Collision collision)
    {
        // ¡Le pasamos el runner a ActivateEffect!
        ActivateEffect(runner, collision);

        if (activationEffect != null)
        {
            GameObject effect = Instantiate(activationEffect, transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }

        // --- ¡MODIFICADO! ---
        // Usamos Despawn para destruir objetos de red
        if (Object != null) // Chequeo de seguridad
            runner.Despawn(Object);
        else
            Destroy(gameObject);
        // --- FIN MODIFICADO ---
    }

    // --- ¡MODIFICADO! ---
    protected abstract void ActivateEffect(NetworkRunner runner, Collision collision);

    protected virtual void OnCollisionEnter(Collision collision)
    {
        // (Lógica de rebote o nada)
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 1, 0, 0.5f);
        Gizmos.DrawSphere(transform.position, explosionRadius);
    }
}
