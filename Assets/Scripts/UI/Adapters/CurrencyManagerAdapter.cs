namespace Cybercitic.UI
{
    /// <summary>
    /// Adapter to make CurrencyManager implement ICurrencyService.
    /// Enables dependency inversion while maintaining compatibility.
    /// </summary>
    public class CurrencyManagerAdapter : ICurrencyService
    {
        public int Credits => CurrencyManager.Instance != null ? CurrencyManager.Instance.Credits : 0;

        public bool CanAfford(int cost)
        {
            return CurrencyManager.Instance != null && CurrencyManager.Instance.CanAfford(cost);
        }

        public bool SpendCredits(int amount)
        {
            return CurrencyManager.Instance != null && CurrencyManager.Instance.SpendCredits(amount);
        }

        public void AddCredits(int amount)
        {
            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.AddCredits(amount);
            }
        }
    }
}

