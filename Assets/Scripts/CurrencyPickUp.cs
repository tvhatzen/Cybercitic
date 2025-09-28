using UnityEngine;

public class CurrencyPickUp : MonoBehaviour
{
    [SerializeField] private int creditsValue = 1;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.AddCredits(creditsValue);
            Destroy(gameObject);
        }
    }
}
