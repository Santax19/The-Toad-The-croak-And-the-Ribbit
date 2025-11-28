using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class PlayerHudController : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private Image _healthBar; // Slider de Unity (0-1 normalizado)
    [SerializeField] private Image _crosshair; // Imagen simple para la mira
    [SerializeField] private TMP_Text _ammoText;

    [SerializeField] private PlayerHealth _healthSystem;
    [SerializeField] private WeaponManager _weaponManager;

    public void Init(PlayerHealth healthSystem)
    {
        _healthSystem = healthSystem;
        _healthSystem.OnHealthChanged += UpdateHealthBar;
        _healthSystem.OnDeath += HandleDeath;

        UpdateHealthBar(_healthSystem.CurrentHealth, _healthSystem.MaxHealth);
    }
    public void InitWeaponHud(WeaponManager weaponManager)
    {
        _weaponManager = weaponManager;
        _weaponManager.OnAmmoChanged += UpdateAmmoText;
    }
    private void UpdateHealthBar(int current, int max)
    {
        if (_healthBar != null)
            _healthBar.fillAmount = (float)current / max;
    }
    private void UpdateAmmoText(int current, int reserve)
    {
        if (_ammoText != null)
            _ammoText.text = $"{current} / {reserve}";
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
