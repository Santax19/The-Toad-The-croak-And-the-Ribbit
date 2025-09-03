using UnityEngine;
using UnityEngine.InputSystem;

/// StarterAssets-like input wrapper to keep bindings familiar.
/// Use with a PlayerInput (Behavior: Send Messages).
public class RibbitInputs : MonoBehaviour
{
    [Header("Input Values")]
    public Vector2 move;
    public Vector2 look;
    public bool jump;
    public bool sprint;
    public bool toggleView;

    [Header("Cursor")]
    public bool cursorLocked = true;
    public bool cursorInputForLook = true;

    // PlayerInput message handlers
    public void OnMove(InputValue value)  { move = value.Get<Vector2>(); }
    public void OnLook(InputValue value)  { if (cursorInputForLook) look = value.Get<Vector2>(); }
    public void OnJump(InputValue value)  { jump = value.isPressed; }
    public void OnSprint(InputValue value){ sprint = value.isPressed; }
    public void OnToggleView(InputValue v){ toggleView = v.isPressed; }

    void OnApplicationFocus(bool hasFocus) => SetCursorState(cursorLocked);

    void SetCursorState(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}