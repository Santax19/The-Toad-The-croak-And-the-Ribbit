using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class CameraController : NetworkBehaviour
{
    [Header("Mouse Settings")]
    [SerializeField] private float _sensitivity = 100f; // sensibilidad del mouse
    [SerializeField] private Transform _cameraTransform; // referencia al transform del jugador

    private float _xRotation = 0f; // control de inclinación vertical
    [SerializeField] private Transform _tongueDetector;
    private Camera _cameraComponent;

    public override void Spawned()
    {
        _cameraComponent = GetComponentInChildren<Camera>(true);

        if (_cameraTransform != null)
        {
            _cameraComponent = _cameraTransform.GetComponent<Camera>();
        }

        if (_cameraComponent == null)
        {
            Debug.LogError("CameraController (en el Player) no pudo encontrar el componente Camera en su _cameraTransform. ¡Asegúrate de asignarlo en el prefab!");
            return;
        }
        //  Solo activa la camara si este jugador es local
        if (Object.HasInputAuthority)
        {
            _cameraComponent.gameObject.SetActive(true);
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            _cameraComponent.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (Object == null) {return;}
        if (!Object.HasInputAuthority)
            return;
        // input del mouse
        float mouseX = Input.GetAxis("Mouse X") * _sensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * _sensitivity * Time.deltaTime;

        // rotación vertical (cámara local)
        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -80f, 80f);

        // Aplicamos la rotación vertical SÓLO al transform de la CÁMARA
        _cameraTransform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        if (_tongueDetector != null) {_tongueDetector.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);}
        transform.Rotate(Vector3.up * mouseX);
    }
}
