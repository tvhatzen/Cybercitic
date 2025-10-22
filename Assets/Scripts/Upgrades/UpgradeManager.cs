using UnityEngine;
using System.Collections.Generic;
using System;

public class UpgradeManager : SingletonBase<UpgradeManager>
{
    [Header("Available Upgrades")]
    [SerializeField] private List<Upgrade> allUpgrades = new List<Upgrade>();
    
    [Header("Shop Settings")]
    [SerializeField] private bool unlockAllUpgrades = false;

    // Events
    public event Action<Upgrade> OnUpgradePurchased;
    public event Action<Upgrade> OnUpgradeUnlocked;
    public event Action OnAllUpgradesReset;

    [Header("DEBUG")]
    public bool debug = false;

    private void Start()
    {
        InitializeUpgrades();
    }

    private void InitializeUpgrades()
    {
        foreach (var upgrade in allUpgrades)
        {
            if (upgrade != null)
            {
                upgrade.ResetUpgrade();
                
                if (unlockAllUpgrades)
                {
                    upgrade.UnlockUpgrade();
                }
            }
        }
    }

    public List<Upgrade> GetAvailableUpgrades()
    {
        return allUpgrades.FindAll(upgrade => upgrade != null && upgrade.isUnlocked);
    }

    public List<Upgrade> GetAllUpgrades()
    {
        return new List<Upgrade>(allUpgrades);
    }

    public bool PurchaseUpgrade(Upgrade upgrade)
    {
        if (upgrade == null) return false;
        
        bool success = upgrade.PurchaseUpgrade();
        if (success)
        {
            OnUpgradePurchased?.Invoke(upgrade);
            if(debug) Debug.Log($"Successfully purchased upgrade: {upgrade.UpgradeName}");
        }
        
        return success;
    }

    public void UnlockUpgrade(Upgrade upgrade)
    {
        if (upgrade == null) return;
        
        upgrade.UnlockUpgrade();
        OnUpgradeUnlocked?.Invoke(upgrade);
        if(debug) Debug.Log($"Unlocked upgrade: {upgrade.UpgradeName}");
    }

    public void UnlockAllUpgrades()
    {
        foreach (var upgrade in allUpgrades)
        {
            if (upgrade != null)
            {
                UnlockUpgrade(upgrade);
            }
        }
    }

    public Upgrade GetUpgradeByName(string upgradeName)
    {
        return allUpgrades.Find(upgrade => upgrade != null && upgrade.UpgradeName == upgradeName);
    }

    // Reset all upgrades 
    public void ResetAllUpgrades()
    {
        foreach (var upgrade in allUpgrades)
        {
            if (upgrade != null)
            {
                upgrade.ResetUpgrade();
            }
        }
        
        if (EntityStats.Instance != null)
        {
            EntityStats.Instance.ResetStats();
        }
        
        // Fire event to notify UI to refresh
        OnAllUpgradesReset?.Invoke();
        if(debug) Debug.Log("All upgrades reset - UI refresh event fired");
    }
}
