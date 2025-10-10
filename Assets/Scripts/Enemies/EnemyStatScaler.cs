using UnityEngine;

/// <summary>
/// EnemyStatScaler scales enemy stats based on floor level and tier
/// Use custom formulas: Health = 19 + (5 * (level * 0.2))^2
///                      Damage = 1 + (2.5 * (level * 0.2))^2
/// </summary>
public class EnemyStatScaler : MonoBehaviour
{
    [Header("Scaling Settings")]
    [SerializeField] private int floorsPerTier = 5;
    [SerializeField] private bool isBoss = false;
    [SerializeField] private float bossMultiplier = 2.0f;
    [SerializeField] private bool isElite = false;
    [SerializeField] private float eliteMultiplier = 1.5f;

    private EntityData entityData;
    private bool hasBeenScaled = false;
    private int currentTier = 1;

    [Header("DEBUG")]
    public bool debug = false;

    private void Awake()
    {
        entityData = GetComponent<EntityData>();
        if (entityData == null)
        {
            if(debug) Debug.LogError($"{name} EnemyStatScaler: Missing EntityData component!");
        }

        if (TierManager.Instance != null)
        {
            currentTier = TierManager.Instance.CurrentTier;
        }
    }

    /// <summary>
    /// scale enemy stats based on current floor level and enemy's tier
    /// called when enemy is spawned
    /// </summary>
    public void ScaleToFloor(int floorLevel)
    {
        if (hasBeenScaled)
        {
            if(debug) Debug.LogWarning($"{name} has already been scaled");
            return;
        }

        if (entityData == null)
        {
            if(debug) Debug.LogError($"{name} cannot scale - no EntityData found");
            return;
        }

        // update tier
        if (TierManager.Instance != null)
        {
            currentTier = TierManager.Instance.CurrentTier;
        }

        // get effective level based on floor and tier
        int effectiveLevel = CalculateEffectiveLevel(floorLevel, currentTier);

        int scaledHealth = CalculateHealth(effectiveLevel);
        int scaledDamage = CalculateDamage(effectiveLevel);

        if (isBoss)
        {
            scaledHealth = Mathf.RoundToInt(scaledHealth * bossMultiplier);
            scaledDamage = Mathf.RoundToInt(scaledDamage * bossMultiplier);
        }
        else if (isElite)
        {
            scaledHealth = Mathf.RoundToInt(scaledHealth * eliteMultiplier);
            scaledDamage = Mathf.RoundToInt(scaledDamage * eliteMultiplier);
        }

        // update stats
        entityData.baseHealth = scaledHealth;
        entityData.baseAttack = scaledDamage;
        entityData.InitializeStats();
        
        hasBeenScaled = true;

        if(debug) Debug.Log($"{name} scaled - Tier {currentTier}, Floor {floorLevel}, Level {effectiveLevel}: HP={scaledHealth}, ATK={scaledDamage}");
    }

    private int CalculateEffectiveLevel(int floor, int tier)
    {
        return floor + ((tier - 1) * floorsPerTier);
    }

    // calculate health
    private int CalculateHealth(int level)
    {
        float scaledLevel = level * 0.2f;
        float healthValue = 19f + (5f * scaledLevel * scaledLevel);
        return Mathf.RoundToInt(healthValue);
    }

    // calculate damage
    private int CalculateDamage(int level)
    {
        float scaledLevel = level * 0.2f;
        float damageValue = 1f + (2.5f * scaledLevel * scaledLevel);
        return Mathf.RoundToInt(damageValue);
    }


    public void SetAsBoss(bool value = true)
    {
        isBoss = value;
        isElite = false; 
    }
    public void SetAsElite(bool value = true)
    {
        isElite = value;
        isBoss = false; 
    }

    public string GetScalingInfo(int floorLevel)
    {
        int tier = TierManager.Instance != null ? TierManager.Instance.CurrentTier : 1;
        int level = CalculateEffectiveLevel(floorLevel, tier);
        int hp = CalculateHealth(level);
        int dmg = CalculateDamage(level);
        
        if (isBoss)
        {
            hp = Mathf.RoundToInt(hp * bossMultiplier);
            dmg = Mathf.RoundToInt(dmg * bossMultiplier);
        }
        else if (isElite)
        {
            hp = Mathf.RoundToInt(hp * eliteMultiplier);
            dmg = Mathf.RoundToInt(dmg * eliteMultiplier);
        }

        return $"Tier {tier}, Floor {floorLevel} (Lvl {level}): HP={hp}, ATK={dmg}";
    }

    public int GetCurrentTier()
    {
        return currentTier;
    }
}

