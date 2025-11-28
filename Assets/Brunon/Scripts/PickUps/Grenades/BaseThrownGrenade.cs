using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

[RequireComponent(typeof(Rigidbody))]
public abstract class BaseThrownGrenade : NetworkBehaviour
{
    [Header("Configuración Base")]
    [SerializeField] protected float explosionRadius = 5f; // Radio para el efecto
    [SerializeField] protected ParticlesMan _particlesManager; // Tu prefab de partículas/luz
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
        ActivateEffect(runner, collision);

        // 2. LÓGICA DE VISUALES (BURST)
        if (_particlesManager != null)
        {
            // A. "Salvamos" las partículas separándolas de la granada
            // Si no hacemos esto, al despawnear la granada, las partículas mueren instantáneamente.
            _particlesManager.transform.SetParent(null);

            // B. Reproducimos la explosión
            _particlesManager.PlayShotParticles();

            // C. Programamos que el objeto de partículas se borre solo (localmente) en 3 segundos
            // Como ya no es hijo de un objeto de red, usamos Destroy normal.
            Destroy(_particlesManager.gameObject, 3f);
        }

        // 3. Despawnear la granada física
        if (Object != null)
        {
            runner.Despawn(Object);
        }
        else
        {
            Destroy(gameObject);
        }
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
