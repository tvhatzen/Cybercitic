using UnityEngine;

[CreateAssetMenu(fileName = "Arm Upgrade", menuName = "Scriptable Objects/Upgrades/Arm")]
public class Arm : Upgrade
{
    protected override void ApplyUpgrade()
    {
        base.ApplyUpgrade();
        Debug.Log("Applied Arm upgrade - Increased attack power");
    }
}
