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

    private bool tutorialShown = false;
    private bool inTutorialLevel = false;

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

    public void StartFromMainMenu()
    {
        // reset all upgrades for a fresh start from main menu
        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.ResetAllUpgrades();
            if(debug) Debug.Log("All upgrades reset for fresh start from main menu");
        }
        
        // reset run stats for a fresh start
        if (RunStatsTracker.Instance != null)
        {
            RunStatsTracker.Instance.ResetStats();
            if(debug) Debug.Log("Run stats reset for fresh start from main menu");
        }
        
        // reset all skills for a fresh start
        if (PlayerSkills.Instance != null)
        {
            PlayerSkills.Instance.ResetAllSkills();
            if(debug) Debug.Log("All skills reset for fresh start from main menu");
        }
        
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
            // only reset to floor 1 if this is called from death/retry
            floorManager.ResetToFloor1();
        }
        else if (!fromDeath && floorManager != null)
        {
            // reset to floor 1 for a fresh start from main menu
            floorManager.ResetToFloor1();
            if(debug) Debug.Log("Floor reset to 1 for fresh start from main menu");
        }

        // Re-unlock any equipped skills when starting gameplay
        // This ensures skills are available even after reset
        if (PlayerSkills.Instance != null)
        {
            PlayerSkills.Instance.ReunlockEquippedSkills();
            if(debug) Debug.Log("Re-unlocked equipped skills for new gameplay");
        }
        
        // Re-apply all upgrades based on their current levels
        // This ensures stats are correctly updated when starting gameplay
        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.ReapplyAllUpgrades();
            if(debug) Debug.Log("Re-applied all upgrades for new gameplay");
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
        
        // reset all upgrades for a completely new run after winning
        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.ResetAllUpgrades();
            if(debug) Debug.Log("All upgrades reset for new run after win");
        }
        
        // reset run stats for a fresh start
        if (RunStatsTracker.Instance != null)
        {
            RunStatsTracker.Instance.ResetStats();
            if(debug) Debug.Log("Run stats reset for new run after win");
        }
        
        // reset all skills for a fresh start
        if (PlayerSkills.Instance != null)
        {
            PlayerSkills.Instance.ResetAllSkills();
            if(debug) Debug.Log("All skills reset for new run after win");
        }
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
            movement.ResetToSpawn(floorManager.PlayerSpawnPoint);
    }

    public void ChangeState(GameStates newState)
    {
        if (CurrentState == newState) return;

        lastStateDebug = CurrentState.ToString();
        CurrentState = newState;
        currentStateDebug = CurrentState.ToString();

        if(debug) Debug.Log($"Game state changed: {lastStateDebug} -> {currentStateDebug}");

        GameEvents.GameStateChanged(newState);

		// update music for this state
		ApplyMusicForState(newState);
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
