using TMPro;
using UnityEngine;

public class Currency_UI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI creditsText;

    private void Start()
    {
        GameEvents.OnCreditsChanged += UpdateUI;
        
        // initialize UI with current credits
        if (CurrencyManager.Instance != null)
        {
            UpdateUI(CurrencyManager.Instance.Credits);
        }
    }

    private void OnDestroy()
    {
        GameEvents.OnCreditsChanged -= UpdateUI;
    }

    private void UpdateUI(int credits)
    {
        if (creditsText != null)
            creditsText.text = $"{credits}";
    }
}
