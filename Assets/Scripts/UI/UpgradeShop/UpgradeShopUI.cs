using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using static GameManager;

namespace Cybercitic.UI
{
    /// <summary>
    /// Main upgrade shop UI coordinator
    /// Coordinates between panels and handles UI lifecycle
    /// Business logic is delegated to UpgradeShopController
    /// </summary>
    public class UpgradeShopUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button doneButton;
        [SerializeField] private TextMeshProUGUI scrapText;
        [SerializeField] private Sprite scrapSprite;

        [Header("Panel References")]
        [SerializeField] private UpgradeSelectionPanel selectionPanel;
        [SerializeField] private EquippedDisplayPanel equippedPanel;
        [SerializeField] private UpgradeDetailsPanel detailsPanel;

        [Header("Upgrade Buttons (Legacy - for backward compatibility)")]
        [SerializeField] private UpgradeButtonUI[] upgradeButtons;

        [Header("Equipped Displays (Legacy - for backward compatibility)")]
        [SerializeField] private EquippedUpgradeDisplay[] equippedDisplays;

        [Header("Debug")]
        [SerializeField] private bool debug;

        private UpgradeShopController controller;
        private ICurrencyService currencyService;
        private IUpgradeService upgradeService;

        private void Awake()
        {
            InitializeServices();
            InitializeController();
            InitializePanels();
            SetupButtonListeners();
        }

        private void Start()
        {
            SubscribeToEvents();
            LoadUpgrades();
            UpdateCreditsDisplay(currencyService.Credits);
            HideDetailsPanel();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
            CleanupButtonListeners();
        }

        private void InitializeServices()
        {
            currencyService = new CurrencyManagerAdapter();
            upgradeService = new UpgradeManagerAdapter();
        }

        private void InitializeController()
        {
            controller = new UpgradeShopController(currencyService, upgradeService, debug);
            controller.OnUpgradeSelected += HandleUpgradeSelected;
            controller.OnUpgradePurchased += HandleUpgradePurchased;
        }

        private void InitializePanels()
        {
            if (detailsPanel != null)
            {
                detailsPanel.Initialize(currencyService);
                detailsPanel.SetPurchaseButtonListener(OnPurchaseClicked);
            }

            if (selectionPanel != null)
            {
                selectionPanel.Initialize();
            }

            if (equippedPanel != null)
            {
                equippedPanel.Initialize();
                RegisterEquippedDisplays();
            }
        }

        private void RegisterEquippedDisplays()
        {
            if (equippedDisplays == null || equippedPanel == null) return;

            foreach (var display in equippedDisplays)
            {
                if (display != null)
                {
                    // convert from old BodyPart enum to new one
                    Upgrade.BodyPart bodyPart = ConvertBodyPart(display.BodyPart);
                    equippedPanel.RegisterDisplay(bodyPart, display);
                }
            }
        }

        private Upgrade.BodyPart ConvertBodyPart(Upgrade.BodyPart bodyPart)
        {
            return bodyPart;
        }

        private void SetupButtonListeners()
        {
            if (doneButton != null)
            {
                doneButton.onClick.AddListener(OnDoneClicked);
            }
        }

        private void CleanupButtonListeners()
        {
            if (doneButton != null)
            {
                doneButton.onClick.RemoveListener(OnDoneClicked);
            }
        }

        private void SubscribeToEvents()
        {
            GameEvents.OnCreditsChanged += UpdateCreditsDisplay;
            GameEvents.OnUpgradePurchased += OnUpgradePurchased;
            GameEvents.OnAllUpgradesReset += OnAllUpgradesReset;
        }

        private void UnsubscribeFromEvents()
        {
            GameEvents.OnCreditsChanged -= UpdateCreditsDisplay;
            GameEvents.OnUpgradePurchased -= OnUpgradePurchased;
            GameEvents.OnAllUpgradesReset -= OnAllUpgradesReset;
        }

        private void LoadUpgrades()
        {
            if (selectionPanel != null)
            {
                selectionPanel.LoadUpgrades(upgradeService, controller);
            }
            else
            {
                // Fallback to old initialization
                LoadUpgradesLegacy();
            }

            UpdateEquippedDisplay();
        }

        private void LoadUpgradesLegacy()
        {
            if (upgradeButtons == null || upgradeService == null) return;

            var allUpgrades = upgradeService.GetAllUpgrades();

            foreach (var upgrade in allUpgrades)
            {
                if (upgrade == null) continue;

                Upgrade.BodyPart bodyPart = upgrade.GetBodyPart();

                foreach (var button in upgradeButtons)
                {
                    if (button != null && button.Upgrade == null)
                    {
                        button.Initialize(upgrade, controller);
                        break;
                    }
                }
            }
        }

        public void SelectUpgrade(Upgrade upgrade)
        {
            controller?.SelectUpgrade(upgrade);
        }

        private void HandleUpgradeSelected(Upgrade upgrade)
        {
            if (selectionPanel != null)
            {
                selectionPanel.SelectUpgrade(upgrade);
            }

            UpdateDetailsPanel(upgrade);
            UpdateLevelSquaresVisibility();
        }

