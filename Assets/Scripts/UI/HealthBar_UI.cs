using UnityEngine;
using UnityEngine.UI;

public class HealthBar_UI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image foregroundBar;


    [Header("Settings")]
    public Vector3 offset = new Vector3(0, 2f, 0); 
    public bool followEntity = true;

    [Header("World Space Canvas")]
    public Transform worldSpaceCanvas;

    private HealthSystem healthSystem;
    private Transform target;

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

        healthSystem.OnHealthChanged += UpdateBar;
        healthSystem.OnDeath += HandleDeath;

        UpdateBar(healthSystem.CurrentHealth);
    }

    void Update()
    {
        if (healthSystem == null || target == null) return;

        if (followEntity)
        {
            // world space positioning
            transform.position = target.position + offset;
        }

        UpdateBar(healthSystem.CurrentHealth);
    }

    private void UpdateBar(int currentHealth)
    {
        if (foregroundBar != null && healthSystem != null)
        {
            float ratio = (float)currentHealth / healthSystem.CurrentHealth; // ??
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
        {
            healthSystem.OnHealthChanged -= UpdateBar;
            healthSystem.OnDeath -= HandleDeath;
        }
    }
}