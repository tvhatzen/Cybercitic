using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState : SingletonBase<GameState>
{
    public enum GameStates
    {
        MainMenu,
        Tutorial,
        Playing,
        Paused,
        Upgrade,
        Results,
        Win,
        Lose
    }
    public static event Action<GameStates> OnGameStateChanged;
    public GameStates CurrentState { get; private set; }

    [Header("Debug")]
    [SerializeField] private string currentStateDebug; // store state to string for debugging
    [SerializeField] private string lastStateDebug; // store state to string for debugging

    private bool tutorialShown = false;

    [Header("References")]
    [SerializeField] private FloorManager floorManager;

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        StartCoroutine(InitializeState());
    }

    private IEnumerator InitializeState()
    {
        yield return null; // wait one frame
        ChangeState(GameStates.MainMenu);
    }

    public void StartFromMainMenu()
    {
        if (!tutorialShown)
        {
            tutorialShown = true;
            ChangeState(GameStates.Tutorial);
        }
        else
        {
            StartGameplay();
        }
    
    }

    public void FinishTutorial()
    {
        // reset floor and player
        if (floorManager != null) floorManager.ResetToFloor1();

        // immediately reset player and start gameplay
        ResetPlayer();
        ChangeState(GameStates.Playing);
    
    
    }

    public void StartGameplay(bool fromDeath = false)
    {
        // prevent tutorial if respawning from death
        if (fromDeath)
            tutorialShown = true;

        if (floorManager != null)
            floorManager.ResetToFloor1(); // also respawns player

        // change state to playing
        ChangeState(GameStates.Playing);
    
    }

    public void OnPlayerDeath()
    {
        Debug.Log("Player died — switching to Results screen");

        // force game state to Results
        ChangeState(GameStates.Results);
    
    }

    public void OnBossDeath()
    {
        Debug.Log("Boss died — switching to Win screen");

        ChangeState(GameStates.Win);
    }

    private void ResetPlayer()
    {
        var playerGO = PlayerInstance.Instance?.gameObject;
        if (playerGO == null)
        {
            Debug.LogError("PlayerInstance singleton not found!");
            return;
        }

        // Reset health
        var health = playerGO.GetComponent<HealthSystem>();
        if (health != null) health.ResetHealth();

        // Reset position
        var movement = playerGO.GetComponent<PlayerMovement>();
        if (movement != null && floorManager != null)
            movement.ResetToSpawn(floorManager.playerSpawnPoint);

    }


    public void ChangeState(GameStates newState)
    {
        if (CurrentState == newState) return;

        lastStateDebug = CurrentState.ToString();
        CurrentState = newState;
        currentStateDebug = CurrentState.ToString();

        Debug.Log($"Game state changed: {lastStateDebug} -> {currentStateDebug}");

        OnGameStateChanged?.Invoke(newState);
    }
}
