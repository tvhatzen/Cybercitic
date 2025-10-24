using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using static GameState;

/// <summary>
/// Main upgrade shop UI controller
/// Left Panel: Visual display of equipped upgrades
/// Center Panel: Selectable upgrade icons
/// Right Panel: Detailed info and purchase button
/// </summary>
public class UpgradeShopUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button doneButton;
    [SerializeField] private TextMeshProUGUI creditsText;
    
    [Header("Center Panel - Upgrade Buttons")]
    [SerializeField] private UpgradeButtonUI coreButton;
    [SerializeField] private UpgradeButtonUI leftArmButton;
    [SerializeField] private UpgradeButtonUI rightArmButton;
    [SerializeField] private UpgradeButtonUI leftLegButton;
    [SerializeField] private UpgradeButtonUI rightLegButton;
    
    [Header("Left Panel - Equipped Display")]
    [SerializeField] private EquippedUpgradeDisplay coreDisplay;
    [SerializeField] private EquippedUpgradeDisplay leftArmDisplay;
    [SerializeField] private EquippedUpgradeDisplay rightArmDisplay;
    [SerializeField] private EquippedUpgradeDisplay leftLegDisplay;
    [SerializeField] private EquippedUpgradeDisplay rightLegDisplay;
    
    [Header("Right Panel - Upgrade Details")]
    [SerializeField] private GameObject detailsPanel;
    [SerializeField] private TextMeshProUGUI upgradeName;
    [SerializeField] private TextMeshProUGUI upgradeDescription;
    [SerializeField] private TextMeshProUGUI statInfo;
    [SerializeField] private Button purchaseButton;
    [SerializeField] private TextMeshProUGUI purchaseButtonText;
    
    [Header("Purchase Button Colors")]
    [SerializeField] private Color canPurchaseColor = Color.green;
    [SerializeField] private Color cannotAffordColor = Color.red;
    [SerializeField] private Color maxLevelColor = Color.gray;

    public enum BodyPart
    {
        Core,
        LeftArm,
        RightArm,
        LeftLeg,
        RightLeg
    }

    private Dictionary<BodyPart, UpgradeButtonUI> upgradeButtons = new Dictionary<BodyPart, UpgradeButtonUI>();
    private Dictionary<BodyPart, EquippedUpgradeDisplay> equippedDisplays = new Dictionary<BodyPart, EquippedUpgradeDisplay>();
    private Upgrade selectedUpgrade;
    private UpgradeButtonUI selectedButton;

    [Header("DEBUG")]
    public bool debug = false;

    private void Awake()
    {
        // button dictionary
        upgradeButtons[BodyPart.Core] = coreButton;
        upgradeButtons[BodyPart.LeftArm] = leftArmButton;
        upgradeButtons[BodyPart.RightArm] = rightArmButton;
        upgradeButtons[BodyPart.LeftLeg] = leftLegButton;
        upgradeButtons[BodyPart.RightLeg] = rightLegButton;

        // display dictionary
        equippedDisplays[BodyPart.Core] = coreDisplay;
        equippedDisplays[BodyPart.LeftArm] = leftArmDisplay;
        equippedDisplays[BodyPart.RightArm] = rightArmDisplay;
        equippedDisplays[BodyPart.LeftLeg] = leftLegDisplay;
        equippedDisplays[BodyPart.RightLeg] = rightLegDisplay;

        // buttons
        if (doneButton != null)
            doneButton.onClick.AddListener(OnDoneClicked);
            
        if (purchaseButton != null)
            purchaseButton.onClick.AddListener(OnPurchaseClicked);
    }

    private void Start()
    {
        LoadUpgrades();

        // Subscribe to centralized Event Bus for credits
        GameEvents.OnCreditsChanged += UpdateCreditsDisplay;
        
        if (CurrencyManager.Instance != null)
        {
            UpdateCreditsDisplay(CurrencyManager.Instance.Credits);
        }

        if (UpgradeManager.Instance != null)
        {
            // Subscribe to centralized Event Bus
            GameEvents.OnUpgradePurchased += OnUpgradePurchased;
            GameEvents.OnAllUpgradesReset += OnAllUpgradesReset;
        }
        
        // hide details panel initially
        if (detailsPanel != null)
            detailsPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        // Unsubscribe from centralized Event Bus
        GameEvents.OnCreditsChanged -= UpdateCreditsDisplay;

        if (UpgradeManager.Instance != null)
        {
            // Unsubscribe from centralized Event Bus
            GameEvents.OnUpgradePurchased -= OnUpgradePurchased;
            GameEvents.OnAllUpgradesReset -= OnAllUpgradesReset;
        }

        if (doneButton != null)
            doneButton.onClick.RemoveListener(OnDoneClicked);
            
        if (purchaseButton != null)
            purchaseButton.onClick.RemoveListener(OnPurchaseClicked);
    }

    private void LoadUpgrades()
    {
        if (UpgradeManager.Instance == null) return;

        var allUpgrades = UpgradeManager.Instance.GetAllUpgrades();

        foreach (var upgrade in allUpgrades)
        {
            if (upgrade == null) continue;

            BodyPart bodyPart = GetBodyPartFromUpgrade(upgrade);
            
            if (upgradeButtons.TryGetValue(bodyPart, out var button))
            {
                button.Initialize(upgrade, this);
            }
        }

        UpdateEquippedDisplay();
    }

    private BodyPart GetBodyPartFromUpgrade(Upgrade upgrade)
    {
        string name = upgrade.UpgradeName.ToLower();

        if (name.Contains("core")) return BodyPart.Core;
        if (name.Contains("larm") || name.Contains("left arm")) return BodyPart.LeftArm;
        if (name.Contains("rarm") || name.Contains("right arm")) return BodyPart.RightArm;
        if (name.Contains("lleg") || name.Contains("left leg")) return BodyPart.LeftLeg;
        if (name.Contains("rleg") || name.Contains("right leg")) return BodyPart.RightLeg;

        return BodyPart.Core;
    }

    public void SelectUpgrade(Upgrade upgrade)
    {
        // Deselect previous button
        if (selectedButton != null)
        {
            selectedButton.SetSelected(false);
        }

        // Find and select new button
        foreach (var button in upgradeButtons.Values)
        {
            if (button != null && button.Upgrade == upgrade)
            {
                button.SetSelected(true);
                selectedButton = button;
                break;
            }
        }

        selectedUpgrade = upgrade;
        UpdateDetailsPanel();
    }

    private void UpdateDetailsPanel()
    {
        if (selectedUpgrade == null)
        {
            if (detailsPanel != null)
                detailsPanel.SetActive(false);
            return;
        }

        // show details panel
        if (detailsPanel != null)
            detailsPanel.SetActive(true);

        // update name
        if (upgradeName != null)
        {
            upgradeName.text = selectedUpgrade.UpgradeName;
        }

        // update description
        if (upgradeDescription != null)
        {
            upgradeDescription.text = selectedUpgrade.Description;
        }

        // update stat info
        if (statInfo != null)
        {
            string statText = GetStatIncreaseInfo(selectedUpgrade);
            statInfo.text = statText;
        }

        // update purchase button
        UpdatePurchaseButton();
    }

    private string GetStatIncreaseInfo(Upgrade upgrade)
    {
        // get the stat type and amount from the upgrade
        float increaseAmount = upgrade.statIncreasePerLevel;
        string statName = "Unknown";
        string displayValue = "";

        // determine stat type based on upgrade type
        switch (upgrade.GetUpgradeType())
        {
            case Upgrade.UpgradeType.Health:
                statName = "Health";
                displayValue = $"+{Mathf.RoundToInt(increaseAmount * 100)}";
                break;
            case Upgrade.UpgradeType.Speed:
                statName = "Speed";
                displayValue = $"+{increaseAmount:F2}";
                break;
            case Upgrade.UpgradeType.Attack:
                statName = "Attack";
                displayValue = $"+{Mathf.RoundToInt(increaseAmount * 100)}";
                break;
            case Upgrade.UpgradeType.Defense:
                statName = "Defense";
                displayValue = $"+{increaseAmount:F2}";
                break;
            case Upgrade.UpgradeType.DodgeChance:
                statName = "Dodge Chance";
                displayValue = $"+{(increaseAmount * 100):F1}%";
                break;
        }

        return $"<b>{statName}:</b> {displayValue} per level\n<b>Current Level:</b> {upgrade.CurrentLevel}/{upgrade.MaxLevel}";
    }

    private void UpdatePurchaseButton()
    {
        if (selectedUpgrade == null || purchaseButton == null || purchaseButtonText == null)
            return;

        bool isMaxLevel = selectedUpgrade.IsMaxLevel;
        bool canUpgrade = selectedUpgrade.CanUpgrade;
        int cost = selectedUpgrade.GetCost();
        int currentCredits = CurrencyManager.Instance != null ? CurrencyManager.Instance.Credits : 0;
        bool canAfford = currentCredits >= cost;

        Image buttonImage = purchaseButton.GetComponent<Image>();

        if (isMaxLevel)
        {
            purchaseButtonText.text = "MAX LEVEL";
            purchaseButtonText.color = maxLevelColor;
            if (buttonImage != null)
                buttonImage.color = maxLevelColor;
            purchaseButton.interactable = false;
        }
        else if (canAfford && canUpgrade)
        {
            purchaseButtonText.text = $"PURCHASE [{cost} ȼ]";
            purchaseButtonText.color = canPurchaseColor;
            if (buttonImage != null)
                buttonImage.color = canPurchaseColor;
            purchaseButton.interactable = true;
        }
        else if (!canUpgrade)
        {
            purchaseButtonText.text = "LOCKED";
            purchaseButtonText.color = maxLevelColor;
            if (buttonImage != null)
                buttonImage.color = maxLevelColor;
            purchaseButton.interactable = false;
        }
        else // Can't afford
        {
            int needed = cost - currentCredits;
            purchaseButtonText.text = $"NEED MORE CREDITS [{needed} ȼ]";
            purchaseButtonText.color = cannotAffordColor;
            if (buttonImage != null)
                buttonImage.color = cannotAffordColor;
            purchaseButton.interactable = false;
        }
    }

    private void OnPurchaseClicked()
    {
        if (selectedUpgrade == null) return;

        bool success = UpgradeManager.Instance.PurchaseUpgrade(selectedUpgrade);

        if (success)
        {
            if(debug) Debug.Log($"[UpgradeShopUI] Successfully purchased {selectedUpgrade.UpgradeName}");
        }
    }

    public void UpdateEquippedDisplay()
    {
        foreach (var kvp in upgradeButtons)
        {
            BodyPart bodyPart = kvp.Key;
            UpgradeButtonUI button = kvp.Value;
            
            if (button != null && button.Upgrade != null)
            {
                if (equippedDisplays.TryGetValue(bodyPart, out var display))
                {
                    display.UpdateDisplay(button.Upgrade);
                }
            }
        }
    }

    private void OnUpgradePurchased(Upgrade upgrade)
    {
        // update all button displays
        foreach (var button in upgradeButtons.Values)
        {
            if (button != null)
                button.UpdateDisplay();
        }

        // update equipped display
        UpdateEquippedDisplay();

        // update details panel if this is the selected upgrade
        if (selectedUpgrade == upgrade)
        {
            UpdateDetailsPanel();
        }

        if(debug) Debug.Log($"[UpgradeShopUI] Upgrade purchased: {upgrade.UpgradeName}");
    }

    private void OnAllUpgradesReset()
    {
        if(debug) Debug.Log("[UpgradeShopUI] All upgrades reset - refreshing displays");
        RefreshAllDisplays();
    }

    private void UpdateCreditsDisplay(int credits)
    {
        if (creditsText != null)
        {
            creditsText.text = $"Credits: {credits}";
        }
        
        // update purchase button when credits change
        if (selectedUpgrade != null)
        {
            UpdatePurchaseButton();
        }
    }

    private void OnDoneClicked()
    {
        gameObject.SetActive(false);

        // reset run stats for retry (keep upgrades and credits)
        if (RunStatsTracker.Instance != null)
            RunStatsTracker.Instance.ResetStatsForRetry();
        
        // reset floor to 1 but keep player upgrades
        if (FloorManager.Instance != null)
            FloorManager.Instance.ResetToFloor1();
        
        // restart the game
        if (GameState.Instance != null)
            GameState.Instance.ChangeState(GameState.GameStates.Playing);
        else
        {
            // reload current scene 
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    public void CloseShop() => OnDoneClicked();
    
    // Refresh all upgrade displays (useful after reset)
    public void RefreshAllDisplays()
    {
        if(debug) Debug.Log("[UpgradeShopUI] Refreshing all upgrade displays");
        
        // update all button displays
        foreach (var button in upgradeButtons.Values)
        {
            if (button != null)
                button.UpdateDisplay();
        }

        // update equipped display
        UpdateEquippedDisplay();
        
        if(debug) Debug.Log("[UpgradeShopUI] All displays refreshed");
    }
}

