using UnityEngine;

public class EnemyLoot : MonoBehaviour
{
    [Header("Default Currency Drop")]
    [SerializeField] private GameObject currencyPrefab; 
    [SerializeField] private int creditsOnDeath = 10;
    
    [Header("Additional Loot Drops")]
    [SerializeField] private LootDrop[] possibleDrops;

    private HealthSystem health;

    private void Awake()
    {
        health = GetComponent<HealthSystem>();
        health.OnDeath += HandleDeath;
    }

    private void HandleDeath(HealthSystem hs)
    {
        if (CurrencyManager.Instance != null)
        {
            Debug.Log($"{gameObject.name} dropped {creditsOnDeath} credits!");
            CurrencyManager.Instance.AddCredits(creditsOnDeath);
        }

        // spawn world currency prefab
        if (currencyPrefab != null)
        {
            Instantiate(currencyPrefab, transform.position, Quaternion.identity);
        }

        // chance for extra drops
        foreach (var drop in possibleDrops)
        {
            if (Random.Range(0, 100) < drop.dropChance)
            {
                int amount = drop.GetAmount();
                for (int i = 0; i < amount; i++)
                {
                    Instantiate(drop.prefab, transform.position, Quaternion.identity);
                }
            }
        }
    }

    private void OnDestroy()
    {
        if (health != null)
            health.OnDeath -= HandleDeath;
    }
}
