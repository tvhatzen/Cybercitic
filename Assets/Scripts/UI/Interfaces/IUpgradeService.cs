using System.Collections.Generic;

namespace Cybercitic.UI
{
    /// <summary>
    /// Interface for upgrade management services.
    /// Enables dependency inversion and testability.
    /// </summary>
    public interface IUpgradeService
    {
        List<Upgrade> GetAllUpgrades();
        bool PurchaseUpgrade(Upgrade upgrade);
        void UnlockUpgrade(Upgrade upgrade);
        Upgrade GetUpgradeByName(string upgradeName);
    }
}

