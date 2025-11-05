using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class CameraController : NetworkBehaviour
{
    [Header("Mouse Settings")]
    [SerializeField] private float _sensitivity = 100f; // sensibilidad del mouse
    [SerializeField] private Transform _playerBody; // referencia al transform del jugador

    private float _xRotation = 0f; // control de inclinación vertical

    private Camera _camera;

    public override void Spawned()
    {
        _camera = GetComponentInChildren<Camera>(true);

        //  Solo activa la camara si este jugador es local
        if (Object.HasInputAuthority)
        {
            _camera.gameObject.SetActive(true);
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            _camera.gameObject.SetActive(false);
        }
    }

    private void Update()
    {

        if (!Object.HasInputAuthority)
            return;
        // input del mouse
        float mouseX = Input.GetAxis("Mouse X") * _sensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * _sensitivity * Time.deltaTime;

        // rotación vertical (cámara local)
        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -80f, 80f); // limitamos el ángulo

        transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);

        // rotación horizontal (cuerpo del jugador)
        _playerBody.Rotate(Vector3.up * mouseX);
    }
}
