using UnityEngine;
using System;

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
        if (EntityStats.Instance == null) return;
        
        float statIncrease = statIncreasePerLevel;
        
        switch (upgradeType)
        {
            case UpgradeType.Health:
                EntityStats.Instance.ModifyHealth(Mathf.RoundToInt(statIncrease * 100));
                break;
            case UpgradeType.Speed:
                EntityStats.Instance.ModifySpeed(statIncrease);
                break;
            case UpgradeType.Attack:
                EntityStats.Instance.ModifyAttack(Mathf.RoundToInt(statIncrease * 100));
                break;
            case UpgradeType.Defense:
                EntityStats.Instance.ModifyDefense(statIncrease);
                break;
            case UpgradeType.DodgeChance:
                EntityStats.Instance.ModifyDodgeChance(statIncrease);
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