using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;

    [Header("Combat")]
    public LayerMask enemyLayer;
    public float combatCheckRadius = 2f;

    private CharacterController controller;

    // list of enemies in range 
    private readonly List<Transform> enemiesInRange = new List<Transform>();
    private List<GameObject> currentEnemies = new List<GameObject>();
    private Transform currentEnemy;
    private bool inCombat;

    void Awake() => controller = GetComponent<CharacterController>();
    void OnEnable()
    {
        HealthSystem.OnAnyDeath += OnAnyEntityDeath;
        FloorManager.OnEnemySpawned += RegisterEnemy;
    }
    void OnDisable()
    {
        HealthSystem.OnAnyDeath -= OnAnyEntityDeath;
        FloorManager.OnEnemySpawned -= RegisterEnemy;
    }

    private void RegisterEnemy(GameObject enemy)
    {
        currentEnemies.Add(enemy);
        Debug.Log("Player now knows about enemy: " + enemy.name);
    }

    private void UnregisterEnemy(GameObject deadObject)
    {
        if (currentEnemies.Contains(deadObject))
        {
            currentEnemies.Remove(deadObject);
            Debug.Log("Enemy removed from list: " + deadObject.name);
        }
    }

    void Update()
    {
        if (!inCombat)
        {
            // left to right auto move
            transform.position += Vector3.right * moveSpeed * Time.deltaTime;
        }

        UpdateCombatState();
    }

    private void UpdateCombatState()
    {
        ScanEnemiesInRange();
        Transform closestEnemy = GetClosestAliveEnemy();

        if (closestEnemy != null)
        {
            EnterOrContinueCombat(closestEnemy);
        }
        else
        {
            ExitCombatIfNoEnemies();
        }
    }

    /// <summary>
    /// Populate enemiesInRange list with alive enemies inside radius.
    /// </summary>
    private void ScanEnemiesInRange()
    {
        enemiesInRange.Clear();

        Collider[] hits = Physics.OverlapSphere(transform.position, combatCheckRadius, enemyLayer);
        // Debug.Log($"Found {hits.Length} colliders in combat radius");

        foreach (var hit in hits)
        {
            var enemyTransform = hit.transform.root;
            var health = enemyTransform.GetComponent<HealthSystem>();
            if (health != null && health.CurrentHealth > 0)
            {
                enemiesInRange.Add(enemyTransform);
            }
        }
    }

    /// <summary>
    /// Pick the closest alive enemy in range.
    /// </summary>
    private Transform GetClosestAliveEnemy()
    {
        if (enemiesInRange.Count == 0) return null;

        Transform closest = null;
        float closestDist = Mathf.Infinity;
        Vector3 playerPos = transform.position;

        foreach (var e in enemiesInRange)
        {
            if (e == null) continue;
            float dist = Vector3.Distance(playerPos, e.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = e;
            }
        }
        return closest;
    }

    /// <summary>
    /// Enter combat with a new enemy or continue with the same one.
    /// </summary>
    private void EnterOrContinueCombat(Transform enemy)
    {
        if (!inCombat || currentEnemy != enemy)
        {
            inCombat = true;
            currentEnemy = enemy;
            Debug.Log($"Entering combat with: {currentEnemy.name}");
            GameEvents.PlayerEnteredCombat(currentEnemy);
        }
    }

    /// <summary>
    /// Exit combat when no valid enemies are left.
    /// </summary>
    private void ExitCombatIfNoEnemies()
    {
        if (inCombat)
        {
            Debug.Log("No enemies left, exiting combat");
            inCombat = false;
            currentEnemy = null;
            GameEvents.PlayerExitedCombat();
        }
    }

    /// <summary>
    /// React when any entity dies; forces a re-check next frame.
    /// </summary>
    private void OnAnyEntityDeath(GameObject deadObject)
    {
        if (currentEnemies.Contains(deadObject))
        {
            currentEnemies.Remove(deadObject);
        }

        if (currentEnemy != null && deadObject.transform == currentEnemy)
        {
            Debug.Log($"Current enemy {deadObject.name} died, switching target...");
            currentEnemy = null;
            inCombat = false; 
        }
    }

    // debug
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, combatCheckRadius);
    }
}
