using UnityEngine;
using System;
using TMPro;
using System.Collections;

public class HealthSystem : MonoBehaviour
{
    private int currentHealth;
    //[SerializeField] private float maxHealth = 100;
    public int CurrentHealth => currentHealth;
    public int DamagePerHit => entityData != null ? entityData.currentAttack : 0;

    public event Action<HealthSystem> OnDeath; // notify when this entity dies
    public static event Action<GameObject> OnAnyDeath; // global death flag for any death
    public event Action<int> OnHealthChanged; // notify UI elements

    [Header("Health Bar UI")]
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private HealthBar healthBar;

    [Header("Damage Flash Settings")]
    [SerializeField] private SpriteRenderer spriteRenderer;  // make multiple for player sprites
    [SerializeField] private Color flashColor = Color.red;
    [SerializeField] private float flashDuration = 0.1f;

    private EntityData entityData;

    private Color originalColor;

    public bool debug = false;

    private void Awake()
    {
        entityData = GetComponent<EntityData>();
        
        if (entityData == null)
        {
            if(debug) Debug.LogError($"{name} is missing EntityData component!");
            return;
        }

        // store original sprite color for flash effect
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    private void Start()
    {
        if (entityData != null)
        {
            currentHealth = entityData.currentHealth; // set health
            UpdateHealthText();
            healthBar.UpdateHealthBar(entityData.baseHealth, currentHealth);
            
            if(debug) Debug.Log($"[HealthSystem] {name} initialized - HP: {currentHealth}/{entityData.baseHealth}, ATK: {DamagePerHit}");
        }
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if(debug) Debug.Log(name + " takes " + amount + " damage. HP: " + currentHealth);

        OnHealthChanged?.Invoke(currentHealth);
        UpdateHealthText();
        healthBar.UpdateHealthBar(entityData.baseHealth, currentHealth);

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
        if (healthText != null && entityData != null)
        {
            healthText.text = $"HP: {currentHealth} / {entityData.baseHealth}";
        }
    }

    public void SetHealth(int health)
    {
        currentHealth = health;

        // Update health UI
        OnHealthChanged?.Invoke(health);
        UpdateHealthText();
        healthBar.UpdateHealthBar(entityData.baseHealth, currentHealth);
    }

    public void ResetHealth()
    {
        if (entityData != null)
        {
            currentHealth = entityData.baseHealth;
            OnHealthChanged?.Invoke(currentHealth);
            UpdateHealthText();
            healthBar.UpdateHealthBar(entityData.baseHealth, currentHealth);
        }
    }

    public void ResetHealthToBase()
    {
        if (entityData != null)
        {
            currentHealth = entityData.baseHealth;
            OnHealthChanged?.Invoke(currentHealth);
            UpdateHealthText();
            healthBar.UpdateHealthBar(entityData.baseHealth, currentHealth);
        }
    }

    // Reset all stats to original values (for new game after win)
    public void ResetToOriginalStats()
    {
        if (entityData != null)
        {
            entityData.ResetToOriginalStats();
            currentHealth = entityData.currentHealth;
            OnHealthChanged?.Invoke(currentHealth);
            UpdateHealthText();

            // also reset credits if player
            if (CompareTag("Player"))
            {
                if (CurrencyManager.Instance != null)
                {
                    CurrencyManager.Instance.ResetCredits();
                }
            }
            
            if(debug) Debug.Log($"[HealthSystem] {name} reset to original stats - HP: {currentHealth}, ATK: {DamagePerHit}, Credits: {CurrencyManager.Instance.Credits}");
        }
    }

    private void Die()
    {
        if(debug) Debug.Log(name + " died!");

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