using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorDetector : MonoBehaviour
{
    [HideInInspector] public bool IsGrounded = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ground"))
            IsGrounded = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Ground"))
            IsGrounded = false;
    }
}
