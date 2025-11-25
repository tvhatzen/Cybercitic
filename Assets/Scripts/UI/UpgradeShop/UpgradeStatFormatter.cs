using UnityEngine;

namespace Cybercitic.UI
{
    /// <summary>
    /// Encapsulates stat formatting logic for upgrades
    /// Separates formatting concerns from UI display logic
    /// </summary>
    public static class UpgradeStatFormatter
    {
        public static string FormatStatInfo(Upgrade upgrade)
        {
            if (upgrade == null) return string.Empty;

            float increaseAmount = upgrade.statIncreasePerLevel;
            string statName = GetStatName(upgrade.GetUpgradeType());
            string displayValue = GetDisplayValue(upgrade.GetUpgradeType(), increaseAmount);

            return $"<b>{statName} {displayValue} \n<b>Level:</b> {upgrade.CurrentLevel}/{upgrade.MaxLevel}";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="upgradeType"></param>
        /// <returns></returns>
        private static string GetStatName(Upgrade.UpgradeType upgradeType)
        {
            return upgradeType switch
            {
                Upgrade.UpgradeType.Health => "Health",
                Upgrade.UpgradeType.Speed => "Attack Speed",
                Upgrade.UpgradeType.Attack => "Attack",
                Upgrade.UpgradeType.Defense => "Defense",
                Upgrade.UpgradeType.DodgeChance => "Dodge Chance",
                _ => "Unknown"
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="upgradeType"></param>
        /// <param name="increaseAmount"></param>
        /// <returns></returns>
        private static string GetDisplayValue(Upgrade.UpgradeType upgradeType, float increaseAmount)
        {
            return upgradeType switch
            {
                Upgrade.UpgradeType.Health => $"+{Mathf.RoundToInt(increaseAmount * 100)}",
                Upgrade.UpgradeType.Speed => $"+{(increaseAmount * 100):F2}%",
                Upgrade.UpgradeType.Attack => $"+{Mathf.RoundToInt(increaseAmount * 100)}",
                Upgrade.UpgradeType.Defense => $"+{(increaseAmount * 100):F2}%",
                Upgrade.UpgradeType.DodgeChance => $"+{(increaseAmount * 100):F1}%",
                _ => string.Empty
            };
        }
    }
}