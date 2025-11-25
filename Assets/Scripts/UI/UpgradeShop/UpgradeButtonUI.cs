using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Cybercitic.UI;

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
    private UpgradeShopController controller;
    private List<GameObject> levelSquares = new List<GameObject>();
    private bool isSelected = false;

    public Upgrade Upgrade => upgrade;
    
    [SerializeField] private bool debug = false;

    private void Awake()
    {
        HideSquares();

        if (button == null)
            button = GetComponent<Button>();
            
        if (button != null)
            button.onClick.AddListener(OnButtonClicked);
    }

    public void Initialize(Upgrade upgradeData, UpgradeShopController shopController)
    {
        upgrade = upgradeData;
        controller = shopController;
        
        UpdateDisplay();

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

        RefreshAttachedUpgradeBars();
        UpdateButtonState();
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
    }

    private void OnButtonClicked()
    {
        if (upgrade == null || controller == null) return;
        
        // notify controller that this was selected
        controller.SelectUpgrade(upgrade);
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
            // if not bound, try to bind to this button automatically
            var field = bar;
            if (bar != null) bar.Refresh();
        }
    }


    // updates the button state based on whether the upgrade is maxed out and if the player can afford it.
    public void RefreshButtonState()
    {
        UpdateButtonState();
    }

    private void UpdateButtonState()
    {
        if (upgrade == null || button == null) return;

        // check if upgrade is maxed out
        bool isMaxedOut = upgrade.CurrentLevel >= upgrade.MaxLevel;
        
        // check if player can afford the upgrade
        bool canAfford = controller != null && controller.CanAffordUpgrade(upgrade);
        
        // disable button if maxed out or can't afford, enable otherwise
        bool shouldBeEnabled = !isMaxedOut && canAfford;
        SetButtonState(shouldBeEnabled);
    }

    public void SetButtonState(bool state)
    {
        if (button == null) return;

        var buttonVisual = button.GetComponent<Image>();

        if (state == false)
        {
            buttonVisual.sprite = cannotPurchase;
        }
        else if(state == true)
        {
            buttonVisual.sprite = canPurchase;
        }

        if (debug) Debug.Log($"Button state set to {(state ? "enabled" : "disabled")} for upgrade: {upgrade?.name ?? "null"}");
    }
}