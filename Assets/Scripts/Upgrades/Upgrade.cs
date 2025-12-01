using UnityEngine;
using System;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Upgrade", menuName = "Scriptable Objects/Upgrade")]
public class Upgrade : ScriptableObject 
{
    [Header("Upgrade Info")]
    [SerializeField] private string upgradeName;
    [SerializeField] private string upgradeDescription;
    [SerializeField] private Sprite icon;
    [SerializeField] private Sprite iconLocked;
    
    [Header("Cost and Levels")]
    [SerializeField] private int baseCost = 100;
    [SerializeField] private int maxUpgradeLevel = 3;
    [SerializeField] private float costMultiplier = 1.5f;
    
    [Header("Stat Modifications")]
    [SerializeField] private UpgradeType upgradeType;
    [SerializeField] private BodyPart bodyPart = BodyPart.Core;
    public float statIncreasePerLevel = 0.1f;
    
    // Runtime data
    [NonSerialized] public int currentLevel = 0;
    [NonSerialized] public bool isUnlocked = false;

    public enum UpgradeType
    {
        Health,
        Speed,
        Attack,
        Defense,
        DodgeChance
    }

    public enum BodyPart
    {
        Core,
        LeftArm,
        RightArm,
        LeftLeg,
        RightLeg
    }

    public BodyPart GetBodyPart() => bodyPart;

    public string UpgradeName => upgradeName;
    public string Description => upgradeDescription;
    public Sprite Icon => currentLevel > 0 ? icon : iconLocked;
    public int CurrentLevel => currentLevel;
    public int MaxLevel => maxUpgradeLevel;
    public bool IsMaxLevel => currentLevel >= maxUpgradeLevel;
    public bool CanUpgrade => currentLevel < maxUpgradeLevel; 
    public UpgradeType GetUpgradeType() => upgradeType;

    [Header("DEBUG")]
    public bool debug = false;

    public int GetCost()
    {
        if (currentLevel >= maxUpgradeLevel) return int.MaxValue;
        return Mathf.RoundToInt(baseCost * Mathf.Pow(costMultiplier, currentLevel));
    }

    public virtual bool PurchaseUpgrade()
    {
        if(debug) Debug.Log($"[Upgrade] PurchaseUpgrade called for {upgradeName} - Current level: {currentLevel}, CanUpgrade: {CanUpgrade}");
        
        if (!CanUpgrade) 
        {
            if(debug) Debug.LogWarning($"[Upgrade] Cannot upgrade {upgradeName} - already at max level or locked");
            return false;
        }
        
        int cost = GetCost();
        if(debug) Debug.Log($"[Upgrade] Cost for {upgradeName}: {cost} credits");
        
        if (!CurrencyManager.Instance.SpendCredits(cost)) 
        {
            if(debug) Debug.LogWarning($"[Upgrade] Not enough credits for {upgradeName}");
            return false;
        }
        
        int oldLevel = currentLevel;
        currentLevel++;
        if(debug) Debug.Log($"[Upgrade] {upgradeName} level increased from {oldLevel} to {currentLevel}");
        
        // mark as unlocked on first purchase
        if (!isUnlocked)
        {
            isUnlocked = true;
            if(debug) Debug.Log($"[Upgrade] {upgradeName} unlocked!");
        }
        
        ApplyUpgrade();
        
        GameEvents.UpgradePurchased(this);
        if(debug) Debug.Log($"[Upgrade] Successfully purchased {upgradeName} level {currentLevel}/{maxUpgradeLevel} for {cost} credits");
        
        return true;
    }

    protected virtual void ApplyUpgrade()
    {
        if (EntityStats.Instance == null)
        {
            if(debug) Debug.LogWarning($"[Upgrade] EntityStats.Instance is null, cannot apply upgrade for {upgradeName}");
            return;
        }
        
        float statIncrease = statIncreasePerLevel;
        
        if(debug) Debug.Log($"[Upgrade] Applying upgrade {upgradeName} - Type: {upgradeType}, Increase: {statIncrease}, Level: {currentLevel}");
        
        switch (upgradeType)
        {
            case UpgradeType.Health:
                int healthIncrease = Mathf.RoundToInt(statIncrease * 100);
                EntityStats.Instance.ModifyHealth(healthIncrease);
                if(debug) Debug.Log($"[Upgrade] Applied health increase: {healthIncrease}");
                break;
            case UpgradeType.Speed:
                EntityStats.Instance.ModifySpeed(statIncrease);
                if(debug) Debug.Log($"[Upgrade] Applied speed increase: {statIncrease}");
                break;
            case UpgradeType.Attack:
                int attackIncrease = Mathf.RoundToInt(statIncrease * 100);
                EntityStats.Instance.ModifyAttack(attackIncrease);
                if(debug) Debug.Log($"[Upgrade] Applied attack increase: {attackIncrease}");
                break;
            case UpgradeType.Defense:
                EntityStats.Instance.ModifyDefense(statIncrease);
                if(debug) Debug.Log($"[Upgrade] Applied defense increase: {statIncrease}");
                break;
            case UpgradeType.DodgeChance:
                EntityStats.Instance.ModifyDodgeChance(statIncrease);
                if(debug) Debug.Log($"[Upgrade] Applied dodge chance increase: {statIncrease}");
                break;
        }
    }
    
    // Re-apply all upgrade levels (used when starting a new game or loading saved data)
    // This applies the stat increase for each level the upgrade has
    public void ReapplyAllLevels()
    {
        if (currentLevel <= 0) return;
        
        if(debug) Debug.Log($"[Upgrade] Re-applying {currentLevel} levels for {upgradeName}");
        
        if (EntityStats.Instance == null)
        {
            if(debug) Debug.LogWarning($"[Upgrade] EntityStats.Instance is null, cannot re-apply levels for {upgradeName}");
            return;
        }
        
        float statIncrease = statIncreasePerLevel;
        float totalIncrease = statIncrease * currentLevel;
        
        if(debug) Debug.Log($"[Upgrade] Re-applying {upgradeName} - Type: {upgradeType}, Total increase: {totalIncrease} (Level {currentLevel} x {statIncrease})");
        
        // Apply the cumulative stat increase for all levels
        switch (upgradeType)
        {
            case UpgradeType.Health:
                int healthIncrease = Mathf.RoundToInt(totalIncrease * 100);
                EntityStats.Instance.ModifyHealth(healthIncrease);
                if(debug) Debug.Log($"[Upgrade] Re-applied health increase: {healthIncrease}");
                break;
            case UpgradeType.Speed:
                EntityStats.Instance.ModifySpeed(totalIncrease);
                if(debug) Debug.Log($"[Upgrade] Re-applied speed increase: {totalIncrease}");
                break;
            case UpgradeType.Attack:
                int attackIncrease = Mathf.RoundToInt(totalIncrease * 100);
                EntityStats.Instance.ModifyAttack(attackIncrease);
                if(debug) Debug.Log($"[Upgrade] Re-applied attack increase: {attackIncrease}");
                break;
            case UpgradeType.Defense:
                EntityStats.Instance.ModifyDefense(totalIncrease);
                if(debug) Debug.Log($"[Upgrade] Re-applied defense increase: {totalIncrease}");
                break;
            case UpgradeType.DodgeChance:
                EntityStats.Instance.ModifyDodgeChance(totalIncrease);
                if(debug) Debug.Log($"[Upgrade] Re-applied dodge chance increase: {totalIncrease}");
                break;
        }
    }

    public void ResetUpgrade()
    {
        currentLevel = 0;
        isUnlocked = false;
    }

    public void UnlockUpgrade()
    {
        isUnlocked = true;
    }    
} 