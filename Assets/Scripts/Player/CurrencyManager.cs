using System;
using UnityEngine;

public class CurrencyManager : SingletonBase<CurrencyManager>
{
    public event Action<int> OnCreditsChanged;

    private int credits = 0;
    public int Credits => credits;


    public void AddCredits(int amount)
    {
        credits += amount;
        Debug.Log($"Player earned {amount} credits. Total = {credits}");
        OnCreditsChanged?.Invoke(credits);
    }

    public bool SpendCredits(int amount)
    {
        if (credits < amount) return false;

        credits -= amount;
        OnCreditsChanged?.Invoke(credits);
        return true;
    }

}
// find existing or create currency ui reference to tmpro