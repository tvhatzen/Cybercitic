using System;  
using UnityEngine;

public class GameEvents : SingletonBase<GameEvents>
{
    #region Combat Events

    // when the player enters an enemy's combat zone
    public static event Action<Transform[]> OnPlayerEnterCombat;

    // when the player leaves the combat zone
    public static event Action OnPlayerExitCombat;

    // when the player automatically attacks / is damaged
    public static event Action<Transform> OnPlayerAttack;
    public static event Action OnPlayerTakeDamage;

    // when the player's current target changes (provides old target and new target)
    public static event Action<Transform, Transform> OnPlayerTargetChanged;

    #endregion

    #region Health & Death Events

    // when any HealthSystem dies
    public static event Action<HealthSystem> OnAnyDeath;
    // when a boss dies
    public static event Action<HealthSystem> OnBossDeath;
    // when health changes
    public static event Action<int> OnHealthChanged; // current health value
    // when entity dies (instance event)
    public static event Action<HealthSystem> OnEntityDeath;

    #endregion

    #region Player System Events

    // Player Skills
    public static event Action<Skill> OnSkillActivated;
    public static event Action<Skill> OnSkillUnlocked;

    // Player Movement
    public static event Action OnPlayerStartedMoving;
    public static event Action OnPlayerStoppedMoving;

    // Player Animation
    public static event Action<string> OnAnimationChanged; // animation name
    public static event Action OnAnimationCompleted;

    #endregion

    #region Economy Events

    // Currency
    public static event Action<int> OnCreditsChanged;
    public static event Action<int> OnCreditsAdded;
    public static event Action<int> OnCreditsSpent;

    // Upgrades
    public static event Action<Upgrade> OnUpgradePurchased;
    public static event Action<Upgrade> OnUpgradeUnlocked;
    public static event Action OnAllUpgradesReset;

    #endregion

    #region Level & Game State Events

    // Floor management
    public static event Action<int> OnFloorChanged; // notify UI 
    public static event Action<GameObject> OnEnemySpawned;

    // Run Statistics
    public static event Action<int> OnEnemyKilled; // enemy count
    public static event Action<int> OnCreditsCollected; // credits collected this run
    public static event Action<int> OnFloorCleared; // floor number cleared
    public static event Action<string> OnRunSkillUnlocked; // skill name unlocked in run

    // Game State
    public static event Action OnGameStarted;
    public static event Action OnGamePaused;
    public static event Action OnGameResumed;
    public static event Action OnGameEnded;
    public static event Action<GameState.GameStates> OnGameStateChanged;

    // Tier System
    public static event Action<int> OnTierChanged;

    // Entity Stats
    public static event Action<EntityStats> OnStatsChanged;

    #endregion

    #region Audio Events

    // Audio system events
    public static event Action<string> OnSoundRequested; // sound name
    public static event Action<AudioClip> OnMusicRequested; // music clip
    public static event Action<string> OnButtonHover;
    public static event Action<string> OnButtonClick;
    public static event Action<string> NotEnoughCreditsButton;

    #endregion

    public static bool debug = false;

    public static void PlayerEnteredCombat(Transform[] enemies)
    {
        if (enemies == null || enemies.Length == 0)
        {
            if(debug) Debug.LogWarning("GameEvents: tried to enter combat with null enemy");
            return;
        }

        OnPlayerEnterCombat?.Invoke(enemies);
        if(debug) Debug.Log("GameEvents: on player entered combat with " + enemies.Length + "enemies");
    }

    public static void PlayerExitedCombat()
    {
        OnPlayerExitCombat?.Invoke();
        if(debug) Debug.Log("GameEvents: player exited combat");
    }

    public static void PlayerTookDamage()
    {
        OnPlayerTakeDamage?.Invoke();
    }

    public static void PlayerAttack(Transform target)
    {
        OnPlayerAttack?.Invoke(target);
        if(debug) Debug.Log("GameEvents: player attack" + target.name);
    }

