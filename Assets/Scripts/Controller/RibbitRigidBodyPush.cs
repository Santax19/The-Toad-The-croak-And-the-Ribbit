using UnityEngine;

/// Minimal rigidbody push when the CharacterController hits a dynamic RB.
[RequireComponent(typeof(CharacterController))]
public class RibbitRigidBodyPush : MonoBehaviour
{
    public float pushPower = 2.0f;

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;
        if (body == null || body.isKinematic) return;
        if (hit.moveDirection.y < -0.3f) return;

        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);
        body.AddForce(pushDir * pushPower, ForceMode.Impulse);
    }
}