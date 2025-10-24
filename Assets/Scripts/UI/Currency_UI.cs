using TMPro;
using UnityEngine;

public class Currency_UI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI creditsText;

    private void Start()
    {
        // Subscribe to centralized Event Bus
        GameEvents.OnCreditsChanged += UpdateUI;
        
        // Initialize UI with current credits
        if (CurrencyManager.Instance != null)
        {
            UpdateUI(CurrencyManager.Instance.Credits);
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from centralized Event Bus
        GameEvents.OnCreditsChanged -= UpdateUI;
    }

    private void UpdateUI(int credits)
    {
        if (creditsText != null)
            creditsText.text = $"Credits: {credits}";
    }
}
