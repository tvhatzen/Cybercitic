using UnityEngine;

public class CurrencyPickUp : MonoBehaviour
{
    [Header("Currency Settings")]
    [SerializeField] private int creditsValue = 1;
    
    [Header("Magnet Settings")]
    [SerializeField] private float magnetRange = 3f; // Distance to start magnetizing
    [SerializeField] private float magnetSpeed = 8f; // How fast it moves toward player
    [SerializeField] private float pickupRange = 0.5f; // Distance to auto-pickup
    [SerializeField] private bool useMagnet = true; // Toggle magnet effect

    private Transform playerTransform;
    private bool isMagnetized = false;
    private Rigidbody rb;

    public bool debug = false;

    private void Start()
    {
        // find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        // get or add Rigidbody for smooth movement
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        
        // configure Rigidbody for smooth magnet movement
        rb.useGravity = false;
        rb.linearDamping = 5f; 
        rb.angularDamping = 5f;
    }

    private void Update()
    {
        if (playerTransform == null || !useMagnet) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // check if player is within pickup range
        if (distanceToPlayer <= pickupRange)
        {
            CollectCurrency();
            return;
        }

        // check if player is within magnet range
        if (distanceToPlayer <= magnetRange)
        {
            if (!isMagnetized)
            {
                isMagnetized = true;
                if(debug) Debug.Log($"[CurrencyPickUp] {name} magnetized to player!");
            }

            // move toward player
            Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
            Vector3 magnetForce = directionToPlayer * magnetSpeed;
            
            // apply force or direct movement
            if (rb != null)
            {
                rb.AddForce(magnetForce, ForceMode.Force);
            }
            else
            {
                transform.position += magnetForce * Time.deltaTime;
            }
        }
        else
        {
            if (isMagnetized)
            {
                isMagnetized = false;
                if(debug) Debug.Log($"[CurrencyPickUp] {name} no longer magnetized");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && CurrencyManager.Instance != null)
        {
            CollectCurrency();
            
        }
    }

    private void CollectCurrency()
    {
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.AddCredits(creditsValue);
            if(debug) Debug.Log($"[CurrencyPickUp] Collected {creditsValue} credits!");
        }

        AudioManager.Instance.PlaySound("pickUpCurrency");
        Debug.Log("collected currency and played sound");
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, magnetRange);
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }

    public void SetMagnetRange(float range) => magnetRange = range;
    public void SetMagnetSpeed(float speed) => magnetSpeed = speed;
    public void SetCreditsValue(int value) => creditsValue = value;
    public void EnableMagnet(bool enable) => useMagnet = enable;
}
