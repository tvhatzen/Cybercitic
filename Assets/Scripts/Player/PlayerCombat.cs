using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerCombat : MonoBehaviour
{
    [Header("Combat")]
    public LayerMask enemyLayer;
    public float combatCheckRadius = 2f;

    [Tooltip("How long to keep moving to gather nearby enemies before stopping combat")]
    public float gatherWindowDuration = 0.5f;

    public bool InCombat { get; private set; }
    public Transform CurrentTarget { get; private set; }

    private PlayerMovement movement;
    private readonly List<Transform> enemiesInRange = new List<Transform>();
    private bool gatheringEnemies;
    private Coroutine gatherRoutine;

    void Awake()
    {
        movement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        // Keep scanning every frame
        ScanEnemiesInRange();

        if (enemiesInRange.Count > 0)
            EnterOrContinueCombat();
        else
            ExitCombatIfNoEnemies();

        // while in combat, always keep the closest alive enemy as the attack target
        if (InCombat && enemiesInRange.Count > 0)
            CurrentTarget = FindClosestEnemy();
    }

    private void ScanEnemiesInRange()
    {
        enemiesInRange.Clear();

        Collider[] hits = Physics.OverlapSphere(transform.position, combatCheckRadius, enemyLayer);
        // collect root transfroms that have a HealthSystem and are alive
        foreach (var hit in hits)
        {
            var enemyTransform = hit.transform.root;
            var health = enemyTransform.GetComponent<HealthSystem>();
            if (health != null && health.CurrentHealth > 0)
            {
                if (!enemiesInRange.Contains(enemyTransform))
                    enemiesInRange.Add(enemyTransform);
            }
        }
    }

    private void EnterOrContinueCombat()
    {
        // if not already gathering or in combat, start gather window
        if (!InCombat && !gatheringEnemies)
        {
            // during gather player keep moving
            movement.CanMove = true;
            gatherRoutine = StartCoroutine(GatherEnemiesThenEnterCombat());
        }
    }

    private IEnumerator GatherEnemiesThenEnterCombat()
    {
        gatheringEnemies = true;
        float timer = 0f;

        // gather window: keep updating enemies while still moving
        while (timer < gatherWindowDuration)
        {
            timer += Time.deltaTime;
            ScanEnemiesInRange();
            yield return null;
        }

        gatheringEnemies = false;

        if (enemiesInRange.Count > 0)
        {
            // lock into combat
            InCombat = true;

            // stop movement when combat begins
            movement.CanMove = false;

            // pick initial closest target
            CurrentTarget = FindClosestEnemy();

            // pass the full array to GameEvents
            GameEvents.PlayerEnteredCombat(enemiesInRange.ToArray());

            Debug.Log($"Entering combat with {enemiesInRange.Count} enemies. Target: {CurrentTarget?.name ?? "none"}");
        }
    }

    private void ExitCombatIfNoEnemies()
    {
        // only exit if player is in combat and not currently gathering enemies
        if (InCombat && !gatheringEnemies && enemiesInRange.Count == 0)
        {
            InCombat = false;
            CurrentTarget = null;

            // allow movement again
            movement.CanMove = true;

            if (gatherRoutine != null)
            {
                StopCoroutine(gatherRoutine);
                gatherRoutine = null;
            }

            GameEvents.PlayerExitedCombat();
            Debug.Log("No enemies left, exiting combat");
        }
    }

    private Transform FindClosestEnemy()
    {
        Transform closest = null;
        float minDist = float.MaxValue;
        Vector3 pos = transform.position;

        foreach (var e in enemiesInRange)
        {
            if (e == null) continue;
            var hs = e.GetComponent<HealthSystem>();
            if (hs == null || hs.CurrentHealth <= 0) continue;

            float d = Vector3.Distance(pos, e.position);
            if (d < minDist)
            {
                minDist = d;
                closest = e;
            }
        }

        return closest;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, combatCheckRadius);
    }
}
