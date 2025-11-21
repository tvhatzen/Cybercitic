using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SpawnerBase
{
    protected readonly FloorManager floorManager;

    protected SpawnerBase(FloorManager floorManager)
    {
        this.floorManager = floorManager ?? throw new ArgumentNullException(nameof(floorManager));
    }

    protected bool DebugEnabled => floorManager != null && floorManager.debug;

    public virtual void Initialize() { }

    public abstract void Spawn(FloorSpawnContext context);

    protected virtual float DefaultCombatEnableDelay => 0f;

    protected virtual IReadOnlyList<string> CombatantTags => Array.Empty<string>();

    public void ResetCombat()
    {
        ResetCombat(null);
    }

    public void ResetCombat(float? overrideEnableDelaySeconds)
    {
        DisableCombatants();

        float delay = overrideEnableDelaySeconds ?? DefaultCombatEnableDelay;

        if (delay > 0f && floorManager != null)
        {
            IEnumerator routine = ForceEnableCombatAfterDelay(delay);
            if (routine != null)
            {
                floorManager.StartCoroutine(routine);
            }
        }
        else
        {
            EnableCombatImmediate();
        }
    }

    protected abstract void DisableCombatants();

    protected virtual void EnableCombatImmediate() { }

    protected virtual IEnumerator ForceEnableCombatAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        EnableCombatImmediate();
    }

    protected int ApplyCombatAction(string tag, Action<GameObject, EnemyCombat> action)
    {
        if (string.IsNullOrWhiteSpace(tag) || action == null)
            return 0;

        GameObject[] combatants = GameObject.FindGameObjectsWithTag(tag);
        Debug.Log($"[SpawnerBase] ApplyCombatAction for tag '{tag}': Found {combatants.Length} objects with this tag");
        
        int affected = 0;
        int missingComponent = 0;

        foreach (GameObject combatant in combatants)
        {
            EnemyCombat enemyCombat = combatant.GetComponent<EnemyCombat>();
            if (enemyCombat == null)
            {
                missingComponent++;
                Debug.LogWarning($"[SpawnerBase] {combatant.name} has tag '{tag}' but no EnemyCombat component!");
                continue;
            }

            action(combatant, enemyCombat);
            affected++;
        }
        
        if (missingComponent > 0)
        {
            Debug.LogWarning($"[SpawnerBase] {missingComponent} objects with tag '{tag}' were skipped due to missing EnemyCombat component");
        }

        return affected;
    }

    protected int ApplyCombatAction(IEnumerable<string> tags, Action<GameObject, EnemyCombat> action)
    {
        if (tags == null)
            return 0;

        int total = 0;

        foreach (string tag in tags)
        {
            total += ApplyCombatAction(tag, action);
        }

        return total;
    }

    protected void RefreshSpawnPointReference(
        string objectName,
        Action<Transform> onFound,
        string fallbackTag = null,
        bool logAllCandidates = false)
    {
        if (string.IsNullOrEmpty(objectName))
            throw new ArgumentException("Spawn object name must be provided", nameof(objectName));

        if (onFound == null)
            throw new ArgumentNullException(nameof(onFound));

        GameObject spawnGO = GameObject.Find(objectName);

        if (spawnGO == null && !string.IsNullOrEmpty(fallbackTag))
        {
            spawnGO = GameObject.FindGameObjectWithTag(fallbackTag);
            DebugLog($"[SpawnerBase] Search by tag '{fallbackTag}' => {(spawnGO != null ? "found" : "not found")}");
        }

        if (spawnGO == null && logAllCandidates)
        {
            GameObject[] allObjects = UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            DebugLog($"[SpawnerBase] Listing potential spawn candidates. Total objects: {allObjects.Length}");
            foreach (GameObject obj in allObjects)
            {
                string lower = obj.name.ToLower();
                if (lower.Contains("spawn"))
                {
                    DebugLog($"[SpawnerBase] Candidate: '{obj.name}' (Active: {obj.activeInHierarchy})");
                }
            }
        }

        if (spawnGO != null)
        {
            onFound(spawnGO.transform);
            DebugLog($"[SpawnerBase] Refreshed spawn reference using '{spawnGO.name}' at position {spawnGO.transform.position}");
        }
        else
        {
            string fallbackDetails = string.IsNullOrEmpty(fallbackTag) ? string.Empty : $" or tag '{fallbackTag}'";
            DebugError($"[SpawnerBase] Could not find spawn object '{objectName}'{fallbackDetails}");
        }
    }

    protected void DebugLog(string message)
    {
        if (DebugEnabled)
            Debug.Log(message);
    }

    protected void DebugWarning(string message)
    {
        if (DebugEnabled)
            Debug.LogWarning(message);
    }

    protected void DebugError(string message)
    {
        if (DebugEnabled)
            Debug.LogError(message);
    }
}

