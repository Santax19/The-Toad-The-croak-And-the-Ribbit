using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int _maxHealth = 100;
    private int _currentHealth;
    public int CurrentHealth => _currentHealth;
    public int MaxHealth => _maxHealth;
    public bool IsDead => _currentHealth <= 0;

    // Eventos locales -> fáciles de convertir a eventos en red
    public event Action<int, int> OnHealthChanged; // vida actual, máxima
    public event Action OnDeath;

    private void Awake()
    {
        _currentHealth = _maxHealth;
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
    }

    public void TakeDamage(int damage)
    {
        if (IsDead) return;
        _currentHealth -= damage;
        _currentHealth = Mathf.Max(_currentHealth, 0);

        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);

        if (_currentHealth == 0)
            Die();
    }

    public void Heal(int amount)
    {
        if (IsDead) return;

        _currentHealth += amount;
        _currentHealth = Mathf.Min(_currentHealth, _maxHealth);

        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} ha muerto.");
        OnDeath?.Invoke();
        // acá más adelante: respawn, desactivar player, animaciones, etc.
    }
}
