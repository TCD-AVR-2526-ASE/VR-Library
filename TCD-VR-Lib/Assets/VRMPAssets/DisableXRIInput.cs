using UnityEngine;
using UnityEngine.InputSystem;

public class DisableXRIInput : MonoBehaviour
{
    public InputActionReference leftMove;
    public InputActionReference rightMove;
    public InputActionReference jump;

    public void DisableMovement()
    {
        leftMove.action.Disable();
        rightMove.action.Disable();
        jump.action.Disable();
    }

    public void EnableMovement()
    {
        leftMove.action.Enable();
        rightMove.action.Enable();
        jump.action.Enable();
    }
}

