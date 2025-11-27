using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;    
public class CamOclussion : NetworkBehaviour
{
    [Header("Referencias")]
    [Tooltip("La cámara principal del jugador")]
    [SerializeField] private Camera _camera;
    //[SerializeField] private AudioListener _audioListener;

    [Header("Modelos")]
    [Tooltip("El objeto raíz de tus brazos FP (lo que ves tú)")]
    [SerializeField] private GameObject _firstPersonRoot;

    [Tooltip("El objeto raíz de tu cuerpo TP (lo que ven los demás)")]
    [SerializeField] private GameObject _thirdPersonRoot;

    public override void Spawned()
    {
        // Caso 1: Soy YO (El dueño de este jugador)
        if (Object.HasInputAuthority)
        {
            // 1. Activar Cámara y Audio
            if (_camera) _camera.enabled = true;
            //if (_audioListener) _audioListener.enabled = true;

            // 2. Configurar qué ve la cámara (Culling Mask)
            if (_camera)
            {
                _camera.cullingMask = ~(1 << LayerMask.NameToLayer("LocalBody"));
            }

            // 3. Asignar capas a los objetos
            // Ponemos los brazos en la capa "FirstPerson" (para verlos)
            SetLayerRecursively(_firstPersonRoot, LayerMask.NameToLayer("FirstPerson"));

            // Ponemos el cuerpo en "LocalBody" (para que la cámara lo ignore y no nos tape la visión)
            SetLayerRecursively(_thirdPersonRoot, LayerMask.NameToLayer("LocalBody"));

            // Aseguramos que los brazos estén activos
            _firstPersonRoot.SetActive(true);
        }
        // Caso 2: Es OTRO jugador (Remoto)
        else
        {
            // 1. Desactivar su cámara y audio (no queremos ver por sus ojos)
            if (_camera) _camera.enabled = false;
            //if (_audioListener) _audioListener.enabled = false;

            // 2. El cuerpo lo dejamos en "Default" para poder verlo
            SetLayerRecursively(_thirdPersonRoot, LayerMask.NameToLayer("Default"));

            // 3. Desactivamos sus brazos FP (no necesitamos ver sus brazos flotando)
            _firstPersonRoot.SetActive(false);
        }
    }

    // Función auxiliar para cambiar la capa de un objeto y todos sus hijos
    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null) return;

        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}
