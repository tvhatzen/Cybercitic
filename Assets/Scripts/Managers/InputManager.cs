using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;
using System;

public class InputManager : MonoBehaviour, Input.IPlayerActions
{
    private Input input;
    void Awake()
    {
        try
        {
            input = new Input();
            input.player.SetCallbacks(this);
        }
        catch (Exception exception)
        {
            Debug.LogError($"Error initializing InputManager: {exception.Message}");
        }
    }

    public event Action skill1InputEvent;
    public event Action skill2InputEvent;
    public event Action skill3InputEvent;

    void Input.IPlayerActions.OnSkill1(InputAction.CallbackContext context)
    {
        if (context.started) skill1InputEvent?.Invoke();
    }

    void Input.IPlayerActions.OnSkill2(InputAction.CallbackContext context)
    {
        if (context.started) skill2InputEvent?.Invoke();
    }

    void Input.IPlayerActions.OnSkill3(InputAction.CallbackContext context)
    {
        if (context.started) skill3InputEvent?.Invoke();
    }

    void OnEnable()
    {
        if (input != null)
            input.player.Enable();
    }

    void OnDestroy()
    {
        if (input != null)
            input.player.Disable();
    }
}
