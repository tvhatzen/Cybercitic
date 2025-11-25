using System;
using UnityEngine;

namespace Cybercitic.UI
{
    /// <summary>
    /// Controller for upgrade shop business logic
    /// Handles purchase validation, state management, and coordinates between panels
    /// Separates business logic from UI presentation
    /// </summary>
    public class UpgradeShopController
    {
        private readonly ICurrencyService currencyService;
        private readonly IUpgradeService upgradeService;
        private readonly bool debug;

        private Upgrade selectedUpgrade;

        public Upgrade SelectedUpgrade => selectedUpgrade;

        public event Action<Upgrade> OnUpgradeSelected;
        public event Action<Upgrade> OnUpgradePurchased;

        public UpgradeShopController(ICurrencyService currencyService, IUpgradeService upgradeService, bool debug = false)
        {
            this.currencyService = currencyService ?? throw new ArgumentNullException(nameof(currencyService));
            this.upgradeService = upgradeService ?? throw new ArgumentNullException(nameof(upgradeService));
            this.debug = debug;
        }

        public void SelectUpgrade(Upgrade upgrade)
        {
            selectedUpgrade = upgrade;
            OnUpgradeSelected?.Invoke(upgrade);

            if (debug) Debug.Log($"[UpgradeShopController] Selected upgrade: {upgrade?.UpgradeName ?? "null"}");
        }

        public PurchaseButtonState GetPurchaseButtonState(Upgrade upgrade)
        {
            if (upgrade == null) return PurchaseButtonState.Locked;

            if (upgrade.IsMaxLevel)
            {
                return PurchaseButtonState.MaxLevel;
            }

            if (!upgrade.CanUpgrade)
            {
                return PurchaseButtonState.Locked;
            }

            int cost = upgrade.GetCost();
            bool canAfford = currencyService.CanAfford(cost);

            return canAfford ? PurchaseButtonState.CanPurchase : PurchaseButtonState.CannotAfford;
        }

        public bool CanAffordUpgrade(Upgrade upgrade)
        {
            if (upgrade == null) return false;

            int cost = upgrade.GetCost();
            return currencyService.CanAfford(cost);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="upgrade"></param>
        /// <returns></returns>
        public bool PurchaseUpgrade(Upgrade upgrade)
        {
            if (upgrade == null)
            {
                if (debug) Debug.LogWarning("[UpgradeShopController] Cannot purchase null upgrade");
                return false;
            }

            PurchaseButtonState state = GetPurchaseButtonState(upgrade);
            if (state != PurchaseButtonState.CanPurchase)
            {
                if (debug) Debug.LogWarning($"[UpgradeShopController] Cannot purchase upgrade {upgrade.UpgradeName} - State: {state}");
                return false;
            }

            bool success = upgradeService.PurchaseUpgrade(upgrade);

            if (success)
            {
                OnUpgradePurchased?.Invoke(upgrade);
                if (debug) Debug.Log($"[UpgradeShopController] Successfully purchased {upgrade.UpgradeName}");
            }

            return success;
        }
    }
}