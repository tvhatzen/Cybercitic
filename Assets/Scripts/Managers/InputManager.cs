using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;
using System;

public class InputManager : MonoBehaviour, InputSystem_Actions.IPlayerActions
{
    private InputSystem_Actions input;
    void Awake()
    {
        try
        {
            input = new InputSystem_Actions();
            input.Player.SetCallbacks(this);
        }
        catch (Exception exception)
        {
            Debug.LogError($"Error initializing InputManager: {exception.Message}");
        }
    }

    public event Action skill1InputEvent;
    public event Action skill2InputEvent;
    public event Action skill3InputEvent;

    void InputSystem_Actions.IPlayerActions.OnSkill1(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            skill1InputEvent?.Invoke();
            Debug.Log("Skill 1 started");
        }
    }

    void InputSystem_Actions.IPlayerActions.OnSkill2(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            skill2InputEvent?.Invoke();
            Debug.Log("Skill 2 started");
        }
    }

    void InputSystem_Actions.IPlayerActions.OnSkill3(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            skill3InputEvent?.Invoke();
            Debug.Log("Skill 3 started");
        }
    }

    void OnEnable()
    {
        if (input != null)
            input.Player.Enable();
    }

    void OnDestroy()
    {
        if (input != null)
            input.Player.Disable();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        Debug.Log("On Move");
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        Debug.Log("On Attack");
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        Debug.Log("On Interact");
    }

    public void OnSelect(InputAction.CallbackContext context)
    {
        throw new NotImplementedException();
    }
}
