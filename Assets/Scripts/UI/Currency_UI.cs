using TMPro;
using UnityEngine;

public class Currency_UI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI creditsText;

    private void Start()
    {
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnCreditsChanged += UpdateUI;
            UpdateUI(CurrencyManager.Instance.Credits);
        }
    }

    private void OnDestroy()
    {
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnCreditsChanged -= UpdateUI;
    }

    private void UpdateUI(int credits)
    {
        if (creditsText != null)
            creditsText.text = $"Credits: {credits}";
    }
}
