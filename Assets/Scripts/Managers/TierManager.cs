using UnityEngine;
using System;

// tierManager tracks enemy tiers based on bosses defeated
// tier starts at 1 and increases by 1 after each boss is defeated
public class TierManager : SingletonBase<TierManager>
{
    [Header("Tier Settings")]
    [SerializeField] private int currentTier = 1;
    [SerializeField] private int bossesDefeated = 0;
    
    [Header("Tier Colors (for visual feedback)")]
    [SerializeField] private Color tier1Color = new Color(0.8f, 0.8f, 0.8f); // gray
    [SerializeField] private Color tier2Color = new Color(0.5f, 1f, 0.5f);   // green
    [SerializeField] private Color tier3Color = new Color(0.5f, 0.5f, 1f);   // blue
    [SerializeField] private Color tier4Color = new Color(1f, 0.5f, 1f);     // purple
    [SerializeField] private Color tier5Color = new Color(1f, 1f, 0.5f);     // yellow
    [SerializeField] private Color tier6PlusColor = new Color(1f, 0.5f, 0.5f); // red

    public int CurrentTier => currentTier;
    public int BossesDefeated => bossesDefeated;

    public bool debug = false;

    protected override void Awake()
    {
        base.Awake();
    }

    private void OnEnable() => HealthSystem.OnAnyDeath += OnEntityDied;
    private void OnDisable() => HealthSystem.OnAnyDeath -= OnEntityDied;

    private void OnEntityDied(GameObject entity)
    {
        // check for boss
        if (entity != null && entity.CompareTag("Boss"))
        {
            IncrementTier();
        }
    }

    // increment tier when a boss is defeated
    public void IncrementTier()
    {
        bossesDefeated++;
        currentTier = bossesDefeated + 1; // tier starts at 1, becomes 2 after first boss
        
        GameEvents.TierChanged(currentTier);
        if(debug) Debug.Log($"Tier increased! Now at Tier {currentTier} (Bosses defeated: {bossesDefeated})");
    }

    // reset tier to 1 (for new game)
    public void ResetTier()
    {
        currentTier = 1;
        bossesDefeated = 0;
        
        GameEvents.TierChanged(currentTier);
        if(debug) Debug.Log("Tier reset to 1");
    }

    /// get color associated with a specific tier
    public Color GetTierColor(int tier)
    {
        return tier switch
        {
            1 => tier1Color,
            2 => tier2Color,
            3 => tier3Color,
            4 => tier4Color,
            5 => tier5Color,
            _ => tier6PlusColor
        };
    }

    // get tier display name
    public string GetTierName(int tier)
    {
        return tier switch
        {
            1 => "Tier I",
            2 => "Tier II",
            3 => "Tier III",
            4 => "Tier IV",
            5 => "Tier V",
            _ => $"Tier {tier}"
        };
    }
}

