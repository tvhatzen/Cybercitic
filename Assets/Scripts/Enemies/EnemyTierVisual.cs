using UnityEngine;
using TMPro;

// provides visual feedback for enemy tiers (right now is text only)
public class EnemyTierVisual : MonoBehaviour
{
    #region Variables

    [Tooltip("Text element to display tier (optional)")]
    [SerializeField] private TextMeshProUGUI tierText;
    
    [Tooltip("Show tier number as text")]
    [SerializeField] private bool showTierText = true;
    
    [Tooltip("Tier text format")]
    [SerializeField] private string tierTextFormat = "T{0}";

    public Sprite baseSprite;
    public Sprite tier1Sprite;
    public Sprite tier2Sprite;
    public Sprite tier3Sprite;

    private EnemyStatScaler scaler;
    private int currentTier = 1;

    #endregion

    [Header("DEBUG")]
    public bool debug = false;

    private void Awake()
    {
        scaler = GetComponent<EnemyStatScaler>();
    }

    private void Start()
    {
        // get tier and apply visual
        if (TierManager.Instance != null)
        {
            currentTier = TierManager.Instance.CurrentTier;
            ApplyTierVisual(currentTier);
        }
        
        // subscribe to tier changes
        if (TierManager.Instance != null)
            // Subscribe to centralized Event Bus
            GameEvents.OnTierChanged += OnTierChanged;
    }

    private void OnDestroy()
    {
        if (TierManager.Instance != null)
            // Unsubscribe from centralized Event Bus
            GameEvents.OnTierChanged -= OnTierChanged;
    }

    private void OnTierChanged(int newTier)
    {
        currentTier = newTier;
    }

    // apply visual changes based on tier
    public void ApplyTierVisual(int tier)
    {
        currentTier = tier;
        
        // get tier color from TierManager (implement later)
        Color tierColor = Color.white;
        Sprite tierSprite = baseSprite; // change sprite based on tier

        if (TierManager.Instance != null)
        {
            tierColor = TierManager.Instance.GetTierColor(tier);
            tierSprite = TierManager.Instance.GetTierSprite(tier); // get sprite per tier,
                                                                   // !! change this since sprites are now assigned per enemy type
        }

        // update tier text
        if (tierText != null && showTierText)
        {
            tierText.text = string.Format(tierTextFormat, tier);
            tierText.color = tierColor;
            baseSprite = tierSprite; // apply changed sprite
        }
    }

    // force update the visual (for after spawning)
    public void RefreshVisual()
    {
        if (scaler != null)
        {
            currentTier = scaler.GetCurrentTier();
        }
        else if (TierManager.Instance != null)
        {
            currentTier = TierManager.Instance.CurrentTier;
        }
        
        ApplyTierVisual(currentTier);
    }
}

