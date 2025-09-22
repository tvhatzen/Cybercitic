using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStsteMachine : MonoBehaviour
{
    // manager for state machine
    // turn menus on/off for different states the player decides

    public enum GameState
    {
        MainMenu_state,
        Gameplay_state,
        PauseMenu_state,
        GameOver_state
    }
    public GameState currentState { get; private set; }
    [SerializeField] private string currentStateDebug; // store state to string for debugging
    [SerializeField] private string lastStateDebug; // store state to string for debugging

    private void Start()
    {
        ChangeState(GameState.MainMenu_state);
    }
    public void ChangeState(GameState newState)
    {
        lastStateDebug = currentState.ToString(); // store current state as last state
        currentState = newState;// set new state to change to (passed in as parameter)

        HandleStateChange(newState); // Initiate state change
        currentStateDebug = currentState.ToString(); // Store the new state as the current state
    }
    private void Update()
    {
        if (currentState == GameState.MainMenu_state && Input.GetKeyDown(KeyCode.Space)) // MAIN MENU -> GAMEPLAY
        {
            ChangeState(GameState.Gameplay_state);
        }

        if (Input.GetKeyDown(KeyCode.Escape)) // GAMEPLAY -> PAUSE -> GAMEPLAY
        {
            // check if in gameplay 
            if (currentState == GameState.Gameplay_state)
            {
                ChangeState(GameState.PauseMenu_state); // switch from gameplay to pause menu
            }
            else if (currentState == GameState.PauseMenu_state)
            {
                ChangeState(GameState.Gameplay_state); // switch from pause menu to gameplay
            }
        }
    }
    private void HandleStateChange(GameState state)
    {
        switch (state)
        { // switching between game states

            case GameState.MainMenu_state:
                Debug.Log("Switched to Main Menu State!");
                // Instructions for state here...
                Time.timeScale = 1f;
                break;
            case GameState.Gameplay_state:
                Debug.Log("Switched to Gameplay State!");
                // Instructions for state here...
                Time.timeScale = 1f;
                break;
            case GameState.PauseMenu_state:
                Debug.Log("Switched to Pause Menu State!");
                // Instructions for state here...
                Time.timeScale = 0f;
                break;
            case GameState.GameOver_state:
                Debug.Log("Switched to Game Over State!");
                // Instructions for state here...
                Time.timeScale = 1f;
                break;
        }
    }
}
