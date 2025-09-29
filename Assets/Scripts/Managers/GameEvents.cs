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

    // when purchasing an upgrade 
    public static event Action<Upgrade> onUpgradePurchased;

    public static void PlayerEnteredCombat(Transform[] enemies)
    {
        if (enemies == null || enemies.Length == 0)
        {
            Debug.LogWarning("GameEvents: tried to enter combat with null enemy");
            return;
        }

        OnPlayerEnterCombat?.Invoke(enemies);
        Debug.Log("GameEvents: on player entered combat with " + enemies.Length + "enemies");
    }

    public static void PlayerExitedCombat()
    {
        OnPlayerExitCombat?.Invoke();
        Debug.Log("GameEvents: player exited combat");
    }

    public static void PlayerAttack(Transform target)
    {
        OnPlayerAttack?.Invoke(target);
        Debug.Log("GameEvents: player attack" + target.name);
    }

    public static void EntityDied(HealthSystem hs)
    {
        if (hs == null)
        {
            Debug.LogWarning("GameEvents: tried to broadcast death of null entity");
            return;
        }

        Debug.Log($"GameEvents: {hs.name} died!");
        OnAnyDeath?.Invoke(hs);
    }

    public static void UpgradePurchased(Upgrade upgrade)
    {
        if (upgrade == null) return;

        onUpgradePurchased?.Invoke(upgrade);
        Debug.Log("Upgrade event called for: " + upgrade.name);
    }
}
