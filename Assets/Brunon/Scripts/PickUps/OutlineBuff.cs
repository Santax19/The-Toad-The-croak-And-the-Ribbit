using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class OutLineBuff : MonoBehaviour
{
    [SerializeField] private Material outlineMat;

    private Renderer _renderer;
    private Material[] _originalMats;
    private bool _outlined;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _originalMats = _renderer.materials;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Tongue"))
            EnableOutline(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Tongue"))
            EnableOutline(false);
    }

    private void EnableOutline(bool enable)
    {
        if (_renderer == null || outlineMat == null)
            return;

        if (enable && !_outlined)
        {
            // agregamos el outline como segundo material
            var newMats = new Material[_originalMats.Length + 1];
            _originalMats.CopyTo(newMats, 0);
            newMats[_originalMats.Length] = outlineMat;
            _renderer.materials = newMats;
            _outlined = true;
        }
        else if (!enable && _outlined)
        {
            // restauramos materiales originales
            _renderer.materials = _originalMats;
            _outlined = false;
        }
    }
}
