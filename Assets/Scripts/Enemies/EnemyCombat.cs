using System.Collections;
using UnityEngine;

[RequireComponent(typeof(HealthSystem))]
public class EnemyCombat : MonoBehaviour
{
    [Header("Enemy Combat Settings")]
    public float attackCooldown = 2f;
    private int damage;

    [Header("Combat Target")]
    protected  Transform player;
    private float nextAttackTime;
    protected  HealthSystem health;

    private EntityStats stats;

    // Only attack if player is in combat
    private bool canAttack = false;

    public virtual void Awake()
    {
        // Initialize HealthSystem
        health = GetComponent<HealthSystem>();
        if (health == null)
            Debug.LogError($"{name} is missing a HealthSystem component!");

        // Try to find player
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        damage = stats.attack; // set damage amount

        // Subscribe to GameEvents
        GameEvents.OnPlayerEnterCombat += EnableCombat;
        GameEvents.OnPlayerExitCombat += DisableCombat;
    }

    protected virtual void OnDestroy()
    {
        // Unsubscribe to combat event
        GameEvents.OnPlayerEnterCombat -= EnableCombat;
        GameEvents.OnPlayerExitCombat -= DisableCombat;
    }

    public virtual void Update()
    {
        if (!canAttack) return; // only attack if combat is active
        if (health == null || health.CurrentHealth <= 0) return;
        if (player == null) return;

        var playerHealth = player.GetComponent<HealthSystem>();
        if (playerHealth != null && playerHealth.CurrentHealth > 0)
        {
            TryAttack(playerHealth);
        }
    }

    protected virtual void TryAttack(HealthSystem target)
    {
        if (Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;
            AttackTarget(target);
        }
    }

    protected virtual void AttackTarget(HealthSystem target)
    {
        if (target == null) return;

        target.TakeDamage(damage);
        Debug.Log($"{name} attacked {target.name} for {damage} damage!");
    }

    private void EnableCombat(Transform[] enemiesInCombat)
    {
        // only enable if this enemy is part of the combat
        foreach (var t in enemiesInCombat)
        {
            if (t == transform)
            {
                canAttack = true;
                break;
            }
        }
    }
    
    private void DisableCombat()
    {
        canAttack = false;
    }
}
// when adding Entity stats script, enemies sprites are not appearing / they are not being instantiated. problem here?