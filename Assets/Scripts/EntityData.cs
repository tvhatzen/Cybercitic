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

    [Tooltip("Base movement speed")]
    public float baseSpeedMultiplier = 5f;

    [Tooltip("Base attack damage")]
    public int baseAttack = 10;

    [Tooltip("PLAYER : Base dodge chance (0-1)")]
    public float baseDodgeChance = 0.1f;

    [Tooltip("PLAYER: Base defense - reduces incoming damage")]
    public float baseDefense = 0f;

    [Header("Current Stats")]
    [Tooltip("Current health - will be set to baseHealth on start")]
    public int currentHealth;

    [Tooltip("Current speed - can be modified by buffs/debuffs")]
    public float currentSpeed;

    [Tooltip("Current speed multiplier - can be modified by buffs/debuffs")]
    public float currentSpeedMultiplier;

    [Tooltip("Current attack - can be modified by upgrades")]
    public int currentAttack;

    [Tooltip("PLAYER: Current dodge chance - can be modified by upgrades")]
    public float currentDodgeChance;

    [Tooltip("PLAYER: Current defense - can be modified by upgrades")]
    public float currentDefense;

    // store original base stats to reset to after a win
    private int originalBaseHealth;
    private float originalBaseSpeed;
    private int originalBaseAttack;
    private float originalBaseDodgeChance;
    private float originalBaseDefense;
    private bool originalStatsStored = false;

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
        originalBaseDefense = baseDefense;
        originalStatsStored = true;
    }

    // initialize current stats to base values
    public void InitializeStats()
    {
        currentHealth = baseHealth;
        currentSpeed = baseSpeed;
        currentAttack = baseAttack;
        currentDodgeChance = baseDodgeChance;
        currentDefense = baseDefense;
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
        baseDefense = originalBaseDefense;

        InitializeStats();

        if (debug) Debug.Log($"[EntityData] {name} reset to original stats - HP: {baseHealth}, ATK: {baseAttack}, Speed: {baseSpeed}, SpeedMultiplier: {baseSpeedMultiplier}, Dodge: {baseDodgeChance}, Defense: {baseDefense}");
    }

    // scale stats by a multiplier (for enemy progression)
    public void ScaleStats(float healthMultiplier, float attackMultiplier)
    {
        baseHealth = Mathf.RoundToInt(baseHealth * healthMultiplier);
        baseAttack = Mathf.RoundToInt(baseAttack * attackMultiplier);
        InitializeStats();
    }

    // modify base stats (for permanent upgrades)
    public void ModifyBaseStats(int healthMod = 0, int attackMod = 0, float speedMod = 0f, float speedMultiplier = 0f, float dodgeMod = 0f, float defenseMod = 0f)
    {
        baseHealth += healthMod;
        baseAttack += attackMod;
        baseSpeed += speedMod;
        baseSpeedMultiplier += speedMultiplier;
        baseDodgeChance = Mathf.Clamp01(baseDodgeChance + dodgeMod);
        baseDefense = Mathf.Clamp01(baseDefense + defenseMod);

        // update current stats too
        currentHealth += healthMod;
        currentAttack += attackMod;
        currentSpeed += speedMod;
        currentSpeedMultiplier += speedMultiplier;
        currentDodgeChance = Mathf.Clamp01(currentDodgeChance + dodgeMod);
        currentDefense = Mathf.Clamp01(currentDefense + defenseMod);
    }
}