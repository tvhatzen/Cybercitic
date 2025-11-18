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

    [Header("DEBUG")]
    public bool debug = false;

    public virtual void Awake()
    {
        // initialize HealthSystem
        health = GetComponent<HealthSystem>();

        // initialize EntityData
        entityData = GetComponent<EntityData>();

        // try to find player
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // set damage from EntityData
        if (entityData != null)
        {
            damage = entityData.currentAttack; 
        }

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
            // check if player is in attack range
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer <= attackRange)
            {
                TryAttack(playerHealth);
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

        // make simple attack anim (bounce sprite on y, like dmg shake)
        //StartCoroutine(AtkAnim());

        target.TakeDamage(damage);
        if(debug) Debug.Log($"[EnemyCombat] {name} attacked {target.name} for {damage} damage!");
    }

    IEnumerator AtkAnim()
    {
        isAttacking = true;
        float elapsed = 0f;

        while (elapsed < animDuration)
        {
            // move sprite
            transform.position = originalPosition + new Vector3(0f, 1f, 0f) * animMagnitude;

            // increment elapsed time
            elapsed += Time.deltaTime;

            // wait for next frame
            yield return null;
        }

        // Return to original position
        transform.position = originalPosition;
        isAttacking = false;
    }

    private void EnableCombat(Transform[] enemiesInCombat)
    {
        // only enable if this enemy is in combat
        foreach (var t in enemiesInCombat)
        {
            if (t == transform)
            {
                canAttack = true;
                // Set next attack time to current time + cooldown to prevent immediate attack
                nextAttackTime = Time.time + attackCooldown;
                if (debug) Debug.Log($"[EnemyCombat] {name} combat enabled, next attack in {attackCooldown}s");
                break;
            }
        }
    }
    
    private void DisableCombat()
    {
        canAttack = false;
    }

    // Public method to force enable combat (used after respawn)
    public void ForceEnableCombat()
    {
        canAttack = true;
        // Set next attack time to current time + cooldown to prevent immediate attack
        nextAttackTime = Time.time + attackCooldown;
        if (debug) Debug.Log($"[EnemyCombat] {name} combat force enabled, next attack in {attackCooldown}s");
    }

    // Draw attack range
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}