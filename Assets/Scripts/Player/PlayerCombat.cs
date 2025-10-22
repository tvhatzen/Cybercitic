using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerCombat : MonoBehaviour
{

    // * add combat dodge chance upgrade. start at 15% to not take dmg from enemy attack
    // * add defense upgrade. start with 10% reduced dmg from enemy.
    // * add speed upgrade. reduce atk cooldown by 15% each upgrade.

    [Header("Combat")]
    public LayerMask enemyLayer;
    public float combatCheckRadius = 2f;

    [Tooltip("How long to keep moving to gather nearby enemies before stopping combat")]
    public float gatherWindowDuration = 0.5f;

    public bool InCombat { get; private set; }
    public Transform CurrentTarget { get; private set; }

    private PlayerMovement movement;
    private FrameBasedPlayerAnimator frameAnimator;
    private readonly List<Transform> enemiesInRange = new List<Transform>();
    private bool gatheringEnemies;
    private Coroutine gatherRoutine;
    private Transform previousTarget; // Track previous target for event firing

    public bool debug = false;

    void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        frameAnimator = GetComponent<FrameBasedPlayerAnimator>();
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
        {
            Transform newTarget = FindClosestEnemy();
            
            // Fire event if target changed
            if (newTarget != CurrentTarget)
            {
                // Check if CurrentTarget is still valid before passing it to the event
                Transform oldTarget = null;
                if (CurrentTarget != null)
                {
                    // Additional check to ensure the Transform hasn't been destroyed
                    try
                    {
                        string name = CurrentTarget.name; // This will throw if destroyed
                        oldTarget = CurrentTarget;
                    }
                    catch (System.Exception)
                    {
                        // Transform was destroyed, set to null
                        oldTarget = null;
                        CurrentTarget = null;
                    }
                }
                
                GameEvents.PlayerTargetChanged(oldTarget, newTarget);
                previousTarget = CurrentTarget;
                CurrentTarget = newTarget;
            }
        }
    }

    private void ScanEnemiesInRange()
    {
        enemiesInRange.Clear();

        Collider[] hits = Physics.OverlapSphere(transform.position, combatCheckRadius, enemyLayer);
        
        // Debug info
        if (hits.Length > 0)
        {
            if(debug) Debug.Log($"[PlayerCombat] Found {hits.Length} colliders in range");
        }
        
        // collect root transfroms that have a HealthSystem and are alive
        foreach (var hit in hits)
        {
            var enemyTransform = hit.transform.root;
            var health = enemyTransform.GetComponent<HealthSystem>();
            if (health != null && health.CurrentHealth > 0)
            {
                if (!enemiesInRange.Contains(enemyTransform))
                {
                    enemiesInRange.Add(enemyTransform);
                    if(debug) Debug.Log($"[PlayerCombat] Added enemy to combat: {enemyTransform.name} (HP: {health.CurrentHealth})");
                }
            }
            else
            {
                if(debug) Debug.LogWarning($"[PlayerCombat] Hit object {hit.name} but no valid HealthSystem found");
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
            
            // Stop movement animation and prepare for combat
            if (frameAnimator != null)
            {
                frameAnimator.StopCurrentAnimation();
                if (debug) Debug.Log("[PlayerCombat] Stopped movement animation for combat");
            }

            // pick initial closest target
            CurrentTarget = FindClosestEnemy();

            // Fire target changed event (from null to first target)
            GameEvents.PlayerTargetChanged(null, CurrentTarget);

            // pass the full array to GameEvents
            GameEvents.PlayerEnteredCombat(enemiesInRange.ToArray());

            if(debug) Debug.Log($"Entering combat with {enemiesInRange.Count} enemies. Target: {CurrentTarget?.name ?? "none"}");
        }
    }

    private void ExitCombatIfNoEnemies()
    {
        // only exit if player is in combat and not currently gathering enemies
        if (InCombat && !gatheringEnemies && enemiesInRange.Count == 0)
        {
            InCombat = false;
            
            // Fire target changed event (from current target to null)
            if (CurrentTarget != null)
            {
                // Check if CurrentTarget is still valid before passing it to the event
                Transform oldTarget = null;
                try
                {
                    string name = CurrentTarget.name; // This will throw if destroyed
                    oldTarget = CurrentTarget;
                }
                catch (System.Exception)
                {
                    // Transform was destroyed, set to null
                    oldTarget = null;
                }
                
                GameEvents.PlayerTargetChanged(oldTarget, null);
            }
            
            CurrentTarget = null;

            // allow movement again
            movement.CanMove = true;
            
            // Resume movement animation when exiting combat
            if (frameAnimator != null && movement.IsMoving())
            {
                frameAnimator.PlayRunAnimation();
                if (debug) Debug.Log("[PlayerCombat] Resumed running animation after combat");
            }

            if (gatherRoutine != null)
            {
                StopCoroutine(gatherRoutine);
                gatherRoutine = null;
            }

            GameEvents.PlayerExitedCombat();
            if(debug) Debug.Log("No enemies left, exiting combat");
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
