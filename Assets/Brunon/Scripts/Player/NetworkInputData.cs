using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct NetworkInputData : INetworkInput
{
    public Vector2 moveInput;
    public NetworkBool crouch;
    public NetworkBool grapple;

    public NetworkBool fire1;    // Disparo principal
    public NetworkBool fire2;    // Disparo secundario / Apuntar
    public NetworkBool reload;

    public NetworkBool alpha1;
    public NetworkBool alpha2;
    public NetworkBool alpha3;
    public NetworkBool alpha4;
}
