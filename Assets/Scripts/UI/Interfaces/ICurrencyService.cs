namespace Cybercitic.UI
{
    /// <summary>
    /// Interface for currency management services.
    /// Enables dependency inversion and testability.
    /// </summary>
    public interface ICurrencyService
    {
        int Credits { get; }
        bool CanAfford(int cost);
        bool SpendCredits(int amount);
        void AddCredits(int amount);
    }
}

