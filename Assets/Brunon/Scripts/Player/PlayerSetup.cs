using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSetup : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private PlayerHudController _hudController;
    [SerializeField] private PlayerHealth _playerHealth;

    private void Awake()
    {
        if (_hudController != null && _playerHealth != null)
        {
            _hudController.Init(_playerHealth);
        }
        else
        {
            Debug.LogWarning("PlayerSetup: faltan referencias al HUD o PlayerHealth.");
        }
    }
}
