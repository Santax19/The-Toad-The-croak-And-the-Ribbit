using UnityEngine;

// Put this on the root (with the Motor & Driver)
public class RibbitAnimationBinder : MonoBehaviour
{
    void Awake()
    {
        var motor = GetComponent<RibbitThirdPersonMotor>();
        var driver = GetComponent<CharacterAnimationDriver>();
        if (motor != null && driver != null)
            motor.OnJumpEvent += driver.OnJump;
    }
}