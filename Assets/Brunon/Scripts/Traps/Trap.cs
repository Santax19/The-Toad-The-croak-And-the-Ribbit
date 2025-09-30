using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Trap : MonoBehaviour
{
    [SerializeField] protected int _damage = 10;

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerHealth>(out var health))
        {
            OnEnterTrap(health, other.gameObject);
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        PlayerHealth health = other.GetComponent<PlayerHealth>();
        if (health != null)
        {
            OnExitTrap(health, other.gameObject);
        }
    }

    protected abstract void OnEnterTrap(PlayerHealth health, GameObject player);
    protected abstract void OnExitTrap(PlayerHealth health, GameObject player);
}
