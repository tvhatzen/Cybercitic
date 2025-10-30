using UnityEngine;
using UnityEngine.UI;

// Displays an upgrade bar sprite that changes based on the upgrade level
// Attached to a GameObject with an Image component. Provide sprites for each level state
// in order from level 0 (no upgrades) up to max level
[RequireComponent(typeof(Image))]
public class UpgradeBarSprite : MonoBehaviour
{
    [Header("Bindings")]
    [SerializeField] private UpgradeButtonUI sourceButton; // reads level from this button's Upgrade
    [SerializeField] private Image targetImage; // defaults to this GameObject's Image

    [Header("Sprites by Level (index 0 = level 0)")]
    [SerializeField] private Sprite[] levelSprites;

    public bool debug = false;

    private void Awake()
    {
        if (targetImage == null)
            targetImage = GetComponent<Image>();
    }

    private void OnEnable()
    {
        Subscribe();
        Refresh();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void Subscribe()
    {
        GameEvents.OnUpgradePurchased += OnUpgradePurchased;
        GameEvents.OnAllUpgradesReset += OnAllUpgradesReset;
    }

    private void Unsubscribe()
    {
        GameEvents.OnUpgradePurchased -= OnUpgradePurchased;
        GameEvents.OnAllUpgradesReset -= OnAllUpgradesReset;
    }

    private void OnUpgradePurchased(Upgrade _)
    {
        Refresh();
    }

    private void OnAllUpgradesReset()
    {
        Refresh();
    }

    // Public for manual refreshes from other UI scripts if needed
    public void Refresh()
    {
        if (targetImage == null) return;

        int level = 0;
        int max = levelSprites != null ? levelSprites.Length - 1 : 0;

        if (sourceButton != null && sourceButton.Upgrade != null)
        {
            // Clamp into available sprite range; assumes index 0 is level 0 graphic
            level = Mathf.Clamp(sourceButton.Upgrade.CurrentLevel, 0, max);
        }

        Sprite sprite = (levelSprites != null && level >= 0 && level < levelSprites.Length)
            ? levelSprites[level]
            : null;

        targetImage.sprite = sprite;

        if (debug)
            Debug.Log($"[UpgradeBarSprite] Applied level {level} sprite on {name}");
    }
}


