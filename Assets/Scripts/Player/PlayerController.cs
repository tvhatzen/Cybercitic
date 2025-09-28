using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;

    [Header("Combat")]
    public LayerMask enemyLayer;
    public float combatCheckRadius = 2f;

    [Tooltip("How long to keep moving to gather nearby enemies before stopping combat")]
    public float gatherWindowDuration = 0.5f;
    
    private CharacterController controller;

    // list of enemies in range 
    private readonly List<Transform> enemiesInRange = new List<Transform>();
    private List<GameObject> currentEnemies = new List<GameObject>();
    private Transform currentEnemy;

    // combat checks
    private bool inCombat;
    private bool gatheringEnemies;
    private Coroutine gatherRoutine;

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
        // only move if not actually in combat, can still move while gathering enemies
        if (!inCombat || gatheringEnemies)
        {
            // left to right auto move
            transform.position += Vector3.right * moveSpeed * Time.deltaTime;
        }

        UpdateCombatState();
    }

    private void UpdateCombatState()
    {
        ScanEnemiesInRange();

        Transform[] enemiesArray = enemiesInRange.ToArray();

        if (enemiesArray.Length > 0)
        {
            EnterOrContinueCombat(enemiesArray);
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
    /// Enter combat with a new enemy or continue with the same one.
    /// </summary>
    private void EnterOrContinueCombat(Transform[] enemies)
    {
        if (!inCombat && !gatheringEnemies)
        {
            // Start gather window
            gatherRoutine = StartCoroutine(GatherEnemiesThenEnterCombat());
        }
    }

    private IEnumerator GatherEnemiesThenEnterCombat()
    {
        gatheringEnemies = true;
        float timer = 0f;

        while (timer < gatherWindowDuration)
        {
            timer += Time.deltaTime;
            ScanEnemiesInRange(); // keep updating who’s in range
            yield return null;
        }

        // after gather window
        gatheringEnemies = false;

        if (enemiesInRange.Count > 0)
        {
            inCombat = true;
            Debug.Log($"Entering combat with {enemiesInRange.Count} enemies");
            GameEvents.PlayerEnteredCombat(enemiesInRange.ToArray());
        }
    }

    /// <summary>
    /// Exit combat when no valid enemies are left.
    /// </summary>
    private void ExitCombatIfNoEnemies()
    {
        if (inCombat && !gatheringEnemies && enemiesInRange.Count == 0)
        {
            Debug.Log("No enemies left, exiting combat");
            inCombat = false;
            currentEnemy = null;
            if (gatherRoutine != null) StopCoroutine(gatherRoutine);
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
