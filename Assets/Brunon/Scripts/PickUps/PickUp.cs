using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    [SerializeField] private BuffData _buff;
    public void Consume(PlayerHealth health, MovementController movement)
    {
        var weaponManager = health.GetComponent<WeaponManager>();
        if (health != null && movement != null && weaponManager != null)
        {
            _buff.Apply(health, movement, weaponManager);
            Destroy(gameObject);
        }
    }
}
