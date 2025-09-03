using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class RibbitThirdPersonMotor : MonoBehaviour
{
    [Header("StarterAssets-like input (optional)")]
    public RibbitInputs ribbitInputs; // or your StarterAssetsInputs-equivalent

    [Header("Direct Input (optional if using RibbitInputs)")]
    public InputActionProperty moveAction, lookAction, jumpAction, sprintAction;

    [Header("Camera")]
    public Transform cameraTransform;  // auto-assigns Camera.main if null
    public Transform cameraTarget;     // Cinemachine pivot

    [Header("Movement")]
    public float moveSpeed = 2.0f;
    public float sprintSpeed = 5.335f;
    public float rotationSmoothTime = 0.12f;
    public float speedChangeRate = 10.0f;

    [Header("Jump / Gravity")]
    public float jumpHeight = 1.1f;
    public float gravity = -25.0f;
    public float jumpTimeout = 0.1f;
    public float fallTimeout = 0.08f;

    [Header("Grounding")]
    public float groundedOffset = -0.2f;
    public float groundedRadius = 0.4f;
    public LayerMask groundLayers = ~0;
    

    [Header("Look Control")]
    public bool motorControlsLook = true;  // TP: true. FP+POV: false
    public float lookSensitivity = 1.0f;
    public bool strafeMode = true;         // face camera yaw

    [Header("Debug")]
    public bool debugLogs = false;
    public bool debugGizmos = true;

    public bool Grounded { get; private set; }
    public float CurrentSpeed { get; private set; }

    CharacterController _cc;
    float _verticalVelocity;
    readonly float _terminalVelocity = 53f;
    float _targetRotation, _rotationVelocity;
    float _jumpTimeoutDelta, _fallTimeoutDelta;
    bool _prevJumpHeld;  // for edge detection
    bool _prevGrounded;

    // Simple hook other scripts can subscribe to (e.g., CharacterAnimationDriver)
    public System.Action OnJumpEvent;

    void Awake()
    {
        _cc = GetComponent<CharacterController>();
        _jumpTimeoutDelta = jumpTimeout;
        _fallTimeoutDelta = fallTimeout;
        if (!ribbitInputs) ribbitInputs = GetComponent<RibbitInputs>(); // optional
        if (!cameraTransform && Camera.main) cameraTransform = Camera.main.transform;
    }

    void OnEnable()
    {
        if (!ribbitInputs)
        {
            moveAction.action?.Enable();
            lookAction.action?.Enable();
            jumpAction.action?.Enable();
            sprintAction.action?.Enable();
        }
    }
    void OnDisable()
    {
        if (!ribbitInputs)
        {
            moveAction.action?.Disable();
            lookAction.action?.Disable();
            jumpAction.action?.Disable();
            sprintAction.action?.Disable();
        }
    }

    void Update()
    {
        GroundedCheck();
        HandleJumpAndGravity();
        HandleMoveAndRotate();
        if (motorControlsLook) UpdateCameraTargetYawPitch();
    }

    // ---------- INPUT READS ----------
    Vector2 ReadMove() =>
        ribbitInputs ? ribbitInputs.move :
        (moveAction.action != null ? moveAction.action.ReadValue<Vector2>() : Vector2.zero);

    Vector2 ReadLook() =>
        ribbitInputs ? ribbitInputs.look :
        (lookAction.action != null ? lookAction.action.ReadValue<Vector2>() : Vector2.zero);

    bool JumpPressedThisFrame()
    {
        if (ribbitInputs)
        {
            bool held = ribbitInputs.jump;
            bool pressedNow = !_prevJumpHeld && held; // rising edge
            _prevJumpHeld = held;
            return pressedNow;
        }
        else
        {
            return jumpAction.action != null && jumpAction.action.triggered; // Input System edge
        }
    }

    bool ReadSprintHeld() =>
        ribbitInputs ? ribbitInputs.sprint :
        (sprintAction.action != null && sprintAction.action.IsPressed());

    // ---------- GROUNDED / JUMP / GRAV ----------
    void GroundedCheck()
    {
        // Use both CC.isGrounded and a small sphere check for robustness
        bool ccGrounded = _cc.isGrounded;
        Vector3 pos = transform.position + Vector3.up * groundedOffset;
        bool sphereGrounded = Physics.CheckSphere(pos, groundedRadius, groundLayers, QueryTriggerInteraction.Ignore);

        Grounded = ccGrounded || sphereGrounded;

        if (Grounded && _verticalVelocity < 0f) _verticalVelocity = -2f;

        if (debugLogs && Grounded != _prevGrounded)
        {
            Debug.Log($"[Motor] Grounded -> {Grounded}  (cc:{ccGrounded}, sphere:{sphereGrounded})", this);
        }
        _prevGrounded = Grounded;
    }

    void HandleJumpAndGravity()
    {
        if (Grounded)
        {
            _fallTimeoutDelta = fallTimeout;

            // Consume jumps only on rising edge to prevent auto-bunny-hop on landing
            if (_jumpTimeoutDelta <= 0f && JumpPressedThisFrame())
            {
                _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                if (debugLogs) Debug.Log($"[Motor] JUMP! vVel={_verticalVelocity:F2}", this);
                OnJumpEvent?.Invoke();
                _jumpTimeoutDelta = jumpTimeout; // briefly lock out a re-press
            }

            if (_jumpTimeoutDelta > 0f) _jumpTimeoutDelta -= Time.deltaTime;

            if (_verticalVelocity < 0f)
                _verticalVelocity = -2f; // small stick-to-ground
        }
        else
        {
            _jumpTimeoutDelta = jumpTimeout;
            if (_fallTimeoutDelta > 0f) _fallTimeoutDelta -= Time.deltaTime;
        }

        if (_verticalVelocity < _terminalVelocity)
            _verticalVelocity += gravity * Time.deltaTime;
    }

    // ---------- MOVE / ROT ----------
    void HandleMoveAndRotate()
    {
        Vector2 input = ReadMove();
        bool sprinting = ReadSprintHeld();

        float targetSpeed = (input.sqrMagnitude > 0.01f) ? (sprinting ? sprintSpeed : moveSpeed) : 0f;
        float currentHorizontalSpeed = new Vector3(_cc.velocity.x, 0f, _cc.velocity.z).magnitude;
        if (Mathf.Abs(currentHorizontalSpeed - targetSpeed) > 0.1f)
        {
            CurrentSpeed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed, Time.deltaTime * speedChangeRate);
            CurrentSpeed = Mathf.Round(CurrentSpeed * 1000f) / 1000f;
        }
        else
        {
            CurrentSpeed = targetSpeed;
        }

        Vector3 camFwd = cameraTransform ? Vector3.Scale(cameraTransform.forward, new Vector3(1,0,1)).normalized : transform.forward;
        Vector3 camRight = cameraTransform ? cameraTransform.right : transform.right;
        Vector3 moveWorld = (camRight * input.x + camFwd * input.y).normalized;

        if (strafeMode)
        {
            float yaw = cameraTransform ? cameraTransform.eulerAngles.y : transform.eulerAngles.y;
            _targetRotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, yaw, ref _rotationVelocity, rotationSmoothTime);
            transform.rotation = Quaternion.Euler(0f, _targetRotation, 0f);
        }
        else if (moveWorld.sqrMagnitude > 0f)
        {
            float targetRot = Mathf.Atan2(moveWorld.x, moveWorld.z) * Mathf.Rad2Deg;
            _targetRotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRot, ref _rotationVelocity, rotationSmoothTime);
            transform.rotation = Quaternion.Euler(0f, _targetRotation, 0f);
        }

        Vector3 velocity = moveWorld * CurrentSpeed + Vector3.up * _verticalVelocity;
        _cc.Move(velocity * Time.deltaTime);
    }

    void UpdateCameraTargetYawPitch()
    {
        if (!cameraTarget) return;
        Vector2 delta = ReadLook() * lookSensitivity * Time.deltaTime;
        cameraTarget.Rotate(Vector3.up, delta.x, Space.World);
        cameraTarget.Rotate(Vector3.right, -delta.y, Space.Self);
    }

    // ---------- DEBUG DRAW ----------
    void OnDrawGizmosSelected()
    {
        if (!debugGizmos) return;
        Gizmos.color = Color.yellow;
        Vector3 pos = transform.position + Vector3.up * groundedOffset;
        Gizmos.DrawWireSphere(pos, groundedRadius);
    }
}
