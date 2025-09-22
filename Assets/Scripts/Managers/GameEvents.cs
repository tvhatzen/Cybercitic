using UnityEngine;
using System;  

public class GameEvents : MonoBehaviour
{
    // when the player enters an enemy’s combat zone
    public static event Action<Transform> OnPlayerEnterCombat;

    // when the player leaves the combat zone
    public static event Action OnPlayerExitCombat;

    // when the player automatically attacks
    public static event Action<Transform> OnPlayerAttack;

    public static void PlayerEnteredCombat(Transform enemy)
    {
        OnPlayerEnterCombat?.Invoke(enemy);
    }

    public static void PlayerExitedCombat()
    {
        OnPlayerExitCombat?.Invoke();
    }

    public static void PlayerAttack(Transform target)
    {
        OnPlayerAttack?.Invoke(target);
    }
}
