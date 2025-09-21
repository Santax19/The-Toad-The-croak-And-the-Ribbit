using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Mouse Settings")]
    [SerializeField] private float _sensitivity = 100f; // sensibilidad del mouse
    [SerializeField] private Transform _playerBody; // referencia al transform del jugador

    private float _xRotation = 0f; // control de inclinación vertical

    private void Start()
    {
        // bloqueamos el cursor en el centro y lo ocultamos
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
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
