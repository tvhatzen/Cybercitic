using System;
using UnityEngine;

public class CurrencyManager : SingletonBase<CurrencyManager>
{
    private int credits = 0;
    public int Credits => credits;

    public bool debug = false;

    public bool CanAfford(int cost)
    {
        if (cost <= credits)
            return true;
        return false;
    }

    public void AddCredits(int amount)
    {
        credits += amount;
        if(debug) Debug.Log($"Player earned {amount} credits. Total = {credits}");
        
        GameEvents.CreditsChanged(credits);
        GameEvents.CreditsAdded(amount);
    }

    public bool SpendCredits(int amount)
    {
        if (credits < amount) return false;

        credits -= amount;
        
        GameEvents.CreditsChanged(credits);
        GameEvents.CreditsSpent(amount);
        return true;
    }

    public void ResetCredits()
    {
        credits = 0;
        if(debug) Debug.Log("Credits reset to 0");
        
        GameEvents.CreditsChanged(credits);
    }
}