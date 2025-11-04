using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public abstract class BaseThrownGrenade : MonoBehaviour
{
    [Header("Configuración Base")]
    [SerializeField] protected float explosionRadius = 5f; // Radio para el efecto
    [SerializeField] protected GameObject activationEffect; // Tu prefab de partículas/luz
    public void Initialize(float timeToExplode)
    {
        StartCoroutine(ActivationTimer(Mathf.Max(0.01f, timeToExplode)));
    }

    private IEnumerator ActivationTimer(float time)
    {
        yield return new WaitForSeconds(time);

        // 1. Llama al efecto específico de la clase hija
        ActivateEffect();

        // 2. Instanciar efectos visuales
        if (activationEffect != null)
        {
            GameObject effect = Instantiate(activationEffect, transform.position, Quaternion.identity);
            Destroy(effect, 2f); // Autolimpieza
        }

        // 3. Destruir la granada
        Destroy(gameObject);
    }
    protected abstract void ActivateEffect();
    protected virtual void OnCollisionEnter(Collision collision)
    {
        
    }
    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 1, 0, 0.5f);
        Gizmos.DrawSphere(transform.position, explosionRadius);
    }
}
