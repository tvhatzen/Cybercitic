using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using static GameState;
using Unity.VisualScripting;

/// <summary>
/// Main upgrade shop UI controller
/// Left Panel: Visual display of equipped upgrades
/// Center Panel: Selectable upgrade icons
/// Right Panel: Detailed info and purchase button
/// </summary>
public class UpgradeShopUI : MonoBehaviour
{
    #region Panel Elements

    [Header("UI References")]
    [SerializeField] private Button doneButton;
    [SerializeField] private TextMeshProUGUI scrapText;
    [SerializeField] private Sprite scrapSprite; // make it appear higher Y

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

    #endregion

    #region Other Variables
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

    public bool debug = false;

    #endregion

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

        GameEvents.OnCreditsChanged += UpdateCreditsDisplay;
        
        if (CurrencyManager.Instance != null)
        {
            UpdateCreditsDisplay(CurrencyManager.Instance.Credits);
        }

        if (UpgradeManager.Instance != null)
        {
            GameEvents.OnUpgradePurchased += OnUpgradePurchased;
            GameEvents.OnAllUpgradesReset += OnAllUpgradesReset;
        }
        
        // hide details panel initially
        if (detailsPanel != null)
            detailsPanel.SetActive(false);
        // ensure level squares are hidden at start
        HideAllSquares();
    }

    private void OnDestroy()
    {
        GameEvents.OnCreditsChanged -= UpdateCreditsDisplay;

        if (UpgradeManager.Instance != null)
        {
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
        if (name.Contains("left arm")) return BodyPart.LeftArm;
        if (name.Contains("right arm")) return BodyPart.RightArm;
        if (name.Contains("left leg")) return BodyPart.LeftLeg;
        if (name.Contains("right leg")) return BodyPart.RightLeg;

        return BodyPart.Core;
    }

    public void SelectUpgrade(Upgrade upgrade)
    {
        // deselect previous button
        if (selectedButton != null)
        {
            selectedButton.SetSelected(false);
        }

        // find and select new button
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
        UpdateLevelSquaresVisibility();
    }

    private void UpdateDetailsPanel()
    {
        if (selectedUpgrade == null)
        {
            if (detailsPanel != null)
                detailsPanel.SetActive(false);
            // hide all squares when no selection or panel closed
            HideAllSquares();
            return;
        }

        // show details panel
        if (detailsPanel != null) { detailsPanel.SetActive(true); }

        // update name
        if (upgradeName != null) { upgradeName.text = selectedUpgrade.UpgradeName; }

        // update description
        if (upgradeDescription != null) { upgradeDescription.text = selectedUpgrade.Description; }

        // update stat info
        if (statInfo != null)
        {
            string statText = GetStatIncreaseInfo(selectedUpgrade);
            statInfo.text = statText;
        }

        // show unique square level pips only for selected when details are open
        UpdateLevelSquaresVisibility();

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
                statName = "Attack Speed";
                displayValue = $"+{(increaseAmount * 100):F2}%";
                break;
            case Upgrade.UpgradeType.Attack:
                statName = "Attack";
                displayValue = $"+{Mathf.RoundToInt(increaseAmount * 100)}";
                break;
            case Upgrade.UpgradeType.Defense:
                statName = "Defense";
                displayValue = $"+{(increaseAmount * 100):F2}%";
                break;
            case Upgrade.UpgradeType.DodgeChance:
                statName = "Dodge Chance";
                displayValue = $"+{(increaseAmount * 100):F1}%";
                break;
        }

        return $"<b>{statName} {displayValue} \n<b>Level:</b> {upgrade.CurrentLevel}/{upgrade.MaxLevel}";
    }

    // change the text of the purchase button depending on credits
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

        if (isMaxLevel) // upgrade maxed out
        {
            purchaseButtonText.text = "MAX LEVEL";
            purchaseButtonText.color = maxLevelColor;
            if (buttonImage != null)
                buttonImage.color = maxLevelColor;
            purchaseButton.interactable = false;
        }
        else if (canAfford && canUpgrade) // can purchase
        {
            string spriteTag = scrapSprite != null ? $"<sprite name=\"{scrapSprite.name}\">" : "";
            // set position offset
            purchaseButtonText.text = $"{spriteTag} PURCHASE: {cost}";
            purchaseButtonText.color = canPurchaseColor;
            if (buttonImage != null)
                buttonImage.color = canPurchaseColor;
            purchaseButton.interactable = true;
        }
        else if (!canUpgrade) // can't purchase
        {
            purchaseButtonText.text = "LOCKED";
            purchaseButtonText.color = maxLevelColor;
            if (buttonImage != null)
                buttonImage.color = maxLevelColor;
            purchaseButton.interactable = false;
        }
        else // can't afford
        {
            string spriteTag = scrapSprite != null ? $"<sprite name=\"{scrapSprite.name}\">" : "";
            // set position offset
            purchaseButtonText.text = $"{spriteTag} NEEDED: {cost}";
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

    private void UpdateEquippedDisplay()
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

    private void UpdateCreditsDisplay(int scrap)
    {
        if (scrapText != null)
        {
            string spriteTag = scrapSprite != null ? $"<sprite name=\"{scrapSprite.name}\">" : "";
            scrapText.text = $"{spriteTag}: {scrap}";
        }
        
        // update purchase button when credits change
        if (selectedUpgrade != null)
        {
            UpdatePurchaseButton();
        }
        
        // refresh all upgrade button states when credits change
        foreach (var button in upgradeButtons.Values)
        {
            if (button != null)
            {
                button.RefreshButtonState();
            }
        }
    }

    private void OnDoneClicked()
    {
        gameObject.SetActive(false);

        // reset run stats for retry (keep upgrades and credits)
        if (RunStatsTracker.Instance != null)
            RunStatsTracker.Instance.ResetStatsForRetry();
        
        // reset floor to 1 
        if (FloorManager.Instance != null)
            FloorManager.Instance.ResetToFloor1();
        
        // restart the game
        if (GameState.Instance != null)
            GameState.Instance.ChangeState(GameStates.Playing);
        else
        {
            // reload current scene 
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    public void CloseShop() => OnDoneClicked();
    
    // refresh all upgrade displays (useful after reset)
    private void RefreshAllDisplays()
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

    public void ShowDetailsPanel()
    {
        if (detailsPanel != null)
            detailsPanel.SetActive(true);
        UpdateLevelSquaresVisibility();
    }

    public void HideDetailsPanel()
    {
        if (detailsPanel != null)
            detailsPanel.SetActive(false);
        HideAllSquares();
    }

	private void UpdateLevelSquaresVisibility()
	{
		// Hide all by default
		HideAllSquares();
		// Show only for the selected button when details panel is open
		if (detailsPanel != null && detailsPanel.activeSelf && selectedButton != null)
		{
			//selectedButton.ShowSquares();
		}
	}

    private void HideAllSquares()
    {
        foreach (var button in upgradeButtons.Values)
        {
            if (button != null)
            {
                button.HideSquares();
            }
        }
    }

    /// Checks if the player can afford to purchase the given upgrade.
    public bool CanAffordUpgrade(Upgrade upgrade)
    {
        if (upgrade == null) return false;

        int cost = upgrade.GetCost();
        int currentCredits = CurrencyManager.Instance != null ? CurrencyManager.Instance.Credits : 0;
        
        return currentCredits >= cost;
    }

}