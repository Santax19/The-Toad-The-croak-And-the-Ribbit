using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PlayerHudController : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private Image _healthBar; // Slider de Unity (0-1 normalizado)
    [SerializeField] private Image _crosshair; // Imagen simple para la mira

    private PlayerHealth _healthSystem;

    public void Init(PlayerHealth healthSystem)
    {
        _healthSystem = healthSystem;
        _healthSystem.OnHealthChanged += UpdateHealthBar;
        _healthSystem.OnDeath += HandleDeath;

        UpdateHealthBar(_healthSystem.CurrentHealth, _healthSystem.MaxHealth);
    }

    private void UpdateHealthBar(int current, int max)
    {
        if (_healthBar != null)
            _healthBar.fillAmount = (float)current / max;
    }

    private void HandleDeath()
    {
        Debug.Log("Jugador muerto -> HUD podría mostrar pantalla de muerte.");
    }

    private void OnDestroy()
    {
        if (_healthSystem != null)
        {
            _healthSystem.OnHealthChanged -= UpdateHealthBar;
            _healthSystem.OnDeath -= HandleDeath;
        }
    }
}
