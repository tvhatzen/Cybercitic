using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;
using System;

public class InputManager : MonoBehaviour, InputSystem_Actions.IPlayerActions
{
    private InputSystem_Actions input;

    [Header("DEBUG")]
    public bool debug = false;

    void Awake()
    {
        try
        {
            input = new InputSystem_Actions();
            input.Player.SetCallbacks(this);
            input.Player.Enable();
        }
        catch (Exception exception)
        {
            if(debug) Debug.LogError($"Error initializing InputManager: {exception.Message}");
        }
    }


    public event Action skill1InputEvent;
    public event Action skill2InputEvent;
    public event Action skill3InputEvent;
    public event Action interactInputEvent;

    public event Action<InputAction.CallbackContext> Skill1InputEvent;
    public event Action<InputAction.CallbackContext> Skill2InputEvent;
    public event Action<InputAction.CallbackContext> Skill3InputEvent;
    public event Action<InputAction.CallbackContext> InteractInputEvent;

    #region Callbacks

    public void OnSkill1(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            skill1InputEvent?.Invoke();
            if(debug) Debug.Log("Skill 1 started");
        }
    }

    public void OnSkill2(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            skill2InputEvent?.Invoke();
            if(debug) Debug.Log("Skill 2 started");
        }
    }

    public void OnSkill3(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            skill3InputEvent?.Invoke();
            if(debug) Debug.Log("Skill 3 started");
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if(debug) Debug.Log("On Interact");
    }

    #endregion

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
}
