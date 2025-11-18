using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : SpawnerBase
{
    private static readonly string[] CombatTags = { "Enemy" };

    public EnemySpawner(FloorManager floorManager) : base(floorManager)
    {
    }

    public List<string> enemyNames = new List<string>();

    // dictionary to correlate floor number to enemies (3)
    public Dictionary<int, List<string>> enemiesForThisFloor = new Dictionary<int, List<string>>();

    // new spawning system
    // get prefabs from names, instantiate at spawnpoint(s)
    // eventually, refresh when going to new floor 
    // find index of dictionary -> to floor level

    public void ClearEnemies()
    {
        enemiesForThisFloor.Clear();
    }

    public void GetPrefabFromName(string name)
    {
        // get prefab based on name given
        // name = GameObject.FindGameObjectWithTag($"{name}");
    }

    public void LoadEnemySpawnData()
    {
        enemiesForThisFloor.Add(1, new List<string> { "Enemy_Basic", "Enemy_Basic", "Enemy_Basic" });
        enemiesForThisFloor.Add(2, new List<string> { "Enemy_Basic", "Enemy_Basic", "Enemy_Elite" });
        enemiesForThisFloor.Add(3, new List<string> { "Enemy_Basic", "Enemy_Elite", "Enemy_Elite" });
        enemiesForThisFloor.Add(4, new List<string> { "Enemy_Elite", "Enemy_Elite", "Enemy_Elite" });
        enemiesForThisFloor.Add(5, new List<string> { "Enemy_Boss" });
        enemiesForThisFloor.Add(6, new List<string> { "Enemy_Basic", "Enemy_Basic", "Enemy_Basic" });
        enemiesForThisFloor.Add(7, new List<string> { "Enemy_Basic", "Enemy_Basic", "Enemy_Elite" });
        enemiesForThisFloor.Add(8, new List<string> { "Enemy_Basic", "Enemy_Elite", "Enemy_Elite" });
        enemiesForThisFloor.Add(9, new List<string> { "Enemy_Elite", "Enemy_Elite", "Enemy_Elite" });
        enemiesForThisFloor.Add(10, new List<string> { "Enemy_Boss" });
        enemiesForThisFloor.Add(11, new List<string> { "Enemy_Basic", "Enemy_Basic", "Enemy_Basic" });
        enemiesForThisFloor.Add(12, new List<string> { "Enemy_Basic", "Enemy_Basic", "Enemy_Elite" });
        enemiesForThisFloor.Add(13, new List<string> { "Enemy_Basic", "Enemy_Elite", "Enemy_Elite" });
        enemiesForThisFloor.Add(14, new List<string> { "Enemy_Elite", "Enemy_Elite", "Enemy_Elite" });
        enemiesForThisFloor.Add(15, new List<string> { "Enemy_Boss" });
    }

    public void ClearExistingEnemies()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            Object.Destroy(enemy);
        }

        GameObject[] bosses = GameObject.FindGameObjectsWithTag("Boss");
        foreach (GameObject boss in bosses)
        {
            Object.Destroy(boss);
        }

        if (DebugEnabled)
        {
            Debug.Log($"[EnemySpawner] Cleared {enemies.Length} enemies and {bosses.Length} bosses before spawning");
        }
    }

    public override void Spawn(FloorSpawnContext context) // take in dictionary?
    {
        if (context.IsBossFloor)
        {
            DebugLog("[EnemySpawner] Skipping regular enemy spawn on boss floor");
            return;
        }

        int spawnedCount = 0;

        for (int i = 0; i < context.EnemySpawnPoints.Count; i++)
        {
            if (i >= context.EnemyPrefabs.Count)
                break;

            // make this the index of the current floor (would need to be multiple)
            GameObject prefab = context.EnemyPrefabs[i]; // needs change
            Transform point = context.EnemySpawnPoints[i];

            if (prefab == null)
            {
                DebugWarning($"[EnemySpawner] Enemy prefab at index {i} is null - skipping spawn");
                continue;
            }

            if (point == null)
            {
                DebugError($"[EnemySpawner] Enemy spawn point {i} is null - cannot spawn enemy");
                continue;
            }

            GameObject enemy = Object.Instantiate(prefab, point.position, point.rotation);

            EnemyStatScaler scaler = enemy.GetComponent<EnemyStatScaler>();
            if (scaler != null)
            {
                scaler.ScaleToFloor(context.Floor);
            }

            EnemyTierVisual visual = enemy.GetComponent<EnemyTierVisual>();
            if (visual != null)
            {
                visual.RefreshVisual();
            }

            if (!enemy.activeInHierarchy)
            {
                enemy.SetActive(true);
                DebugWarning($"[EnemySpawner] Enemy {enemy.name} was not active. Activating...");
            }

            GameEvents.EnemySpawned(enemy);
            spawnedCount++;
        }

        DebugLog($"[EnemySpawner] Spawned {spawnedCount} regular enemies for floor {context.Floor}");
    }

    public void RefreshSpawnPointReferences()
    {
        var spawnPoints = floorManager.EnemySpawnPointsInternal;

        for (int i = 0; i < spawnPoints.Count; i++)
        {
            if (spawnPoints[i] != null)
                continue;

            int index = i;
            base.RefreshSpawnPointReference(
                $"EnemySpawnPoint{index}",
                transform => spawnPoints[index] = transform);
        }
    }

    protected override IReadOnlyList<string> CombatantTags => CombatTags;

    protected override void DisableCombatants()
    {
        int enemiesDisabled = ApplyCombatAction(CombatantTags, (enemy, combat) =>
        {
            combat.SendMessage("DisableCombat", SendMessageOptions.DontRequireReceiver);
            DebugLog($"[EnemySpawner] Disabled combat for enemy: {enemy.name}");
        });

        DebugLog($"[EnemySpawner] Disabled combat for {enemiesDisabled} enemies");
    }

    protected override void EnableCombatImmediate()
    {
        int enemiesEnabled = ApplyCombatAction(CombatantTags, (enemy, combat) =>
        {
            combat.ForceEnableCombat();
            DebugLog($"[EnemySpawner] Enabled combat for enemy: {enemy.name}");
        });

        DebugLog($"[EnemySpawner] Enabled combat for {enemiesEnabled} enemies");
    }
}