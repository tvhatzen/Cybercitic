using UnityEngine;
using System;
using TMPro;
using System.Collections;
using Random = UnityEngine.Random;

public class HealthSystem : MonoBehaviour
{
    private int currentHealth;
    public int CurrentHealth => currentHealth;
    public int DamagePerHit => entityData != null ? entityData.currentAttack : 0;

    public event Action<HealthSystem> OnDeath; // notify when this entity dies
    public static event Action<GameObject> OnAnyDeath; // global death flag for any death
    public event Action<int> OnHealthChanged; // notify UI elements

    [Header("Health Bar UI")]
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private HealthBar healthBar;

    [Header("Damage Flash")]
    [SerializeField] private SpriteRenderer spriteRenderer;  // make multiple for player sprites
    [SerializeField] private Color flashColor = Color.red;
    [SerializeField] private float flashDuration = 0.1f;

    [Header("Damage Sprite Shake")]
    private Vector3 originalPosition;
    private bool isShaking = false;
    public float shakeDuration = 0.5f;
    public float shakeMagnitude = 0.1f;

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

        //originalPosition = gameObject.transform.position;
    }

    public void TakeDamage(int amount)
    {
        // Check for dodge chance first (complete damage avoidance)
        if (entityData != null && entityData.currentDodgeChance > 0)
        {
            float dodgeRoll = Random.Range(0f, 1f);
            if (dodgeRoll < entityData.currentDodgeChance)
            {
                if(debug) Debug.Log($"{name} dodged the attack! (Roll: {dodgeRoll:F2} < Dodge: {entityData.currentDodgeChance:F2})");
                
                // Play dodge sound/effect here if desired
                GameEvents.RequestSound("dodge"); // Assuming you have a dodge sound
                
                return; // No damage taken
            }
        }
        
        // Apply defense damage reduction
        int finalDamage = amount;
        if (entityData != null && entityData.currentDefense > 0)
        {
            float damageReduction = entityData.currentDefense;
            finalDamage = Mathf.RoundToInt(amount * (1f - damageReduction));
            finalDamage = Mathf.Max(1, finalDamage); // Ensure at least 1 damage is dealt
            
            if(debug) Debug.Log($"{name} defense reduces damage from {amount} to {finalDamage} (Defense: {damageReduction:F2})");
        }
        
        currentHealth -= finalDamage;
        if(debug) Debug.Log(name + " takes " + finalDamage + " damage. HP: " + currentHealth);

        // store position
        originalPosition = gameObject.transform.position;

        // play damage sound
        if (AudioManager.Instance != null)
        {
            if (CompareTag("Player"))
            {
                AudioManager.Instance.PlaySound("damaged");
                if (debug) Debug.Log("[HealthSystem] Playing player damage sound");
            }
            else
            {
                AudioManager.Instance.PlaySound("enemydamaged");
                if (debug) Debug.Log("[HealthSystem] Playing enemy damage sound");
            }
        }

        // trigger both local and centralized events
        OnHealthChanged?.Invoke(currentHealth);
        GameEvents.HealthChanged(currentHealth);
        UpdateHealthText();
        healthBar.UpdateHealthBar(entityData.baseHealth, currentHealth);

        if (spriteRenderer != null)
        {
            StartCoroutine(FlashSprite()); // flash red
        }

        if (!isShaking)
        {
            StartCoroutine(Shake()); // shake
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
            healthText.text = $"{currentHealth} / {entityData.baseHealth}";
        }
    }

    public void SetHealth(int health)
    {
        currentHealth = health;

        // update health UI
        OnHealthChanged?.Invoke(health);
        GameEvents.HealthChanged(health);

        UpdateHealthText();
        healthBar.UpdateHealthBar(entityData.baseHealth, currentHealth);
    }

    public void ResetHealth()
    {
        if (entityData != null)
        {
            currentHealth = entityData.baseHealth;

            OnHealthChanged?.Invoke(currentHealth);
            GameEvents.HealthChanged(currentHealth);

            UpdateHealthText();
            healthBar.UpdateHealthBar(entityData.baseHealth, currentHealth);
            
            ResetSpriteColor();
        }
    }

    public void ResetHealthToBase()
    {
        if (entityData != null)
        {
            currentHealth = entityData.baseHealth;
            
            OnHealthChanged?.Invoke(currentHealth);
            GameEvents.HealthChanged(currentHealth);

            UpdateHealthText();
            healthBar.UpdateHealthBar(entityData.baseHealth, currentHealth);
            
            ResetSpriteColor();
        }
    }

    // reset all stats to original values (for new game after win)
    public void ResetToOriginalStats()
    {
        if (entityData != null)
        {
            entityData.ResetToOriginalStats();
            currentHealth = entityData.currentHealth;
            
            OnHealthChanged?.Invoke(currentHealth);
            GameEvents.HealthChanged(currentHealth);

            UpdateHealthText();

            // also reset credits if player
            if (CompareTag("Player"))
            {
                if (CurrencyManager.Instance != null)
                {
                    CurrencyManager.Instance.ResetCredits();
                }
            }
            
            ResetSpriteColor();
            
            if(debug) Debug.Log($"[HealthSystem] {name} reset to original stats - HP: {currentHealth}, ATK: {DamagePerHit}, Credits: {CurrencyManager.Instance.Credits}");
        }
    }

    // Reset sprite color back to original (removes damage flash effect)
    public void ResetSpriteColor()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
            if (debug) Debug.Log($"[HealthSystem] {name} sprite color reset to original");
        }
    }

    // shake sprite at random distance when damaged
    IEnumerator Shake()
    {
        isShaking = true;
        float elapsed = 0f;

        while(elapsed < shakeDuration)
        {
            // get random offset
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;

            // move sprite
            transform.position = originalPosition + new Vector3(x,y,0f);

            // increment elapsed time
            elapsed += Time.deltaTime;

            // wait for next frame
            yield return null;
        }

        // Return to original position
        transform.position = originalPosition;
        isShaking = false;
    }

    private void Die()
    {
        if(debug) Debug.Log(name + " died!");

        // play death sound
        if (AudioManager.Instance != null)
        {
            if (CompareTag("Player"))
            {
                AudioManager.Instance.PlaySound("die");
                if (debug) Debug.Log("[HealthSystem] Playing player death sound");
            }
            else
            {
                AudioManager.Instance.PlaySound("enemydie");
                if (debug) Debug.Log("[HealthSystem] Playing enemy death sound");
            }
        }

        // trigger both local and centralized events
        OnDeath?.Invoke(this);
        OnAnyDeath?.Invoke(gameObject);
        GameEvents.EntityDeath(this);
        GameEvents.EntityDied(this);

        // if its the player, show Results
        if (CompareTag("Player"))
        {
            // disable player prefab and trigger results
            gameObject.SetActive(false);
            GameState.Instance.OnPlayerDeath();
        }
        else if (CompareTag("Boss") && FloorManager.Instance.IsFinalFloor()) // check if is final boss floor 
        {
            GameState.Instance.OnBossDeath();
        }
        else
        {
            Destroy(gameObject);
        }
    }
}