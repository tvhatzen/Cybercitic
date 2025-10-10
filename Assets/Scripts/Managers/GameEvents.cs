using System;  
using UnityEngine;

public class GameEvents : MonoBehaviour
{
    // when the player enters an enemy’s combat zone
    public static event Action<Transform[]> OnPlayerEnterCombat;

    // when the player leaves the combat zone
    public static event Action OnPlayerExitCombat;

    // when the player automatically attacks
    public static event Action<Transform> OnPlayerAttack;

    // when any HealthSystem dies
    public static event Action<HealthSystem> OnAnyDeath;
    // when a boss dies
    public static event Action<HealthSystem> OnBossDeath;

    // when purchasing an upgrade 
    public static event Action<Upgrade> onUpgradePurchased;

    [Header("DEBUG")]
    public static bool debug = false;

    public static void PlayerEnteredCombat(Transform[] enemies)
    {
        if (enemies == null || enemies.Length == 0)
        {
            if(debug) Debug.LogWarning("GameEvents: tried to enter combat with null enemy");
            return;
        }

        OnPlayerEnterCombat?.Invoke(enemies);
        if(debug) Debug.Log("GameEvents: on player entered combat with " + enemies.Length + "enemies");
    }

    public static void PlayerExitedCombat()
    {
        OnPlayerExitCombat?.Invoke();
        if(debug) Debug.Log("GameEvents: player exited combat");
    }

    public static void PlayerAttack(Transform target)
    {
        OnPlayerAttack?.Invoke(target);
        if(debug) Debug.Log("GameEvents: player attack" + target.name);
    }

    public static void EntityDied(HealthSystem hs)
    {
        if (hs == null)
        {
            if(debug) Debug.LogWarning("GameEvents: tried to broadcast death of null entity");
            return;
        }

        if(debug) Debug.Log($"GameEvents: {hs.name} died!");
        OnAnyDeath?.Invoke(hs);
    }

    public static void UpgradePurchased(Upgrade upgrade)
    {
        if (upgrade == null) return;

        onUpgradePurchased?.Invoke(upgrade);
        if(debug) Debug.Log("Upgrade event called for: " + upgrade.name);
    }
}
