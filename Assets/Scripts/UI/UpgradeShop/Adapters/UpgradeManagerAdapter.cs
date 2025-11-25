using System.Collections.Generic;

namespace Cybercitic.UI
{
    /// <summary>
    /// Adapter to make UpgradeManager implement IUpgradeService
    /// </summary>
    public class UpgradeManagerAdapter : IUpgradeService
    {
        public List<Upgrade> GetAllUpgrades()
        {
            return UpgradeManager.Instance != null 
                ? UpgradeManager.Instance.GetAllUpgrades() 
                : new List<Upgrade>();
        }

        public bool PurchaseUpgrade(Upgrade upgrade)
        {
            return UpgradeManager.Instance != null && UpgradeManager.Instance.PurchaseUpgrade(upgrade);
        }

        public void UnlockUpgrade(Upgrade upgrade)
        {
            if (UpgradeManager.Instance != null)
            {
                UpgradeManager.Instance.UnlockUpgrade(upgrade);
            }
        }

        public Upgrade GetUpgradeByName(string upgradeName)
        {
            return UpgradeManager.Instance != null 
                ? UpgradeManager.Instance.GetUpgradeByName(upgradeName) 
                : null;
        }
    }
}

