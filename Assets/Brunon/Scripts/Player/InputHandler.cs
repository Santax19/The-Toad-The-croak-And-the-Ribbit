using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        // Creamos una nueva instancia de nuestra estructura de datos
        NetworkInputData data = new()
        {
            // Leemos los inputs locales y los guardamos en la estructura
            moveInput = new Vector2(
                Input.GetAxisRaw("Horizontal"),
                Input.GetAxisRaw("Vertical")
            ),
            fire1 = Input.GetButton("Fire1"),
            fire2 = Input.GetButton("Fire2"),

            // Usamos GetKeyDown para acciones de una sola vez
            reload = Input.GetKeyDown(KeyCode.R),
            alpha1 = Input.GetKeyDown(KeyCode.Alpha1),
            alpha2 = Input.GetKeyDown(KeyCode.Alpha2),
            alpha3 = Input.GetKeyDown(KeyCode.Alpha3),
            alpha4 = Input.GetKeyDown(KeyCode.Alpha4),
            crouch = Input.GetKey(KeyCode.LeftShift),
            grapple = Input.GetKey(KeyCode.Space) // Listo para GrappleBruno
        };

        // Le entregamos la estructura de datos a Fusion
        input.Set(data);
    }
}