    public static void PlayerTargetChanged(Transform oldTarget, Transform newTarget)
    {
        OnPlayerTargetChanged?.Invoke(oldTarget, newTarget);
        if(debug) 
        {
            string oldTargetName = (oldTarget != null) ? oldTarget.name : "none";
            string newTargetName = (newTarget != null) ? newTarget.name : "none";
            Debug.Log($"GameEvents: player target changed from {oldTargetName} to {newTargetName}");
        }
    }

    public static void EntityDied(HealthSystem hs)
    {
        if (hs == null)
        {
            if(debug) Debug.LogWarning("GameEvents: tried to broadcast death of null entity");
            return;
        }

        if(debug) Debug.Log($"GameEvents: {hs.name} died!");
        OnAnyDeath?.Invoke(hs);
    }

    public static void HealthChanged(int currentHealth)
    {
        OnHealthChanged?.Invoke(currentHealth);
        if(debug) Debug.Log($"GameEvents: Health changed to - {currentHealth}");
    }

    public static void EntityDeath(HealthSystem healthSystem)
    {
        OnEntityDeath?.Invoke(healthSystem);
        if(debug) Debug.Log($"GameEvents: Entity death - {healthSystem?.name ?? "null"}");
    }

    public static void UpgradePurchased(Upgrade upgrade)
    {
        if (upgrade == null) return;

        OnUpgradePurchased?.Invoke(upgrade);
        if(debug) Debug.Log("Upgrade event called for: " + upgrade.name);
    }

    public static void UpgradeUnlocked(Upgrade upgrade)
    {
        if (upgrade == null) return;

        OnUpgradeUnlocked?.Invoke(upgrade);
        if(debug) Debug.Log($"GameEvents: Upgrade unlocked - {upgrade.name}");
    }

    public static void AllUpgradesReset()
    {
        OnAllUpgradesReset?.Invoke();
        if(debug) Debug.Log("GameEvents: All upgrades reset");
    }

    #region Player System Event Triggers

    // Player Skills
    public static void SkillActivated(Skill skill)
    {
        OnSkillActivated?.Invoke(skill);
        if(debug) Debug.Log($"GameEvents: Skill activated - {skill?.name ?? "null"}");
    }

    public static void SkillUnlocked(Skill skill)
    {
        OnSkillUnlocked?.Invoke(skill);
        if(debug) Debug.Log($"GameEvents: Skill unlocked - {skill?.name ?? "null"}");
    }

    // Player Movement
    public static void PlayerStartedMoving()
    {
        OnPlayerStartedMoving?.Invoke();
        if(debug) Debug.Log("GameEvents: Player started moving");
    }

    public static void PlayerStoppedMoving()
    {
        OnPlayerStoppedMoving?.Invoke();
        if(debug) Debug.Log("GameEvents: Player stopped moving");
    }

    // Player Animation
    public static void AnimationChanged(string animationName)
    {
        OnAnimationChanged?.Invoke(animationName);
        if(debug) Debug.Log($"GameEvents: Animation changed to - {animationName}");
    }

    public static void AnimationCompleted()
    {
        OnAnimationCompleted?.Invoke();
        if(debug) Debug.Log("GameEvents: Animation completed");
    }

    #endregion

    #region Economy Event Triggers

    // Currency
    public static void CreditsChanged(int newAmount)
    {
        OnCreditsChanged?.Invoke(newAmount);
        if(debug) Debug.Log($"GameEvents: Credits changed to - {newAmount}");
    }

    public static void CreditsAdded(int amount)
    {
        OnCreditsAdded?.Invoke(amount);
        if(debug) Debug.Log($"GameEvents: Credits added - {amount}");
    }

    public static void CreditsSpent(int amount)
    {
        OnCreditsSpent?.Invoke(amount);
        if(debug) Debug.Log($"GameEvents: Credits spent - {amount}");
    }

