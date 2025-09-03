using System.Collections;
using System.Collections.Generic;
using Fusion;
using Fusion.Addons.Physics;
using UnityEngine;

public class Player : NetworkBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // if (!HasStateAuthority)
        //     return;

        // _horizontalInput = Input.GetAxis("Horizontal");
    }

    public override void FixedUpdateNetwork()
    {
        // Movement(_horizontalInput);
    }

    // usage
    void Movement(float xAxis)
    {
        // if (xAxis != 0)
        // {
        //     transform.right = Vector3.right * Mathf.Sign(xAxis);

        //     _rb.Rigidbody.velocity += Vector3.right * xAxis * _moveSpeed;
        // }
        // else
        // {
        // }
    }
}
