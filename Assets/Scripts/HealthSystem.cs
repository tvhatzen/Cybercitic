using UnityEngine;
using System;
using TMPro;
using System.Collections;

public class HealthSystem : MonoBehaviour
{
    private int currentHealth;
    private int damagePerHit;

    public int CurrentHealth => currentHealth;
    public int DamagePerHit => damagePerHit;

    public event Action<HealthSystem> OnDeath; // notify when this entity dies
    public static event Action<GameObject> OnAnyDeath; // global death flag for any death
    public event Action<int> OnHealthChanged; // notify UI elements

    [Header("Health Bar UI")]
    [SerializeField] private TextMeshProUGUI healthText;


    [Header("Damage Flash Settings")]
    [SerializeField] private SpriteRenderer spriteRenderer;  // make multiple for player sprites
    [SerializeField] private Color flashColor = Color.red;
    [SerializeField] private float flashDuration = 0.1f;

    private EntityStats stats;

    private Color originalColor;

    private void Awake()
    {
        stats = GetComponent<EntityStats>();

        currentHealth = stats.Health; // set health
        damagePerHit = stats.attack; // set damage

        UpdateHealthText();
    }

    void Update()
    {
         if (healthText != null)
        {
            // make text face the camera
            healthText.transform.rotation = Quaternion.LookRotation(
                healthText.transform.position - Camera.main.transform.position
            );
        }
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        Debug.Log(name + " takes " + amount + " damage. HP: " + currentHealth);

        OnHealthChanged?.Invoke(currentHealth);
        UpdateHealthText();

        if (spriteRenderer != null)
        {
            StartCoroutine(FlashSprite());
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator FlashSprite() 
    {
        spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }

    private void UpdateHealthText()
    {
        if (healthText != null)
        {
            healthText.text = $"HP: {currentHealth} / {stats.baseHealth}";
        }
    }

    public void SetHealth(int health)
    {
        currentHealth = health;

        // Update health UI
        OnHealthChanged?.Invoke(health);
        UpdateHealthText();
    }

    public void ResetHealth() // !!! NOTE: make it so when the player progresses floors, set health to current. when respawning, set to max
    {
        currentHealth = stats.baseHealth;
        OnHealthChanged?.Invoke(currentHealth);
        UpdateHealthText();
    }

    private void Die()
    {
        Debug.Log(name + " died!");

        OnDeath?.Invoke(this);
        OnAnyDeath?.Invoke(gameObject);

        GameEvents.EntityDied(this);

        // if its the player, show Results
        if (CompareTag("Player"))
        {
            // disable player prefab and trigger results
            gameObject.SetActive(false);
            GameState.Instance.OnPlayerDeath();
        }
        else if (CompareTag("Boss"))
        {
            GameState.Instance.OnBossDeath();
        }
        else
        {
            Destroy(gameObject);
        }

        //Destroy(gameObject); // Do death animation eventually
    }
}
// make sure health carries over to other scenes, then on death reset it to max