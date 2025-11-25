using System.Collections.Generic;
using UnityEngine;

namespace Cybercitic.UI
{
    /// <summary>
    /// Handles the equipped upgrade display panel UI.
    /// Shows currently equipped upgrades for each body part.
    /// </summary>
    public class EquippedDisplayPanel : MonoBehaviour
    {
        [Header("Equipped Displays")]
        [SerializeField] private EquippedUpgradeDisplay[] equippedDisplays;

        [Header("Debug")]
        [SerializeField] private bool debug;

        private Dictionary<Upgrade.BodyPart, EquippedUpgradeDisplay> displayMap = new Dictionary<Upgrade.BodyPart, EquippedUpgradeDisplay>();

        public void Initialize()
        {
            displayMap.Clear();

            if (equippedDisplays != null)
            {
                foreach (var display in equippedDisplays)
                {
                    if (display != null)
                    {
                        Upgrade.BodyPart bodyPart = display.BodyPart;
                        displayMap[bodyPart] = display;
                    }
                }
            }

            if (debug)
            {
                Debug.Log($"[EquippedDisplayPanel] Initialized with {displayMap.Count} displays");
            }
        }

        public void UpdateDisplay(Upgrade.BodyPart bodyPart, Upgrade upgrade)
        {
            if (displayMap.TryGetValue(bodyPart, out var display))
            {
                if (display != null)
                {
                    display.UpdateDisplay(upgrade);
                }
            }
        }

        public void UpdateAllDisplays(Dictionary<Upgrade.BodyPart, UpgradeButtonUI> upgradeButtons)
        {
            foreach (var kvp in upgradeButtons)
            {
                Upgrade.BodyPart bodyPart = kvp.Key;
                UpgradeButtonUI button = kvp.Value;

                if (button != null && button.Upgrade != null)
                {
                    UpdateDisplay(bodyPart, button.Upgrade);
                }
            }
        }

        public void RegisterDisplay(Upgrade.BodyPart bodyPart, EquippedUpgradeDisplay display)
        {
            if (display != null)
            {
                displayMap[bodyPart] = display;
            }
        }
    }
}

