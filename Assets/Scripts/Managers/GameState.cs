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
    public bool debug = false;

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
        // immediately reset player and start gameplay
        ResetPlayer();
        ChangeState(GameStates.Playing);
    }

    public void StartGameplay(bool fromDeath = false)
    {
        if (fromDeath && floorManager != null)
        {
            // Only reset to floor 1 if this is called from death/retry
            floorManager.ResetToFloor1();
        }

        // change state to playing
        ChangeState(GameStates.Playing);
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
        ResetPlayer();
    }

    private void ResetPlayer()
    {
        var playerGO = PlayerInstance.Instance?.gameObject;
        if (playerGO == null)
        {
            if(debug) Debug.LogError("PlayerInstance singleton not found!");
            return;
        }

        // reset health
        var health = playerGO.GetComponent<HealthSystem>();
        if (health != null && GameStates.Win == CurrentState)
            health.ResetToOriginalStats();

        if (health != null) health.ResetHealth();

        // reset position
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

        if(debug) Debug.Log($"Game state changed: {lastStateDebug} -> {currentStateDebug}");

        OnGameStateChanged?.Invoke(newState);
    }
}
