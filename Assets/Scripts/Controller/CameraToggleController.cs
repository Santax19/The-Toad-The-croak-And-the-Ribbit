using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

/// Coordinates TP/FP camera modes and look ownership (motor vs CinemachinePOV).
public class CameraToggleController : MonoBehaviour
{
    [Header("Refs")]
    public RibbitThirdPersonMotor motor;

    [Header("Cameras")]
    public CinemachineVirtualCamera thirdPersonCam;
    public CinemachineVirtualCamera firstPersonCam;

    [Header("Input")]
    public RibbitInputs ribbitInputs;            // If present, reads toggle from here
    public InputActionProperty toggleAction;     // Otherwise reads from this button
    public InputActionProperty lookActionForPOV; // For FP POV only

    [Header("First-person options")]
    public bool usePOVForFirstPerson = true;
    public Renderer[] headRenderersToHide;

    [Header("Priorities")]
    public int thirdPersonPriority = 11;
    public int firstPersonPriority  = 10;

    bool _fp;

    void OnEnable()
    {
        if (!ribbitInputs && toggleAction.action != null)
        {
            toggleAction.action.performed += OnToggle;
            toggleAction.action.Enable();
        }
    }
    void OnDisable()
    {
        if (!ribbitInputs && toggleAction.action != null)
        {
            toggleAction.action.performed -= OnToggle;
            toggleAction.action.Disable();
        }
    }

    void Update()
    {
        if (ribbitInputs && ribbitInputs.toggleView)
        {
            ribbitInputs.toggleView = false;
            Toggle();
        }
    }

    void OnToggle(InputAction.CallbackContext _) => Toggle();
    void Toggle() { _fp = !_fp; Apply(); }
    void Start() => Apply();

    void Apply()
    {
        if (thirdPersonCam) thirdPersonCam.Priority = _fp ? firstPersonPriority : thirdPersonPriority;
        if (firstPersonCam) firstPersonCam.Priority  = _fp ? thirdPersonPriority : firstPersonPriority;

        if (headRenderersToHide != null)
            foreach (var r in headRenderersToHide) if (r) r.enabled = !_fp;

        if (motor) motor.motorControlsLook = !_fp || !usePOVForFirstPerson;

        var pov = firstPersonCam ? firstPersonCam.GetComponent<CinemachinePOV>() : null;
        var provider = firstPersonCam ? firstPersonCam.GetComponent<CinemachineInputProvider>() : null;

        if (usePOVForFirstPerson && _fp)
        {
            if (!pov) pov = firstPersonCam.gameObject.AddComponent<CinemachinePOV>();
            if (!provider) provider = firstPersonCam.gameObject.AddComponent<CinemachineInputProvider>();
            provider.XYAxis = lookActionForPOV.reference;
            pov.enabled = true; provider.enabled = true;
        }
        else
        {
            if (provider) provider.enabled = false;
            if (pov) pov.enabled = false;
        }
    }
}