    #endregion

    #region Game State Event Triggers

    public static void GameStarted()
    {
        OnGameStarted?.Invoke();
        if(debug) Debug.Log("GameEvents: Game started");
    }

    public static void GamePaused()
    {
        OnGamePaused?.Invoke();
        if(debug) Debug.Log("GameEvents: Game paused");
    }

    public static void GameResumed()
    {
        OnGameResumed?.Invoke();
        if(debug) Debug.Log("GameEvents: Game resumed");
    }

    public static void GameEnded()
    {
        OnGameEnded?.Invoke();
        if(debug) Debug.Log("GameEvents: Game ended");
    }

    public static void GameStateChanged(GameState.GameStates newState)
    {
        OnGameStateChanged?.Invoke(newState);
        if(debug) Debug.Log($"GameEvents: Game state changed to - {newState}");
    }

    public static void TierChanged(int newTier)
    {
        OnTierChanged?.Invoke(newTier);
        if(debug) Debug.Log($"GameEvents: Tier changed to - {newTier}");
    }

    public static void StatsChanged(EntityStats stats)
    {
        OnStatsChanged?.Invoke(stats);
        if(debug) Debug.Log($"GameEvents: Stats changed - {stats?.name ?? "null"}");
    }

    #endregion

    #region Floor & Level Event Triggers

    public static void FloorChanged(int newFloor)
    {
        OnFloorChanged?.Invoke(newFloor);
        if(debug) Debug.Log($"GameEvents: Floor changed to - {newFloor}");
    }

    public static void EnemySpawned(GameObject enemy)
    {
        OnEnemySpawned?.Invoke(enemy);
        if(debug) Debug.Log($"GameEvents: Enemy spawned - {enemy?.name ?? "null"}");
    }

    // Run Statistics
    public static void EnemyKilled(int totalKilled)
    {
        OnEnemyKilled?.Invoke(totalKilled);
        if(debug) Debug.Log($"GameEvents: Enemy killed - Total: {totalKilled}");
    }

    public static void CreditsCollected(int creditsCollected)
    {
        OnCreditsCollected?.Invoke(creditsCollected);
        if(debug) Debug.Log($"GameEvents: Credits collected this run - {creditsCollected}");
    }

    public static void FloorCleared(int floorNumber)
    {
        OnFloorCleared?.Invoke(floorNumber);
        if(debug) Debug.Log($"GameEvents: Floor cleared - {floorNumber}");
    }

    public static void RunSkillUnlocked(string skillName)
    {
        OnRunSkillUnlocked?.Invoke(skillName);
        if(debug) Debug.Log($"GameEvents: Run skill unlocked - {skillName}");
    }

    #endregion

    #region Audio Event Triggers

    public static void RequestSound(string soundName)
    {
        OnSoundRequested?.Invoke(soundName);
        if(debug) Debug.Log($"GameEvents: Sound requested - {soundName}");
    }

    public static void RequestMusic(AudioClip musicClip)
    {
        OnMusicRequested?.Invoke(musicClip);
        if(debug) Debug.Log($"GameEvents: Music requested - {musicClip?.name ?? "null"}");
    }

    #endregion

    #region UI Audio Event Triggers

    public static void UIButtonHovered(string soundName)
    {
        OnButtonHover?.Invoke(soundName);
        if (debug) Debug.Log($"GameEvents: Sound requested - {soundName}");
    }

    public static void UIButtonClicked(string soundName)
    {
        OnButtonClick?.Invoke(soundName);
        if (debug) Debug.Log($"GameEvents: Sound requested - {soundName}");
    }

    public static void UIButtonNotEnoughCredits(string soundName)
    {
        NotEnoughCreditsButton?.Invoke(soundName);
        if (debug) Debug.Log($"GameEvents: Sound requested - {soundName}");
    }

    #endregion
}
