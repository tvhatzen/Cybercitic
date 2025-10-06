using UnityEngine;

[CreateAssetMenu(fileName = "Leg Upgrade", menuName = "Scriptable Objects/Upgrades/Leg")]
public class Leg : Upgrade
{
    protected override void ApplyUpgrade()
    {
        base.ApplyUpgrade();
        Debug.Log("Applied Leg upgrade - Increased speed and dodge chance");
        
        // Leg upgrades can affect multiple stats
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.ModifySpeed(statIncreasePerLevel * 0.5f);
            PlayerStats.Instance.ModifyDodgeChance(statIncreasePerLevel * 0.5f);
        }
    }
}
