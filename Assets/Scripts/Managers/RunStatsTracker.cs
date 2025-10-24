using UnityEngine;
using System;

// tracks statistics for the current run (enemies killed, credits collected, floors cleared, boss defeated, skill unlocked)
public class RunStatsTracker : SingletonBase<RunStatsTracker>
{
    [Header("Run Statistics")]
    [SerializeField] private int enemiesKilled = 0;
    [SerializeField] private int creditsCollected = 0;
    [SerializeField] private int floorsCleared = 0;
    [SerializeField] private bool beatBoss = false;
    [SerializeField] private string unlockedSkill = "";
    
    [Header("Starting Values")]
    [SerializeField] private int startingCredits = 0;

    public bool debug = false;

    // public properties
    public int EnemiesKilled 
    { 
        get 
        {
            if(debug) Debug.Log($"[RunStatsTracker] EnemiesKilled getter called: {enemiesKilled}");
            return enemiesKilled;
        }
    }
    public int CreditsCollected 
    { 
        get 
        {
            if(debug) Debug.Log($"[RunStatsTracker] CreditsCollected getter called: {creditsCollected}");
            return creditsCollected;
        }
    }
    public int FloorsCleared 
    { 
        get 
        {
            if(debug) Debug.Log($"[RunStatsTracker] FloorsCleared getter called: {floorsCleared}");
            return floorsCleared;
        }
    }
    public bool BeatBoss => beatBoss;
    public string UnlockedSkill => unlockedSkill;

    protected override void Awake()
    {
        base.Awake();
        
        // subscribe to game events
        HealthSystem.OnAnyDeath += OnEntityDied;
        
        // subscribe to Event Bus
        GameEvents.OnCreditsChanged += OnCreditsChanged;
        GameEvents.OnFloorCleared += OnFloorClearedInternal;
        
        if (CurrencyManager.Instance != null)
        {
            startingCredits = CurrencyManager.Instance.Credits;
        }
    }

    private void OnDestroy()
    {
        HealthSystem.OnAnyDeath -= OnEntityDied;
        
        // unsubscribe from Event Bus
        GameEvents.OnCreditsChanged -= OnCreditsChanged;
        GameEvents.OnFloorCleared -= OnFloorClearedInternal;
    }

    private void Start()
    {
        // record starting credits
        if (CurrencyManager.Instance != null)
            startingCredits = CurrencyManager.Instance.Credits;
    }

    // resets all run stats (called at start of new run)
    public void ResetStats()
    {
        enemiesKilled = 0;
        creditsCollected = 0;
        floorsCleared = 0;
        beatBoss = false;
        unlockedSkill = "";
        
        if (CurrencyManager.Instance != null)
        {
            startingCredits = CurrencyManager.Instance.Credits;
        }
        
        if(debug) Debug.Log("[RunStatsTracker] Stats reset for new run");
    }

    // resets stats for retry (keeps current credits as starting point)
    public void ResetStatsForRetry()
    {
        enemiesKilled = 0;
        creditsCollected = 0;
        floorsCleared = 0;
        beatBoss = false;
        unlockedSkill = "";
        
        if (CurrencyManager.Instance != null)
            startingCredits = CurrencyManager.Instance.Credits;
        
        if(debug) Debug.Log("[RunStatsTracker] Stats reset for retry - keeping current upgrades and credits");
    }

    private void OnEntityDied(GameObject entity)
    {
        if (entity == null) return;

        // check if its an enemy
        if (entity.CompareTag("Enemy") || entity.CompareTag("Boss"))
        {
            enemiesKilled++;
            
            GameEvents.EnemyKilled(enemiesKilled);
            
            if(debug) Debug.Log($"[RunStatsTracker] Enemy killed! Total: {enemiesKilled}");

            // check for boss
            if (entity.CompareTag("Boss"))
            {
                beatBoss = true;
                UnlockSkillForBoss();
            }
        }
    }

    private void OnCreditsChanged(int newTotal)
    {
        // calculate credits collected this run
        creditsCollected = newTotal - startingCredits;
        
        if (creditsCollected < 0)
            creditsCollected = 0; 
        
        GameEvents.CreditsCollected(creditsCollected);
    }

    // handle floor cleared events from Event Bus
    private void OnFloorClearedInternal(int floorNumber)
    {
        floorsCleared = floorNumber;
        
        if(debug) Debug.Log($"[RunStatsTracker] Floor {floorNumber} cleared! Total floors: {floorsCleared}");
    }

    private void UnlockSkillForBoss()
    {
        // determine which skill to unlock based on tier or floor
        int tier = TierManager.Instance != null ? TierManager.Instance.CurrentTier : 1;
        
        switch (tier)
        {
            case 1:
                unlockedSkill = "Double Jump";
                break;
            case 2:
                unlockedSkill = "Dash Attack";
                break;
            case 3:
                unlockedSkill = "Ground Pound";
                break;
            case 4:
                unlockedSkill = "Air Dash";
                break;
            case 5:
                unlockedSkill = "Ultimate Power";
                break;
            default:
                unlockedSkill = $"Tier {tier} Skill";
                break;
        }
        
        GameEvents.RunSkillUnlocked(unlockedSkill);
        if(debug) Debug.Log($"[RunStatsTracker] Skill unlocked: {unlockedSkill}");
    }

    public string GetRunSummary()
    {
        return $"Floors Cleared: {floorsCleared}\n" +
               $"Enemies Killed: {enemiesKilled}\n" +
               $"Credits Collected: {creditsCollected}\n" +
               $"Boss Defeated: {(beatBoss ? "Yes" : "No")}\n" +
               $"Skill Unlocked: {(string.IsNullOrEmpty(unlockedSkill) ? "None" : unlockedSkill)}";
    }
}

