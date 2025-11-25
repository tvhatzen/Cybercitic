using System.Collections.Generic;
using UnityEngine;

namespace Cybercitic.UI
{
    /// <summary>
    /// Handles the upgrade selection panel UI
    /// Manages upgrade buttons and their display
    /// </summary>
    public class UpgradeSelectionPanel : MonoBehaviour
    {
        [Header("Upgrade Buttons")]
        [SerializeField] private UpgradeButtonUI[] upgradeButtons;

        [Header("Debug")]
        [SerializeField] private bool debug;

        private Dictionary<Upgrade.BodyPart, UpgradeButtonUI> buttonMap = new Dictionary<Upgrade.BodyPart, UpgradeButtonUI>();
        private UpgradeButtonUI selectedButton;

        public void Initialize()
        {
            buttonMap.Clear();

            if (upgradeButtons != null)
            {
                foreach (var button in upgradeButtons)
                {
                    if (button != null && button.Upgrade != null)
                    {
                        Upgrade.BodyPart bodyPart = button.Upgrade.GetBodyPart();
                        if (!buttonMap.ContainsKey(bodyPart))
                        {
                            buttonMap[bodyPart] = button;
                        }
                    }
                }
            }

            if (debug) Debug.Log($"[UpgradeSelectionPanel] Initialized with {buttonMap.Count} buttons");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="upgradeService"></param>
        /// <param name="controller"></param>
        public void LoadUpgrades(IUpgradeService upgradeService, UpgradeShopController controller)
        {
            if (upgradeService == null || controller == null) return;

            var allUpgrades = upgradeService.GetAllUpgrades();

            // First, try to map upgrades to existing buttons by body part
            foreach (var upgrade in allUpgrades)
            {
                if (upgrade == null) continue;

                Upgrade.BodyPart bodyPart = upgrade.GetBodyPart();

                if (buttonMap.TryGetValue(bodyPart, out var button))
                {
                    button.Initialize(upgrade, controller);
                }
            }

            // If buttons array is provided but not in map, initialize them
            if (upgradeButtons != null)
            {
                int upgradeIndex = 0;
                foreach (var button in upgradeButtons)
                {
                    if (button != null && upgradeIndex < allUpgrades.Count)
                    {
                        var upgrade = allUpgrades[upgradeIndex];
                        if (upgrade != null)
                        {
                            button.Initialize(upgrade, controller);
                            Upgrade.BodyPart bodyPart = upgrade.GetBodyPart();
                            if (!buttonMap.ContainsKey(bodyPart))
                            {
                                buttonMap[bodyPart] = button;
                            }
                        }
                        upgradeIndex++;
                    }
                }
            }
        }

        public void SelectUpgrade(Upgrade upgrade)
        {
            if (selectedButton != null)
            {
                selectedButton.SetSelected(false);
            }

            selectedButton = null;

            foreach (var button in buttonMap.Values)
            {
                if (button != null && button.Upgrade == upgrade)
                {
                    button.SetSelected(true);
                    selectedButton = button;
                    break;
                }
            }
        }

        public void UpdateAllDisplays()
        {
            foreach (var button in buttonMap.Values)
            {
                if (button != null)
                {
                    button.UpdateDisplay();
                }
            }
        }

        public void RefreshAllButtonStates()
        {
            foreach (var button in buttonMap.Values)
            {
                if (button != null)
                {
                    button.RefreshButtonState();
                }
            }
        }

        public void ShowLevelSquares(UpgradeButtonUI button)
        {
            if (button != null)
            {
                button.ShowSquares();
            }
        }

        public void HideAllLevelSquares()
        {
            foreach (var button in buttonMap.Values)
            {
                if (button != null)
                {
                    button.HideSquares();
                }
            }
        }

        public UpgradeButtonUI GetSelectedButton() => selectedButton;
        public Dictionary<Upgrade.BodyPart, UpgradeButtonUI> GetAllButtons() => new Dictionary<Upgrade.BodyPart, UpgradeButtonUI>(buttonMap);
    }
}