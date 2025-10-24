using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatManager : SingletonBase<CombatManager>
{
    private HealthSystem playerHealth;

    // track all enemies Healthsystem components currently in combat
    private readonly List<HealthSystem> activeEnemies = new List<HealthSystem>();

    [SerializeField] private float playerAttackRate = 1.0f; // attacks per second
    [SerializeField] private float enemyAttackRate = 1.5f;

    private Coroutine combatRoutine;

    [Header("DEBUG")]
    public bool debug = false;

    void OnEnable()
    {
        GameEvents.OnPlayerEnterCombat += HandleEnterCombat;
        GameEvents.OnPlayerExitCombat += HandleExitCombat;
    }

    void OnDisable()
    {
        GameEvents.OnPlayerEnterCombat -= HandleEnterCombat;
        GameEvents.OnPlayerExitCombat -= HandleExitCombat;
    }

    void Start()
    {
        playerHealth = FindFirstObjectByType<PlayerController>().GetComponent<HealthSystem>();
    }

    public void HandleEnterCombat(Transform[] enemies)
    {        
        if(debug) Debug.Log("Player entered combat with " + enemies.Length + "enemies");

        activeEnemies.Clear();
        foreach (var enemy in enemies)
        {
            var hs = enemy.GetComponent<HealthSystem>();
            if (hs != null && hs.CurrentHealth > 0)
            {
                activeEnemies.Add(hs);

                // subscribe to each enemy’s death
                hs.OnDeath += OnAnyDeath;
            }
        }

        // subscribe to player death once
        playerHealth.OnDeath += OnAnyDeath;

        if (combatRoutine != null)
            StopCoroutine(combatRoutine);

        combatRoutine = StartCoroutine(CombatLoop());
    }

    void HandleExitCombat()
    {
        if(debug) Debug.Log("Player exited combat");

        if (combatRoutine != null)
        {
            StopCoroutine(combatRoutine);
            combatRoutine = null;
        }

        // unsubscribe from deaths
        playerHealth.OnDeath -= OnAnyDeath;
        foreach (var e in activeEnemies)
            e.OnDeath -= OnAnyDeath;

        activeEnemies.Clear();
    }

    void OnAnyDeath(HealthSystem dead)
    {
        // remove dead enemy
        if (activeEnemies.Contains(dead))
        {
            activeEnemies.Remove(dead);
        }

        // if player died or all enemies dead, exit combat
        if (dead == playerHealth || activeEnemies.Count == 0)
        {
            HandleExitCombat();
        }
    }

    private IEnumerator CombatLoop()
    {
        if(debug) Debug.Log("entered combat loop coroutine");

        float playerTimer = 0f;
        float enemyTimer = 0f;

        while (playerHealth.CurrentHealth > 0 && activeEnemies.Count > 0)
        {
            playerTimer += Time.deltaTime;
            enemyTimer += Time.deltaTime;

            // pick closest or first alive enemy
            var targetEnemy = GetNextEnemyTarget();

            if (playerTimer >= 1f / playerAttackRate)
            {
                playerTimer = 0f;
                Attack(playerHealth, targetEnemy);
            }

            yield return null;
        }
        HandleExitCombat();
    }

    private HealthSystem GetNextEnemyTarget()
    {
        // choose closest alive
        HealthSystem closest = null;
        float closestDist = Mathf.Infinity;
        var playerPos = playerHealth.transform.position;

        foreach (var e in activeEnemies)
        {
            if (e == null || e.CurrentHealth <= 0) continue;
            float dist = Vector3.Distance(playerPos, e.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = e;
            }
        }
        return closest;
    }

    private void Attack(HealthSystem attacker, HealthSystem target)
    {
        if (target == null) return;
        target.TakeDamage(attacker.DamagePerHit);
        if(debug) Debug.Log(attacker.name + " attacks " + target.name);
    }
}
