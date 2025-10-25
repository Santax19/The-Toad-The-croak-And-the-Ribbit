using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FullScreenFXMan : MonoBehaviour
{
    [Header("Materiales de efectos")]
    [SerializeField] private Material damageMat;
    [SerializeField] private Material slowMat;
    [SerializeField] private Material speedMat;

    private PlayerHealth playerHealth;

    private void Start()
    {
        // Vinculamos la reacción al daño
        playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth != null)
            playerHealth.OnHealthChanged += UpdateDamageEffect;
        else
            Debug.LogWarning("FullScreenFXMan: No se encontró PlayerHealth en la escena.");

        // Aseguramos que todos arranquen desactivados
        if (damageMat) damageMat.SetFloat("_Active", 0);
        if (slowMat) slowMat.SetFloat("_Active", 0);
        if (speedMat) speedMat.SetFloat("_Active", 0);
    }

    private void UpdateDamageEffect(int current, int max)
    {
        if (damageMat == null) return;

        float healthRatio = (float)current / max;
        float vignetteValue = Mathf.Lerp(1f, 5f, healthRatio);
        damageMat.SetFloat("_VignettePower", vignetteValue);

        // activamos el efecto solo si hay daño visible
        damageMat.SetFloat("_Active", healthRatio < 0.99f ? 1f : 0f);
    }

    public void ApplySlowEffect(bool active)
    {
        if (slowMat == null) return;
        slowMat.SetFloat("_Active", active ? 1f : 0f);
    }

    public void ApplySpeedEffect(bool active)
    {
        if (speedMat == null) return;
        speedMat.SetFloat("_Active", active ? 1f : 0f);
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged -= UpdateDamageEffect;
    }
}
