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

        private Dictionary<string, UpgradeButtonUI> buttonMap = new Dictionary<string, UpgradeButtonUI>(); // Use upgrade name as key since BodyPart is not unique
        private UpgradeButtonUI selectedButton;

        public void Initialize()
        {
            buttonMap.Clear();

            // If upgradeButtons array is not assigned or empty, try to find buttons automatically
            if (upgradeButtons == null || upgradeButtons.Length == 0)
            {
                upgradeButtons = GetComponentsInChildren<UpgradeButtonUI>(true);
                if (debug) Debug.Log($"[UpgradeSelectionPanel] Auto-found {upgradeButtons?.Length ?? 0} upgrade buttons");
            }

            if (upgradeButtons != null)
            {
                foreach (var button in upgradeButtons)
                {
                    if (button != null)
                    {
                        // Don't require Upgrade to be set at initialization time
                        // It will be set later in LoadUpgrades
                        // Just store the button reference for now
                        if (button.Upgrade != null)
                        {
                            string upgradeName = button.Upgrade.UpgradeName;
                            if (!string.IsNullOrEmpty(upgradeName) && !buttonMap.ContainsKey(upgradeName))
                            {
                                buttonMap[upgradeName] = button;
                            }
                        }
                    }
                }
            }

            if (debug) Debug.Log($"[UpgradeSelectionPanel] Initialized with {buttonMap.Count} buttons (found {upgradeButtons?.Length ?? 0} total buttons)");
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

            // Ensure we have the buttons array
            if (upgradeButtons == null || upgradeButtons.Length == 0)
            {
                upgradeButtons = GetComponentsInChildren<UpgradeButtonUI>(true);
            }

            // Clear and rebuild buttonMap to ensure all buttons are registered
            buttonMap.Clear();

            // Create a list of available buttons (not yet assigned)
            List<UpgradeButtonUI> availableButtons = new List<UpgradeButtonUI>();
            if (upgradeButtons != null)
            {
                foreach (var button in upgradeButtons)
                {
                    if (button != null)
                    {
                        availableButtons.Add(button);
                    }
                }
            }

            // Map each upgrade to its corresponding button
            foreach (var upgrade in allUpgrades)
            {
                if (upgrade == null) continue;

                Upgrade.BodyPart bodyPart = upgrade.GetBodyPart();
                UpgradeButtonUI targetButton = null;

                if (debug) Debug.Log($"[UpgradeSelectionPanel] Looking for button for upgrade: {upgrade.UpgradeName} (BodyPart: {bodyPart})");

                // First, try to find a button that already has this upgrade
                foreach (var button in availableButtons)
                {
                    if (button != null && button.Upgrade != null && button.Upgrade.UpgradeName == upgrade.UpgradeName)
                    {
                        targetButton = button;
                        if (debug) Debug.Log($"[UpgradeSelectionPanel] Found existing button with matching upgrade: {button.gameObject.name}");
                        break;
                    }
                }

                // If not found, try to match by upgrade name or GameObject name
                if (targetButton == null)
                {
                    string upgradeNameLower = upgrade.UpgradeName.ToLower();
                    string bodyPartName = bodyPart.ToString();
                    string bodyPartLower = bodyPartName.ToLower();
                    
                    // First try matching by upgrade name (e.g., "Left Arm" upgrade matches "LeftArm" GameObject)
                    foreach (var button in availableButtons)
                    {
                        if (button != null && !buttonMap.ContainsValue(button))
                        {
                            string buttonName = button.gameObject.name.ToLower();
                            string buttonNameNoSpaces = buttonName.Replace(" ", "").Replace("_", "");
                            string upgradeNameNoSpaces = upgradeNameLower.Replace(" ", "").Replace("_", "");
                            
                            // Check if button name matches upgrade name
                            if (buttonName == upgradeNameLower || buttonNameNoSpaces == upgradeNameNoSpaces ||
                                buttonName.Contains(upgradeNameLower) || upgradeNameLower.Contains(buttonName) ||
                                buttonNameNoSpaces.Contains(upgradeNameNoSpaces) || upgradeNameNoSpaces.Contains(buttonNameNoSpaces))
                            {
                                targetButton = button;
                                if (debug) Debug.Log($"[UpgradeSelectionPanel] Matched by upgrade name: '{button.gameObject.name}' -> '{upgrade.UpgradeName}'");
                                break;
                            }
                        }
                    }
                    
                    // If still not found, try matching by BodyPart name (fallback)
                    if (targetButton == null)
                    {
                        foreach (var button in availableButtons)
                        {
                            if (button != null && !buttonMap.ContainsValue(button))
                            {
                                string buttonName = button.gameObject.name.ToLower();
                                // Check for exact match or contains match
                                if (buttonName == bodyPartLower || buttonName.Contains(bodyPartLower) || bodyPartLower.Contains(buttonName))
                                {
                                    targetButton = button;
                                    if (debug) Debug.Log($"[UpgradeSelectionPanel] Matched by BodyPart name: '{button.gameObject.name}' -> '{bodyPartName}'");
                                    break;
                                }
                            }
                        }
                    }
                    
                    // If still not found, try partial matching (e.g., "RightArm" matches "rightarm" or "right arm")
                    if (targetButton == null)
                    {
                        string bodyPartNoSpaces = bodyPartLower.Replace(" ", "").Replace("_", "");
                        string upgradeNameNoSpaces = upgradeNameLower.Replace(" ", "").Replace("_", "");
                        foreach (var button in availableButtons)
                        {
                            if (button != null && !buttonMap.ContainsValue(button))
                            {
                                string buttonName = button.gameObject.name.ToLower().Replace(" ", "").Replace("_", "");
                                if (buttonName == bodyPartNoSpaces || buttonName.Contains(bodyPartNoSpaces) || bodyPartNoSpaces.Contains(buttonName) ||
                                    buttonName == upgradeNameNoSpaces || buttonName.Contains(upgradeNameNoSpaces) || upgradeNameNoSpaces.Contains(buttonName))
                                {
                                    targetButton = button;
                                    if (debug) Debug.Log($"[UpgradeSelectionPanel] Matched by partial name: '{button.gameObject.name}' -> '{upgrade.UpgradeName}'");
                                    break;
                                }
                            }
                        }
                    }
                }

                // If still not found, use the first available button (fallback)
                if (targetButton == null && availableButtons.Count > 0)
                {
                    foreach (var button in availableButtons)
                    {
                        if (button != null && !buttonMap.ContainsValue(button))
                        {
                            targetButton = button;
                            if (debug) Debug.LogWarning($"[UpgradeSelectionPanel] Using fallback button '{button.gameObject.name}' for upgrade '{upgrade.UpgradeName}' (BodyPart: {bodyPart})");
                            break;
                        }
                    }
                }

                // Initialize the button and add to map
                if (targetButton != null)
                {
                    targetButton.Initialize(upgrade, controller);
                    buttonMap[upgrade.UpgradeName] = targetButton;
                    availableButtons.Remove(targetButton); // Remove from available list
                    if (debug) Debug.Log($"[UpgradeSelectionPanel] Successfully mapped {upgrade.UpgradeName} to button '{targetButton.gameObject.name}'");
                }
                else
                {
                    Debug.LogError($"[UpgradeSelectionPanel] Could not find button for upgrade: {upgrade.UpgradeName} (BodyPart: {bodyPart}). Available buttons: {availableButtons.Count}");
                }
            }

            // Final pass: Add any buttons that were initialized but not in the map
            // This handles cases where matching failed but buttons were still initialized
            if (upgradeButtons != null)
            {
                foreach (var button in upgradeButtons)
                {
                    if (button != null && button.Upgrade != null)
                    {
                        string upgradeName = button.Upgrade.UpgradeName;
                        if (!string.IsNullOrEmpty(upgradeName) && !buttonMap.ContainsKey(upgradeName))
                        {
                            buttonMap[upgradeName] = button;
                            if (debug) Debug.Log($"[UpgradeSelectionPanel] Added button '{button.gameObject.name}' to map for {upgradeName}");
                        }
                    }
                }
            }

            if (debug) Debug.Log($"[UpgradeSelectionPanel] Loaded {buttonMap.Count} upgrades into button map (total upgrades: {allUpgrades.Count})");
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
            // Refresh buttons from the map
            foreach (var button in buttonMap.Values)
            {
                if (button != null)
                {
                    button.RefreshButtonState();
                }
            }

            // Fallback: if map is empty, refresh all buttons found in children
            if (buttonMap.Count == 0)
            {
                if (upgradeButtons == null || upgradeButtons.Length == 0)
                {
                    upgradeButtons = GetComponentsInChildren<UpgradeButtonUI>(true);
                }

                if (upgradeButtons != null)
                {
                    foreach (var button in upgradeButtons)
                    {
                        if (button != null)
                        {
                            button.RefreshButtonState();
                        }
                    }
                }

                if (debug) Debug.LogWarning("[UpgradeSelectionPanel] Button map was empty, refreshed buttons directly");
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
        public Dictionary<string, UpgradeButtonUI> GetAllButtons() => new Dictionary<string, UpgradeButtonUI>(buttonMap);
    }
}