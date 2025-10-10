using System;
using UnityEngine;

public class CurrencyManager : SingletonBase<CurrencyManager>
{
    public event Action<int> OnCreditsChanged;

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
        OnCreditsChanged?.Invoke(credits);
    }

    public bool SpendCredits(int amount)
    {
        if (credits < amount) return false;

        credits -= amount;
        OnCreditsChanged?.Invoke(credits);
        return true;
    }

    public void ResetCredits()
    {
        credits = 0;
        if(debug) Debug.Log("Credits reset to 0");
        OnCreditsChanged?.Invoke(credits);
    }
}