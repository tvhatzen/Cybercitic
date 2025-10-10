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

    // events
    public event Action<int> OnEnemyKilled;
    public event Action<int> OnCreditsCollected;
    public event Action<int> OnFloorCleared;
    public event Action<string> OnSkillUnlocked;

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
        
        // Subscribe to game events
        HealthSystem.OnAnyDeath += OnEntityDied;
        
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnCreditsChanged += OnCreditsChanged;
            startingCredits = CurrencyManager.Instance.Credits;
        }
    }

    private void OnDestroy()
    {
        HealthSystem.OnAnyDeath -= OnEntityDied;
        
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnCreditsChanged -= OnCreditsChanged;
        }
    }

    private void Start()
    {
        // Record starting credits
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
            OnEnemyKilled?.Invoke(enemiesKilled);
            
            if(debug) Debug.Log($"[RunStatsTracker] Enemy killed! Total: {enemiesKilled}");

            // check for boss
            if (entity.CompareTag("Boss"))
            {
                beatBoss = true;
                // unlock skill 
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
        
        OnCreditsCollected?.Invoke(creditsCollected);
    }

    public void FloorCleared(int floorNumber)
    {
        floorsCleared = floorNumber;
        OnFloorCleared?.Invoke(floorsCleared);
        
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
        
        OnSkillUnlocked?.Invoke(unlockedSkill);
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

