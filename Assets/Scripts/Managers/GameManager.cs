using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : SingletonBase<GameManager>
{
    public enum GameStates
    {
        MainMenu,
        Tutorial,
        TutorialLevel,
        Playing,
        Paused,
        Upgrade,
        Results,
        Options,
        Credits,
        Win,
        Lose
    }
    public GameStates CurrentState { get; private set; }

    [Header("Debug")]
    [SerializeField] private string currentStateDebug; // store state to string for debugging
    [SerializeField] private string lastStateDebug; // store state to string for debugging
    public bool debug = false;

    [Header("References")]
    [SerializeField] private FloorManager floorManager;

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        ChangeState(GameStates.MainMenu);
    }

    public void OnPlayerDeath()
    {
        if(debug) Debug.Log("Player died — switching to Results screen");
        ChangeState(GameStates.Results);
    }

    public void OnBossDeath()
    {
        if(debug) Debug.Log("Boss died — switching to Win screen");
        ChangeState(GameStates.Win);
    }

    public void ChangeState(GameStates newState)
    {
        if (CurrentState == newState) return;

        GameStates previousState = CurrentState;
        lastStateDebug = previousState.ToString();
        CurrentState = newState;
        currentStateDebug = CurrentState.ToString();

        if(debug) Debug.Log($"Game state changed: {lastStateDebug} -> {currentStateDebug}");

        // Handle state exit logic
        OnStateExit(previousState);

        // Handle state enter logic
        OnStateEnter(newState);

        // Broadcast state change event
        GameEvents.GameStateChanged(newState);

        // Update music for this state
        ApplyMusicForState(newState);
    }

    private void OnStateExit(GameStates exitingState)
    {
        // Handle any cleanup or exit logic for states if needed
        switch (exitingState)
        {
            // Add exit logic here if needed in the future
            default:
                break;
        }
    }

    private void OnStateEnter(GameStates enteringState)
    {
        // Handle any initialization or enter logic for states if needed
        switch (enteringState)
        {
            // Add enter logic here if needed in the future
            default:
                break;
        }
    }

	private void ApplyMusicForState(GameStates state)
	{
		if (AudioManager.Instance == null) return;

		switch (state)
		{
			case GameStates.MainMenu:
				AudioManager.Instance.PlayMusicTrack("mainMenu");
				break;
			case GameStates.Upgrade:
				AudioManager.Instance.PlayMusicTrack("upgradeScreen");
				break;
			case GameStates.Win:
				AudioManager.Instance.PlayMusicTrack("winScreen");
				break;
			case GameStates.Playing:
				if (floorManager != null)
				{
					AudioManager.Instance.PlayGameplayForFloor(floorManager.CurrentFloor);
				}
				break;
			default:
				// leave currently playing track for other states or extend mapping as needed
				break;
		}
	}
}

