using UnityEngine;
using TMPro;

// provides visual feedback for enemy tiers (right now is text only)
public class EnemyTierVisual : MonoBehaviour
{   
    [Tooltip("Text element to display tier (optional)")]
    [SerializeField] private TextMeshProUGUI tierText;
    
    [Tooltip("Show tier number as text")]
    [SerializeField] private bool showTierText = true;
    
    [Tooltip("Tier text format")]
    [SerializeField] private string tierTextFormat = "T{0}";


    private EnemyStatScaler scaler;
    private int currentTier = 1;

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
        if (TierManager.Instance != null)
        {
            tierColor = TierManager.Instance.GetTierColor(tier);
        }

        // update tier text
        if (tierText != null && showTierText)
        {
            tierText.text = string.Format(tierTextFormat, tier);
            tierText.color = tierColor;
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

