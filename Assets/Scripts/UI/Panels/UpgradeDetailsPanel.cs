using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Cybercitic.UI
{
    /// <summary>
    /// Handles the upgrade details panel UI.
    /// Displays upgrade information and purchase button.
    /// </summary>
    public class UpgradeDetailsPanel : MonoBehaviour
    {
        [Header("UI References")]
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

        [Header("Debug")]
        [SerializeField] private bool debug;

        private Upgrade currentUpgrade;
        private ICurrencyService currencyService;

        public void Initialize(ICurrencyService currencyService)
        {
            this.currencyService = currencyService;
            if (detailsPanel != null)
            {
                detailsPanel.SetActive(false);
            }
        }

        public void Show()
        {
            if (detailsPanel != null)
            {
                detailsPanel.SetActive(true);
            }
        }

        public void Hide()
        {
            if (detailsPanel != null)
            {
                detailsPanel.SetActive(false);
            }
            currentUpgrade = null;
        }

        public void UpdateDisplay(Upgrade upgrade, PurchaseButtonState buttonState)
        {
            currentUpgrade = upgrade;

            if (upgrade == null)
            {
                Hide();
                return;
            }

            Show();

            if (upgradeName != null)
            {
                upgradeName.text = upgrade.UpgradeName;
            }

            if (upgradeDescription != null)
            {
                upgradeDescription.text = upgrade.Description;
            }

            if (statInfo != null)
            {
                statInfo.text = UpgradeStatFormatter.FormatStatInfo(upgrade);
            }

            UpdatePurchaseButton(upgrade, buttonState);
        }

        private void UpdatePurchaseButton(Upgrade upgrade, PurchaseButtonState buttonState)
        {
            if (purchaseButton == null || purchaseButtonText == null) return;

            Image buttonImage = purchaseButton.GetComponent<Image>();

            switch (buttonState)
            {
                case PurchaseButtonState.MaxLevel:
                    purchaseButtonText.text = "MAX LEVEL";
                    purchaseButtonText.color = maxLevelColor;
                    if (buttonImage != null) buttonImage.color = maxLevelColor;
                    purchaseButton.interactable = false;
                    break;

                case PurchaseButtonState.CanPurchase:
                    int cost = upgrade.GetCost();
                    purchaseButtonText.text = $"PURCHASE: {cost}";
                    purchaseButtonText.color = canPurchaseColor;
                    if (buttonImage != null) buttonImage.color = canPurchaseColor;
                    purchaseButton.interactable = true;
                    break;

                case PurchaseButtonState.Locked:
                    purchaseButtonText.text = "LOCKED";
                    purchaseButtonText.color = maxLevelColor;
                    if (buttonImage != null) buttonImage.color = maxLevelColor;
                    purchaseButton.interactable = false;
                    break;

                case PurchaseButtonState.CannotAfford:
                    int cost2 = upgrade.GetCost();
                    purchaseButtonText.text = $"COST: {cost2}";
                    purchaseButtonText.color = cannotAffordColor;
                    if (buttonImage != null) buttonImage.color = cannotAffordColor;
                    purchaseButton.interactable = false;
                    break;
            }
        }

        public void SetPurchaseButtonListener(UnityEngine.Events.UnityAction listener)
        {
            if (purchaseButton != null)
            {
                purchaseButton.onClick.RemoveAllListeners();
                purchaseButton.onClick.AddListener(listener);
            }
        }

        public bool IsVisible => detailsPanel != null && detailsPanel.activeSelf;
    }

    public enum PurchaseButtonState
    {
        MaxLevel,
        CanPurchase,
        Locked,
        CannotAfford
    }
}

