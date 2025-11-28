using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeShaderBehaviour : GrenadeBehaviour
{
    [Header("Referencias Visuales")]
    [SerializeField] private Renderer _handGrenadeRenderer;
    [SerializeField] private string _shaderPropertyName = "_IsActive"; // Verifica esto en ShaderGraph!

    private int _isActiveShaderID;
    private Material _materialInstance;

    public override void Spawned()
    {
        base.Spawned();

        if (_handGrenadeRenderer != null)
        {
            // Forzamos la creación de la instancia del material
            _materialInstance = _handGrenadeRenderer.material;
            _isActiveShaderID = Shader.PropertyToID(_shaderPropertyName);
        }
    }

    public override void Render()
    {
        base.Render();

        if (_materialInstance == null) return;

        // Valor objetivo
        float targetVal = IsActive ? 1f : 0f;

        // Escribimos el valor
        _materialInstance.SetFloat(_isActiveShaderID, targetVal);
    }
}
