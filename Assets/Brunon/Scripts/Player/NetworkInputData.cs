using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct NetworkInputData : INetworkInput
{
    public Vector2 moveInput;   // El movimiento analógico se queda igual
    public NetworkButtons buttons;
    public float mouseX;
    public Vector3 aimDirection;
}
public enum MyButtons
{
    Fire1 = 0,
    Fire2 = 1,
    Jump = 2,
    Crouch = 3,
    Reload = 4,
    Grapple = 5,
    Alpha1 = 6,
    Alpha2 = 7,
    Alpha3 = 8,
    Alpha4 = 9
}