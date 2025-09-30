using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    [SerializeField] private BuffData _buff;
    public void Consume(PlayerHealth health, MovementController movement)
    {
        if (health != null && movement != null)
        {
            _buff.Apply(health, movement);
            Destroy(gameObject);
        }
    }
}
