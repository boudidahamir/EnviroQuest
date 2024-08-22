using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{

    private Playeraction inputActions;

    private void Awake()
    {
        inputActions = new Playeraction();
        Cursor.visible = false;
    }

    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    public Vector2 GetPlayerMovement()
    {
        return inputActions.mvm.Movement.ReadValue<Vector2>();
    }

    public Vector2 GetMouseDelta()
    {
        return inputActions.mvm.Look.ReadValue<Vector2>();
    }

    public bool PlayerJump()
    {
        return inputActions.mvm.Jump.triggered;
    }

    public bool PlayerAccelaration()
    {
        return inputActions.mvm.Speed.IsPressed();
    }

    public bool PlayerPick()
    {
        return inputActions.pickup.pick.triggered;
    }
}
