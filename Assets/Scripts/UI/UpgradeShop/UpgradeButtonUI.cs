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
        {
            // First try to get Button from this GameObject
            button = GetComponent<Button>();
            
            // If not found, search in children (Button component is on "Button" child GameObject)
            if (button == null)
            {
                button = GetComponentInChildren<Button>(true);
            }
        }
            
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
        
        // Green (canPurchase) when: not maxed out AND can afford
        // Red (cannotPurchase) when: maxed out OR can't afford
        bool shouldBeEnabled = !isMaxedOut && canAfford;
        
        if (debug)
        {
            int cost = upgrade.GetCost();
            Debug.Log($"[UpgradeButtonUI] {upgrade.UpgradeName}: isMaxedOut={isMaxedOut}, canAfford={canAfford}, cost={cost}, shouldBeEnabled={shouldBeEnabled}");
        }
        
        SetButtonState(shouldBeEnabled);
    }

    public void SetButtonState(bool state)
    {
        if (button == null) return;

        Image buttonVisual = null;
        
        // First, try to get Image from the Button GameObject itself (standard Unity Button setup)
        if (button.gameObject != null)
        {
            buttonVisual = button.gameObject.GetComponent<Image>();
        }
        
        // If not found, search in children of the Button GameObject
        if (buttonVisual == null && button.gameObject != null)
        {
            buttonVisual = button.gameObject.GetComponentInChildren<Image>();
        }
        
        // If still not found, search from this GameObject (parent) for any Image in the Button's hierarchy
        if (buttonVisual == null)
        {
            // Get all Images in children of this GameObject
            Image[] allImages = GetComponentsInChildren<Image>(true);
            
            // Find the Image that's part of the button's GameObject or its children
            if (button.gameObject != null)
            {
                foreach (var img in allImages)
                {
                    if (img != null && (img.transform == button.transform || img.transform.IsChildOf(button.transform)))
                    {
                        buttonVisual = img;
                        break;
                    }
                }
            }
        }

        if (buttonVisual == null)
        {
            if (debug) Debug.LogWarning($"Button Image component not found for upgrade: {upgrade?.name ?? "null"}. Button GameObject: {button?.gameObject?.name ?? "null"}");
            return;
        }

        if (state == false)
        {
            if (cannotPurchase != null)
            {
                buttonVisual.sprite = cannotPurchase;
                if (debug) Debug.Log($"Set {upgrade?.name ?? "null"} button to cannotPurchase sprite");
            }
            else if (debug)
            {
                Debug.LogWarning($"CannotPurchase sprite is null for upgrade: {upgrade?.name ?? "null"}");
            }
        }
        else if(state == true)
        {
            if (canPurchase != null)
            {
                buttonVisual.sprite = canPurchase;
                if (debug) Debug.Log($"Set {upgrade?.name ?? "null"} button to canPurchase sprite");
            }
            else if (debug)
            {
                Debug.LogWarning($"CanPurchase sprite is null for upgrade: {upgrade?.name ?? "null"}");
            }
        }

        if (debug) Debug.Log($"Button state set to {(state ? "enabled" : "disabled")} for upgrade: {upgrade?.name ?? "null"}");
    }
}