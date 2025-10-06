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
        DodgeChance
    }

    public string UpgradeName => upgradeName;
    public string Description => upgradeDescription;
    public Sprite Icon => isUnlocked ? icon : iconLocked;
    public int CurrentLevel => currentLevel;
    public int MaxLevel => maxUpgradeLevel;
    public bool IsMaxLevel => currentLevel >= maxUpgradeLevel;
    public bool CanUpgrade => currentLevel < maxUpgradeLevel && isUnlocked;

    public int GetCost()
    {
        if (currentLevel >= maxUpgradeLevel) return int.MaxValue;
        return Mathf.RoundToInt(baseCost * Mathf.Pow(costMultiplier, currentLevel));
    }

    public virtual bool PurchaseUpgrade()
    {
        if (!CanUpgrade) return false;
        
        int cost = GetCost();
        if (!CurrencyManager.Instance.SpendCredits(cost)) return false;
        
        currentLevel++;
        ApplyUpgrade();
        
        GameEvents.UpgradePurchased(this);
        Debug.Log($"Purchased {upgradeName} level {currentLevel} for {cost} credits");
        
        return true;
    }

    protected virtual void ApplyUpgrade()
    {
        if (PlayerStats.Instance == null) return;
        
        float statIncrease = statIncreasePerLevel;
        
        switch (upgradeType)
        {
            case UpgradeType.Health:
                PlayerStats.Instance.ModifyHealth(Mathf.RoundToInt(statIncrease * 100));
                break;
            case UpgradeType.Speed:
                PlayerStats.Instance.ModifySpeed(statIncrease);
                break;
            case UpgradeType.Attack:
                PlayerStats.Instance.ModifyAttack(Mathf.RoundToInt(statIncrease * 100));
                break;
            case UpgradeType.DodgeChance:
                PlayerStats.Instance.ModifyDodgeChance(statIncrease);
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