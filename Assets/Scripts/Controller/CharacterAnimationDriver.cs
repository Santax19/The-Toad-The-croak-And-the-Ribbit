using UnityEngine;

/// Decoupled animator driver; reads CharacterController only.
[RequireComponent(typeof(CharacterController))]
public class CharacterAnimationDriver : MonoBehaviour
{
    public Animator animator;
    public string speedParam    = "Speed";
    public string groundedParam = "Grounded";
    public string freeFallParam = "FreeFall";
    public string jumpTrigger   = "Jump";

    CharacterController _cc;

    void Awake() => _cc = GetComponent<CharacterController>();

    void Update()
    {
        if (!animator) return;
        bool grounded = _cc.isGrounded;
        float planarSpeed = new Vector2(_cc.velocity.x, _cc.velocity.z).magnitude;
        bool freeFall = !grounded && _cc.velocity.y < -0.1f;
        animator.SetFloat(speedParam, planarSpeed);
        animator.SetBool(groundedParam, grounded);
        animator.SetBool(freeFallParam, freeFall);
    }

    public void OnJump() { if (animator) animator.SetTrigger(jumpTrigger); }
}