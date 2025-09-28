using UnityEngine;
using System;
using TMPro;

public class HealthSystem : MonoBehaviour
{
    [SerializeField] private int currentHealth;
    [SerializeField] private int maxHealth = 100;
    public int MaxHealth => maxHealth;
    [SerializeField] private int damagePerHit = 10;

    public int CurrentHealth => currentHealth;
    public int DamagePerHit => damagePerHit;

    public event Action<HealthSystem> OnDeath; // notify when this entity dies
    public static event Action<GameObject> OnAnyDeath; // global death flag for any death
    public event Action<int> OnHealthChanged; // notify UI elements

    [Header("UI")]
    //[SerializeField] private GameObject healthBarPrefab; 
    //[SerializeField] private Transform uiParentCanvas; 
    [SerializeField] private TextMeshProUGUI healthText;

    private HealthBar_UI healthBarUI;

    private void Awake()
    {
        currentHealth = maxHealth; // set health
    }

    void Update()
    {
        healthText.text = "HP: " + currentHealth + " / " + maxHealth;
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        Debug.Log(name + " takes " + amount + " damage. HP: " + currentHealth);
        OnHealthChanged?.Invoke(currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log(name + " died!");
        GameEvents.EntityDied(this);
        OnAnyDeath?.Invoke(gameObject); 
        Destroy(gameObject); // Do death animation eventually
    }
}
