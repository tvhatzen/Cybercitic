using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

// Shows icon and level indicators, selects upgrade when clicked
public class UpgradeButtonUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button button;
    [SerializeField] private Image iconImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TextMeshProUGUI nameText;
    
    [Header("Level Indicator Prefabs")]
    [SerializeField] private GameObject purchasedSquarePrefab;
    [SerializeField] private GameObject nonPurchasedSquarePrefab;
    [SerializeField] private Transform levelIndicatorParent;
    
    [Header("Selection Visual")]
    [SerializeField] private Color selectedColor = new Color(1f, 1f, 0.5f, 1f); // yellow
    [SerializeField] private Color normalColor = Color.white;
    
    private Upgrade upgrade;
    private UpgradeShopUI shopUI;
    private List<GameObject> levelSquares = new List<GameObject>();
    private bool isSelected = false;

    public Upgrade Upgrade => upgrade;
    
    public bool debug = false;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();
            
        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();
            
        if (button != null)
            button.onClick.AddListener(OnButtonClicked);
    }

    public void Initialize(Upgrade upgradeData, UpgradeShopUI shop)
    {
        upgrade = upgradeData;
        shopUI = shop;
        
        UpdateDisplay();
    }

    public void UpdateDisplay()
    {
        if (upgrade == null) return;

        // update icon
        if (iconImage != null)
        {
            iconImage.sprite = upgrade.Icon;
            iconImage.color = Color.white;
        }

        // update name text
        if (nameText != null)
        {
            nameText.text = upgrade.UpgradeName;
        }

        // update level indicators
        UpdateLevelIndicators();
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
        
        if (backgroundImage != null)
        {
            backgroundImage.color = selected ? selectedColor : normalColor;
        }
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
}