        private void UpdateDetailsPanel(Upgrade upgrade)
        {
            if (detailsPanel == null) return;

            if (upgrade == null)
            {
                detailsPanel.Hide();
                return;
            }

            PurchaseButtonState buttonState = controller.GetPurchaseButtonState(upgrade);
            detailsPanel.UpdateDisplay(upgrade, buttonState);
        }

        private void UpdateLevelSquaresVisibility()
        {
            if (selectionPanel == null) return;

            selectionPanel.HideAllLevelSquares();

            if (detailsPanel != null && detailsPanel.IsVisible)
            {
                var selectedButton = selectionPanel.GetSelectedButton();
                if (selectedButton != null)
                {
                    selectionPanel.ShowLevelSquares(selectedButton);
                }
            }
        }

        private void OnPurchaseClicked()
        {
            Upgrade selected = controller?.SelectedUpgrade;
            if (selected == null) return;

            bool success = controller.PurchaseUpgrade(selected);

            if (success && debug)
            {
                Debug.Log($"[UpgradeShopUI] Successfully purchased {selected.UpgradeName}");
            }
        }

        private void UpdateEquippedDisplay()
        {
            if (equippedPanel != null && selectionPanel != null)
            {
                var buttons = selectionPanel.GetAllButtons();
                equippedPanel.UpdateAllDisplays(buttons);
            }
            else
            {
                UpdateEquippedDisplayLegacy();
            }
        }

        private void UpdateEquippedDisplayLegacy()
        {
            if (equippedDisplays == null || upgradeButtons == null) return;

            foreach (var button in upgradeButtons)
            {
                if (button != null && button.Upgrade != null)
                {
                    Upgrade.BodyPart bodyPart = button.Upgrade.GetBodyPart();
                    foreach (var display in equippedDisplays)
                    {
                        if (display != null && ConvertBodyPart(display.BodyPart) == bodyPart)
                        {
                            display.UpdateDisplay(button.Upgrade);
                            break;
                        }
                    }
                }
            }
        }

        private void HandleUpgradePurchased(Upgrade upgrade)
        {
            if (selectionPanel != null)
            {
                selectionPanel.UpdateAllDisplays();
            }
            else
            {
                UpdateAllButtonDisplaysLegacy();
            }

            UpdateEquippedDisplay();

            if (controller?.SelectedUpgrade == upgrade)
            {
                UpdateDetailsPanel(upgrade);
            }

            if (debug)
            {
                Debug.Log($"[UpgradeShopUI] Upgrade purchased: {upgrade.UpgradeName}");
            }
        }

        private void UpdateAllButtonDisplaysLegacy()
        {
            if (upgradeButtons == null) return;

            foreach (var button in upgradeButtons)
            {
                if (button != null)
                {
                    button.UpdateDisplay();
                }
            }
        }

        private void OnUpgradePurchased(Upgrade upgrade)
        {
            HandleUpgradePurchased(upgrade);
        }

        private void OnAllUpgradesReset()
        {
            if (debug) Debug.Log("[UpgradeShopUI] All upgrades reset - refreshing displays");

            RefreshAllDisplays();
        }

        private void UpdateCreditsDisplay(int credits)
        {
            if (scrapText != null)
            {
                scrapText.text = $"{credits}";
            }

            if (controller?.SelectedUpgrade != null)
            {
                UpdateDetailsPanel(controller.SelectedUpgrade);
            }

            if (selectionPanel != null)
            {
                selectionPanel.RefreshAllButtonStates();
            }
            else
            {
                RefreshAllButtonStatesLegacy();
            }
        }

        private void RefreshAllButtonStatesLegacy()
        {
            if (upgradeButtons == null) return;

            foreach (var button in upgradeButtons)
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

            if (RunStatsTracker.Instance != null)
            {
                RunStatsTracker.Instance.ResetStatsForRetry();
            }

            if (FloorManager.Instance != null)
            {
                FloorManager.Instance.ResetToFloor1();
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.ChangeState(GameStates.Playing);
            }
            else
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }

        public void CloseShop() => OnDoneClicked();

        private void RefreshAllDisplays()
        {
            if (debug) Debug.Log("[UpgradeShopUI] Refreshing all upgrade displays");

            if (selectionPanel != null)
            {
                selectionPanel.UpdateAllDisplays();
            }
            else
            {
                UpdateAllButtonDisplaysLegacy();
            }

            UpdateEquippedDisplay();

            if (debug) Debug.Log("[UpgradeShopUI] All displays refreshed");
        }

        public void ShowDetailsPanel()
        {
            if (detailsPanel != null)
            {
                detailsPanel.Show();
            }
            UpdateLevelSquaresVisibility();
        }

        public void HideDetailsPanel()
        {
            if (detailsPanel != null)
            {
                detailsPanel.Hide();
            }

            if (selectionPanel != null)
            {
                selectionPanel.HideAllLevelSquares();
            }
        }

        public bool CanAffordUpgrade(Upgrade upgrade)
        {
            return controller != null && controller.CanAffordUpgrade(upgrade);
        }
    }
}
