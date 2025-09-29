using UnityEngine;

public class Upgrade : MonoBehaviour 
{
    // name 
    [SerializeField] private string upgradeName;
    // descriptiom
    [SerializeField] private string[] description;
    // cost
    [SerializeField] private int upgradeCost;
    // level
    [SerializeField] private int upgradeLevel;
    [SerializeField] private int maxUpgradeLevel;
    [SerializeField] private bool canUpgrade;
    // stat to upgrade
    //[SerializeField] private 
    // percent it upgrades
    [SerializeField] private float upgradePercent;
    // icon Locked
    public Sprite iconLocked;
    // icon
    public Sprite icon;

    public virtual void Awake()
    {
        upgradeLevel = 0;
        maxUpgradeLevel = 4;
    }

    public virtual void OnPurchase()
    {
        Debug.Log("Upgrade button pressed");
        // increment stat to upgrade, use upgrade cost
        if (canUpgrade && upgradeLevel != maxUpgradeLevel)
        {
            upgradeLevel++;
            //GameEvents.UpgradePurchased();
            Debug.Log("Upgraded skill " + upgradeName);
        }
    }
}
// need to figure out which upgrade is selected, upgrade only selected one
// base off mouse input 