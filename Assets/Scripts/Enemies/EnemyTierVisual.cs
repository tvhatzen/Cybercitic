using UnityEngine;
using TMPro;

// provides visual feedback for enemy tiers 
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
    private SpriteRenderer spriteRenderer;
    private int currentTier = 1;
    private int floorsPerTier = 5; // Tier increases every 5 floors (after boss floors)

    #endregion

    [Header("DEBUG")]
    public bool debug = false;

    private void Awake()
    {
        scaler = GetComponent<EnemyStatScaler>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
    }

    private void Start()
    {
        // Calculate tier based on current floor and apply visual
        CalculateTierFromFloor();
        ApplyTierVisual(currentTier);
        
        // Subscribe to floor changes to update tier when floor progresses
        GameEvents.OnFloorChanged += OnFloorChanged;
    }

    private void OnDestroy()
    {
        GameEvents.OnFloorChanged -= OnFloorChanged;
    }

    private void OnFloorChanged(int newFloor)
    {
        // Recalculate tier when floor changes
        CalculateTierFromFloor();
        ApplyTierVisual(currentTier);
    }

    // Calculate tier based on current floor (every 5 floors = new tier)
    private void CalculateTierFromFloor()
    {
        if (FloorManager.Instance != null)
        {
            int currentFloor = FloorManager.Instance.CurrentFloor;
            // Tier formula: tier = ceil(floor / 5)
            // Floors 1-5 = Tier 1, Floors 6-10 = Tier 2, Floors 11-15 = Tier 3
            currentTier = Mathf.CeilToInt(currentFloor / (float)floorsPerTier);
            
            // Ensure minimum tier of 1
            if (currentTier < 1) currentTier = 1;
            
            if (debug)
            {
                Debug.Log($"[EnemyTierVisual] Floor {currentFloor} = Tier {currentTier}");
            }
        }
        else
        {
            // Fallback to TierManager if FloorManager not available
            if (TierManager.Instance != null)
            {
                currentTier = TierManager.Instance.CurrentTier;
            }
        }
    }

    // apply visual changes based on tier
    public void ApplyTierVisual(int tier)
    {
        currentTier = tier;
        
        // Get tier color from TierManager
        Color tierColor = Color.white;
        if (TierManager.Instance != null)
        {
            tierColor = TierManager.Instance.GetTierColor(tier);
        }

        // Select sprite based on tier 
        Sprite tierSprite = baseSprite; // Default to base sprite
        
        switch (tier)
        {
            case 1:
                tierSprite = tier1Sprite != null ? tier1Sprite : baseSprite;
                break;
            case 2:
                tierSprite = tier2Sprite != null ? tier2Sprite : baseSprite;
                break;
            case 3:
                tierSprite = tier3Sprite != null ? tier3Sprite : baseSprite;
                break;
            default:
                // For tier 4+, use tier3Sprite or baseSprite
                tierSprite = tier3Sprite != null ? tier3Sprite : baseSprite;
                break;
        }

        // Apply sprite to SpriteRenderer
        if (spriteRenderer != null && tierSprite != null)
        {
            spriteRenderer.sprite = tierSprite;
            
            if (debug)
            {
                Debug.Log($"[EnemyTierVisual] Applied Tier {tier} sprite: {tierSprite.name}");
            }
        }

        // Update tier text
        if (tierText != null && showTierText)
        {
            tierText.text = string.Format(tierTextFormat, tier);
            tierText.color = tierColor;
        }
    }

    // force update the visual (for after spawning)
    public void RefreshVisual()
    {
        // Recalculate tier from floor 
        CalculateTierFromFloor();
        ApplyTierVisual(currentTier);
    }
}

