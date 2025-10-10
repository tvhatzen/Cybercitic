using UnityEngine;

/// <summary>
/// EntityData holds base stats for any entity (player or enemy)
/// each entity gets its own instance
/// sets base stats that can be modified through upgrades or progression
/// </summary>
public class EntityData : MonoBehaviour
{
    [Header("Base Stats")]
    [Tooltip("Base health value for this entity")]
    public int baseHealth = 100;
    
    [Tooltip("Base movement speed")]
    public float baseSpeed = 5f;
    
    [Tooltip("Base attack damage")]
    public int baseAttack = 10;
    
    [Tooltip("Base dodge chance (0-1)")]
    public float baseDodgeChance = 0.1f;

    [Header("Current Stats")]
    [Tooltip("Current health - will be set to baseHealth on start")]
    public int currentHealth;
    
    [Tooltip("Current speed - can be modified by buffs/debuffs")]
    public float currentSpeed;
    
    [Tooltip("Current attack - can be modified by upgrades")]
    public int currentAttack;
    
    [Tooltip("Current dodge chance - can be modified by upgrades")]
    public float currentDodgeChance;

    // store original base stats to reset to after a win
    private int originalBaseHealth;
    private float originalBaseSpeed;
    private int originalBaseAttack;
    private float originalBaseDodgeChance;
    private bool originalStatsStored = false;

    [Header("DEBUG")]
    public bool debug = false;

    private void Awake()
    {
        // store original base stats on first Awake
        if (!originalStatsStored)
        {
            StoreOriginalStats();
        }
        InitializeStats();
    }

    private void StoreOriginalStats()
    {
        originalBaseHealth = baseHealth;
        originalBaseSpeed = baseSpeed;
        originalBaseAttack = baseAttack;
        originalBaseDodgeChance = baseDodgeChance;
        originalStatsStored = true;
    }

    // initialize current stats to base values
    public void InitializeStats()
    {
        currentHealth = baseHealth;
        currentSpeed = baseSpeed;
        currentAttack = baseAttack;
        currentDodgeChance = baseDodgeChance;
    }

    // reset stats to base values
    public void ResetToBase()
    {
        InitializeStats();
    }

    // reset both base and current stats to the original values (for new game after win)
    public void ResetToOriginalStats()
    {
        baseHealth = originalBaseHealth;
        baseSpeed = originalBaseSpeed;
        baseAttack = originalBaseAttack;
        baseDodgeChance = originalBaseDodgeChance;
        
        InitializeStats();
        
        if(debug) Debug.Log($"[EntityData] {name} reset to original stats - HP: {baseHealth}, ATK: {baseAttack}, Speed: {baseSpeed}, Dodge: {baseDodgeChance}");
    }

    // scale stats by a multiplier (for enemy progression)
    public void ScaleStats(float healthMultiplier, float attackMultiplier)
    {
        baseHealth = Mathf.RoundToInt(baseHealth * healthMultiplier);
        baseAttack = Mathf.RoundToInt(baseAttack * attackMultiplier);
        InitializeStats();
    }

    // modify base stats (for permanent upgrades)
    public void ModifyBaseStats(int healthMod = 0, int attackMod = 0, float speedMod = 0f, float dodgeMod = 0f)
    {
        baseHealth += healthMod;
        baseAttack += attackMod;
        baseSpeed += speedMod;
        baseDodgeChance = Mathf.Clamp01(baseDodgeChance + dodgeMod);
        
        // update current stats too
        currentHealth += healthMod;
        currentAttack += attackMod;
        currentSpeed += speedMod;
        currentDodgeChance = Mathf.Clamp01(currentDodgeChance + dodgeMod);
    }
}

