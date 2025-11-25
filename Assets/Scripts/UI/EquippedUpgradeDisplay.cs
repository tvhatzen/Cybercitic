using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

// *** will need to change to use sprites 

// display for equipped upgrades in the left panel - 3 square level indicators
public class EquippedUpgradeDisplay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Transform levelIndicatorParent;
    
    [Header("Level Indicator Prefabs")]
    [SerializeField] private GameObject purchasedSquarePrefab;
    [SerializeField] private GameObject nonPurchasedSquarePrefab;
    
    [Header("Settings")]
    [SerializeField] private Upgrade.BodyPart bodyPart;
    
    private List<GameObject> levelSquares = new List<GameObject>();

    public Upgrade.BodyPart BodyPart => bodyPart;
    
    public bool debug = false;

    // updates the display with the highest level upgrade
    public void UpdateDisplay(Upgrade upgrade)
    {
        if (iconImage == null) return;

        // update icon
        if (upgrade != null && upgrade.CurrentLevel > 0)
        {
            iconImage.sprite = upgrade.Icon;
            iconImage.color = Color.white;
            iconImage.enabled = true;
        }
        else
        {
            // no upgrade / level 0 - hide or gray out
            iconImage.sprite = null;
            iconImage.color = Color.gray;
            iconImage.enabled = false;
        }

        // update level indicators
        UpdateLevelIndicators(upgrade);
    }

    private void UpdateLevelIndicators(Upgrade upgrade)
    {
        // clear current squares
        foreach (var square in levelSquares)
        {
            if (square != null)
                Destroy(square);
        }
        levelSquares.Clear();

        if (levelIndicatorParent == null || purchasedSquarePrefab == null || nonPurchasedSquarePrefab == null)
            return;

        if (upgrade == null || upgrade.CurrentLevel == 0)
        {
            // show 3 empty squares
            for (int i = 0; i < 3; i++)
            {
                GameObject square = Instantiate(nonPurchasedSquarePrefab, levelIndicatorParent);
                levelSquares.Add(square);
            }
        }
        else
        {
            // show upgrade level
            int maxLevel = upgrade.MaxLevel;
            int currentLevel = upgrade.CurrentLevel;

            for (int i = 0; i < maxLevel; i++)
            {
                GameObject prefab = (i < currentLevel) ? purchasedSquarePrefab : nonPurchasedSquarePrefab;
                GameObject square = Instantiate(prefab, levelIndicatorParent);
                levelSquares.Add(square);
            }
        }
    }

    private void OnDestroy()
    {
        foreach (var square in levelSquares)
        {
            if (square != null)
                Destroy(square);
        }
        levelSquares.Clear();
    }
}

