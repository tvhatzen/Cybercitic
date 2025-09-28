using UnityEngine;
using UnityEngine.UI;

public class HealthBar_UI : MonoBehaviour
{
    [Header("Health Bar Settings")]
    [SerializeField] private Transform uiParentCanvas; // canvas
    [SerializeField] private GameObject healthBarPrefab; // prefab 


    [Header("UI Components")]
    [SerializeField] private Image foregroundBar; 


    [Header("Settings")]
    public Vector3 offset = new Vector3(0, 2f, 0); // position above entity
    public bool followEntity = true;

    private HealthSystem healthSystem;
    private Transform target;

    void Start()
    {
        if (uiParentCanvas == null)
        {
            Debug.LogError("UI Parent Canvas is not assigned!");
            return;
        }

        if (healthBarPrefab == null)
        {
            Debug.LogError("Health Bar Prefab is not assigned!");
            return;
        }

        // attach to player automatically
        HealthSystem playerHS = GameObject.FindGameObjectWithTag("Player")?.GetComponent<HealthSystem>();
        if (playerHS != null)
            AttachToEntity(playerHS, healthBarPrefab, uiParentCanvas);

        // attach to all enemies automatically
        HealthSystem[] allHS = FindObjectsByType<HealthSystem>(FindObjectsSortMode.None);
        foreach (var hs in allHS)
        {
            if (hs != playerHS) // skip player
                AttachToEntity(hs, healthBarPrefab, uiParentCanvas);
        }
    }

    public static void AttachToEntity(HealthSystem hs, GameObject healthBarPrefab, Transform uiParent)
    {
        if (hs == null || healthBarPrefab == null || uiParent == null) return;

        GameObject hb = Instantiate(healthBarPrefab, uiParent);
        var hbScript = hb.GetComponent<HealthBar_UI>();
        hbScript?.Initialize(hs);
    }
    
    public void Initialize(HealthSystem hs)
    {
        healthSystem = hs;
        target = hs.transform;

        // subscribe to death event
        healthSystem.OnDeath += HandleDeath;

        // initialize the bar
        UpdateBar();
    }

    void Update()
    {
        if (healthSystem == null || target == null) return;

        if (followEntity)
        {
            // convert world position to screen position
            Vector3 screenPos = Camera.main.WorldToScreenPoint(target.position + offset);
            transform.position = screenPos;
        }

        // update bar value
        UpdateBar();
    }

    private void UpdateBar()
    {
        if (foregroundBar != null && healthSystem != null)
        {
            float ratio = (float)healthSystem.CurrentHealth / healthSystem.MaxHealth;
            foregroundBar.fillAmount = Mathf.Clamp01(ratio);
        }
    }

    private void HandleDeath(HealthSystem hs)
    {
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (healthSystem != null)
            healthSystem.OnDeath -= HandleDeath;
    }
}