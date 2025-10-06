using UnityEngine;

[CreateAssetMenu(fileName = "Core Upgrade", menuName = "Scriptable Objects/Upgrades/Core")]
public class Core : Upgrade
{
    protected override void ApplyUpgrade()
    {
        base.ApplyUpgrade();
        Debug.Log("Applied Core upgrade - Increased health");
    }
}
