using System.Collections;
using UnityEngine;

[RequireComponent(typeof(HealthSystem))]
public class EnemyCombat : MonoBehaviour
{
    [Header("Enemy Combat Settings")]
    public float attackCooldown = 2f;
    public float attackRange = 2f; // how close enemy needs to be to attack
    private int damage;

    [Header("Combat Target")]
    protected  Transform player;
    private float nextAttackTime;
    protected  HealthSystem health;

    [Header("Attack Anim")]
    private Vector3 originalPosition;
    public float animDuration = 0.1f;
    public float animMagnitude = 5;
    public bool isAttacking = false;

    private EntityData entityData;

    // only attack if player is in combat
    private bool canAttack = false;
    private bool forceEnabled = false; // Track if combat was force-enabled 

    [Header("DEBUG")]
    public bool debug = false;

    public virtual void Awake()
    {
        // initialize HealthSystem
        health = GetComponent<HealthSystem>();

        // initialize EntityData
        entityData = GetComponent<EntityData>();

        // try to find player
        FindPlayer();

        // set damage from EntityData
        if (entityData != null)
        {
            damage = entityData.currentAttack; 
        }

        GameEvents.OnPlayerEnterCombat += EnableCombat;
        GameEvents.OnPlayerExitCombat += DisableCombat;
    }

    private void Start()
    {
        // Ensure player is found after all objects are initialized
        if (player == null)
        {
            FindPlayer();
        }
    }

    private void FindPlayer()
    {
        // Try to find the player
        if (PlayerInstance.Instance != null)
        {
            player = PlayerInstance.Instance.transform;
            if (debug) Debug.Log($"[EnemyCombat] {name} found player via PlayerInstance");
        }
        else
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                if (debug) Debug.Log($"[EnemyCombat] {name} found player via FindGameObjectWithTag");
            }
            else if (debug)
            {
                Debug.LogWarning($"[EnemyCombat] {name} could not find player!");
            }
        }
    }

    protected virtual void OnDestroy()
    {
        // Unsubscribe to combat event
        GameEvents.OnPlayerEnterCombat -= EnableCombat;
        GameEvents.OnPlayerExitCombat -= DisableCombat;
    }

    public virtual void Update()
    {
        if (!canAttack)
        {
            if (debug) Debug.LogWarning($"[EnemyCombat] {name} cannot attack - canAttack is false");
            return; // only attack if combat is active
        }
        if (health == null || health.CurrentHealth <= 0) return;
        
        // Try to find player if null
        if (player == null)
        {
            FindPlayer();
            if (player == null)
            {
                if (debug) Debug.LogWarning($"[EnemyCombat] {name} cannot attack - player is null");
                return; // still can't find player, skip this frame
            }
        }

        var playerHealth = player.GetComponent<HealthSystem>();
        if (playerHealth != null && playerHealth.CurrentHealth > 0)
        {
            // check if player is in attack range
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer <= attackRange)
            {
                TryAttack(playerHealth);
            }
            else if (debug)
            {
                Debug.Log($"[EnemyCombat] {name} player out of range: {distanceToPlayer:F2} > {attackRange}");
            }
        }
    }

    protected virtual void TryAttack(HealthSystem target)
    {
        if (Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;
            AttackTarget(target);
        }
        else if (debug)
        {
            float timeUntilAttack = nextAttackTime - Time.time;
            Debug.Log($"[EnemyCombat] {name} waiting to attack, {timeUntilAttack:F1}s remaining");
        }
    }

    protected virtual void AttackTarget(HealthSystem target)
    {
        // store position
        originalPosition = gameObject.transform.position;

        if (target == null) return;

        target.TakeDamage(damage);
        if(debug) Debug.Log($"[EnemyCombat] {name} attacked {target.name} for {damage} damage!");
    }

    private void EnableCombat(Transform[] enemiesInCombat)
    {
        // only enable if this enemy is in combat
        foreach (var t in enemiesInCombat)
        {
            if (t == transform)
            {
                canAttack = true;
                // If combat was force-enabled, keep that flag (don't reset it)
                nextAttackTime = Time.time + attackCooldown;
                if (debug) Debug.Log($"[EnemyCombat] {name} combat enabled, next attack in {attackCooldown}s");
                break;
            }
        }
    }
    
    private void DisableCombat()
    {
        // Don't disable if combat was force-enabled 
        if (forceEnabled)
        {
            if (debug) Debug.Log($"[EnemyCombat] {name} combat disable ignored - force enabled");
            return;
        }
        canAttack = false;
        if (debug) Debug.Log($"[EnemyCombat] {name} combat disabled");
    }

    // Public method to force enable combat (used after respawn)
    public void ForceEnableCombat()
    {
        // Ensure player reference is found before enabling combat
        if (player == null)
        {
            FindPlayer();
        }
        
        forceEnabled = true; // Mark as force-enabled so DisableCombat won't turn it off
        canAttack = true;
        // Set next attack time to current time + cooldown to prevent immediate attack
        nextAttackTime = Time.time + attackCooldown;
        if (debug) Debug.Log($"[EnemyCombat] {name} combat force enabled, next attack in {attackCooldown}s, player found: {player != null}");
    }

    // Draw attack range
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}