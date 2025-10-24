using UnityEngine;
using System.Collections.Generic;
using System;

public class UpgradeManager : SingletonBase<UpgradeManager>
{
    [Header("Available Upgrades")]
    [SerializeField] private List<Upgrade> allUpgrades = new List<Upgrade>();
    
    [Header("Shop Settings")]
    [SerializeField] private bool unlockAllUpgrades = false;

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
            GameEvents.UpgradePurchased(upgrade);
            if(debug) Debug.Log($"Successfully purchased upgrade: {upgrade.UpgradeName}");
        }
        
        return success;
    }

    public void UnlockUpgrade(Upgrade upgrade)
    {
        if (upgrade == null) return;
        
        upgrade.UnlockUpgrade();
        
        GameEvents.UpgradeUnlocked(upgrade);
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

    // reset all upgrades 
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
        
        // fire event to notify UI to refresh
        GameEvents.AllUpgradesReset();
        if(debug) Debug.Log("All upgrades reset - UI refresh event fired");
    }
}
