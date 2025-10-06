using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameState;

public class ShopUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform upgradeContainer;
    [SerializeField] private GameObject upgradeButtonPrefab;
    [SerializeField] private TextMeshProUGUI creditsText;
    [SerializeField] private Button closeButton;
    
    [Header("Upgrade Display")]
    [SerializeField] private int maxUpgradesPerRow = 3;
    [SerializeField] private float buttonSpacing = 10f;

    private List<GameObject> upgradeButtons = new List<GameObject>();
    private List<Upgrade> availableUpgrades = new List<Upgrade>();

    [Header("Upgrade Skill Slots")]
    public List<Transform> skillSlots = new List<Transform>();

    private void Start()
    {
        SetupShop();
        if (CurrencyManager.Instance != null)
            UpdateCreditsDisplay(CurrencyManager.Instance.Credits);
        
        // Subscribe to events
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnCreditsChanged += UpdateCreditsDisplay;
            
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseShop);
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnCreditsChanged -= UpdateCreditsDisplay;
    }

    private void SetupShop()
    {
        if (UpgradeManager.Instance == null) return;
        
        availableUpgrades = UpgradeManager.Instance.GetAvailableUpgrades();
        CreateUpgradeButtons();
    }

    private void CreateUpgradeButtons()
    {
        // Clear existing buttons
        foreach (var button in upgradeButtons)
        {
            if (button != null)
                Destroy(button);
        }
        upgradeButtons.Clear();

        // Create new buttons
        for (int i = 0; i < availableUpgrades.Count; i++)
        {
            CreateUpgradeButton(availableUpgrades[i], i);
        }
    }

    private void CreateUpgradeButton(Upgrade upgrade, int index)
    {
        if (upgradeButtonPrefab == null || upgradeContainer == null) return;

        GameObject buttonObj = Instantiate(upgradeButtonPrefab, upgradeContainer);
        upgradeButtons.Add(buttonObj);

        // Setup button components
        Button button = buttonObj.GetComponent<Button>();
        Image iconImage = buttonObj.transform.Find("Icon")?.GetComponent<Image>();
        TextMeshProUGUI nameText = buttonObj.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI levelText = buttonObj.transform.Find("Level")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI costText = buttonObj.transform.Find("Cost")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI descriptionText = buttonObj.transform.Find("Description")?.GetComponent<TextMeshProUGUI>();

        // Set button data
        if (iconImage != null)
            iconImage.sprite = upgrade.Icon;
            
        if (nameText != null)
            nameText.text = upgrade.UpgradeName;
            
        if (levelText != null)
            levelText.text = $"Level: {upgrade.CurrentLevel}/{upgrade.MaxLevel}";
            
        if (costText != null)
        {
            int cost = upgrade.GetCost();
            costText.text = cost == int.MaxValue ? "MAX" : $"{cost} Credits";
        }
        
        if (descriptionText != null)
            descriptionText.text = upgrade.Description;

        // Setup button functionality
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => PurchaseUpgrade(upgrade, buttonObj));
        }

        // Update button state
        UpdateUpgradeButtonState(upgrade, buttonObj);
    }

    private void PurchaseUpgrade(Upgrade upgrade, GameObject buttonObj)
    {
        if (UpgradeManager.Instance == null) return;

        bool success = UpgradeManager.Instance.PurchaseUpgrade(upgrade);
        
        if (success)
        {
            // Update the button after successful purchase
            UpdateUpgradeButtonState(upgrade, buttonObj);
            Debug.Log($"Purchased {upgrade.UpgradeName} level {upgrade.CurrentLevel}");
        }
        else
        {
            Debug.Log($"Failed to purchase {upgrade.UpgradeName}");
        }
    }

    private void UpdateUpgradeButtonState(Upgrade upgrade, GameObject buttonObj)
    {
        Button button = buttonObj.GetComponent<Button>();
        TextMeshProUGUI costText = buttonObj.transform.Find("Cost")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI levelText = buttonObj.transform.Find("Level")?.GetComponent<TextMeshProUGUI>();

        if (button != null)
        {
            // Disable button if max level reached or insufficient funds
            bool canPurchase = upgrade.CanUpgrade && 
                             CurrencyManager.Instance != null && 
                             CurrencyManager.Instance.Credits >= upgrade.GetCost();
            
            button.interactable = canPurchase;
        }

        if (levelText != null)
            levelText.text = $"Level: {upgrade.CurrentLevel}/{upgrade.MaxLevel}";

        if (costText != null)
        {
            int cost = upgrade.GetCost();
            costText.text = cost == int.MaxValue ? "MAX" : $"{cost} Credits";
        }
    }

    private void UpdateCreditsDisplay(int credits)
    {
        if (creditsText != null)
        {
            creditsText.text = $"Credits: {credits}";
        }
    }

    public void RefreshShop()
    {
        SetupShop();
    }

    public void OpenShop()
    {
        gameObject.SetActive(true);
        RefreshShop();
    }

    public void CloseShop()
    {
        GameState.Instance.ChangeState(GameStates.Results);
    }

    public void ToggleShop()
    {
        if (gameObject.activeSelf)
            CloseShop();
        else
            OpenShop();
    }
}