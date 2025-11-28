using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class PlayerSetup : NetworkBehaviour
{
    [Header("Referencias")]
    [SerializeField] private PlayerHudController _hudController;
    [SerializeField] private PlayerHealth _playerHealth;

    public override void Spawned()
    {
        // Ahora es seguro leer propiedades [Networked] porque Fusion ya inicializó este objeto.
        if (_hudController != null && _playerHealth != null)
        {
            // Init lee CurrentHealth, y ahora sí funcionará
            _hudController.Init(_playerHealth);
            _hudController.InitWeaponHud(GetComponent<WeaponManager>());
        }
        else
        {
            Debug.LogWarning("PlayerSetup: faltan referencias al HUD o PlayerHealth.");
        }
    }
}
