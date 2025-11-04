using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

// Shows icon and level indicators, selects upgrade when clicked
public class UpgradeButtonUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button button;
    
    [Header("Level Indicator Prefabs")]
    [SerializeField] private GameObject purchasedSquarePrefab;
    [SerializeField] private GameObject nonPurchasedSquarePrefab;
    [SerializeField] private Transform levelIndicatorParent;

    [Header("Enables/Disabled Sprites")]
    [SerializeField] private Sprite canPurchase;
    [SerializeField] private Sprite cannotPurchase;

    private Upgrade upgrade;
    private UpgradeShopUI shopUI;
    private List<GameObject> levelSquares = new List<GameObject>();
    private bool isSelected = false;

    public Upgrade Upgrade => upgrade;
    
    public bool debug = false;

    private void Awake()
    {
        HideSquares();

        if (button == null)
            button = GetComponent<Button>();
            
        if (button != null)
            button.onClick.AddListener(OnButtonClicked);
    }

    public void Initialize(Upgrade upgradeData, UpgradeShopUI shop)
    {
        upgrade = upgradeData;
        shopUI = shop;
        
        UpdateDisplay();

        // Also refresh any UpgradeBarSprite components beneath this button
        RefreshAttachedUpgradeBars();
    }

    public void HideSquares()
    {
        foreach (var square in levelSquares)
        {
            if (square != null)
                square.SetActive(false);
        }
    }

    public void ShowSquares()
    {
        foreach (var square in levelSquares)
        {
            if (square != null)
                square.SetActive(true);
        }
    }

    public void UpdateDisplay()
    {
        if (upgrade == null) return;

        UpdateLevelIndicators();
        RefreshAttachedUpgradeBars();
        UpdateButtonState();
    }

    private void UpdateLevelIndicators()
    {
        // clear existing squares
        foreach (var square in levelSquares)
        {
            if (square != null)
                Destroy(square);
        }
        levelSquares.Clear();

        if (levelIndicatorParent == null || purchasedSquarePrefab == null || nonPurchasedSquarePrefab == null)
            return;

        // create squares for max level
        int maxLevel = upgrade.MaxLevel;
        int currentLevel = upgrade.CurrentLevel;

        for (int i = 0; i < maxLevel; i++)
        {
            GameObject prefab = (i < currentLevel) ? purchasedSquarePrefab : nonPurchasedSquarePrefab;
            GameObject square = Instantiate(prefab, levelIndicatorParent);
            levelSquares.Add(square);
        }
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
    }

    private void OnButtonClicked()
    {
        if (upgrade == null || shopUI == null) return;
        
        // notify shop UI that this was selected
        shopUI.SelectUpgrade(upgrade);
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(OnButtonClicked);
            
        // clean up squares
        foreach (var square in levelSquares)
        {
            if (square != null)
                Destroy(square);
        }
        levelSquares.Clear();
    }

    private void RefreshAttachedUpgradeBars()
    {
        var bars = GetComponentsInChildren<UpgradeBarSprite>(true);
        foreach (var bar in bars)
        {
            // If not bound, try to bind to this button automatically
            var field = bar;
            if (bar != null) bar.Refresh();
        }
    }


    // Updates the button state based on whether the upgrade is maxed out and if the player can afford it.
    public void RefreshButtonState()
    {
        UpdateButtonState();
    }

    private void UpdateButtonState()
    {
        if (upgrade == null || button == null) return;

        // Check if upgrade is maxed out
        bool isMaxedOut = upgrade.CurrentLevel >= upgrade.MaxLevel;
        
        // Check if player can afford the upgrade
        bool canAfford = shopUI != null && shopUI.CanAffordUpgrade(upgrade);
        
        // Disable button if maxed out or can't afford, enable otherwise
        bool shouldBeEnabled = !isMaxedOut && canAfford;
        SetButtonState(shouldBeEnabled);
    }

    public void SetButtonState(bool state)
    {
        if (button == null) return;

        var buttonVisual = button.GetComponent<Image>();

        //button.interactable = state;
        if (state == false)
        {
            buttonVisual.sprite = cannotPurchase;
        }
        else if(state == true)
        {
            buttonVisual.sprite = canPurchase;
        }


        if (debug)
        {
            Debug.Log($"Button state set to {(state ? "enabled" : "disabled")} for upgrade: {upgrade?.name ?? "null"}");
        }
    }
}