using System.Collections.Generic;
using UnityEngine;

public class BossSpawner : SpawnerBase
{
    private static readonly string[] CombatTags = { "Boss" };

    public BossSpawner(FloorManager floorManager) : base(floorManager)
    {
    }

    public override void Spawn(FloorSpawnContext context)
    {
        if (context.BossPrefab == null)
        {
            DebugError("[BossSpawner] Boss prefab is not assigned. Cannot spawn boss.");
            return;
        }

        if (context.BossSpawnPoint == null)
        {
            DebugError("[BossSpawner] Boss spawn point is not assigned. Cannot spawn boss.");
            return;
        }

        GameObject boss = Object.Instantiate(context.BossPrefab, context.BossSpawnPoint.position, context.BossSpawnPoint.rotation);

        EnemyStatScaler scaler = boss.GetComponent<EnemyStatScaler>();
        if (scaler != null) { scaler.ScaleToFloor(context.Floor); }

        EnemyTierVisual visual = boss.GetComponent<EnemyTierVisual>();
        if (visual != null) { visual.RefreshVisual(); }

        if (!boss.activeInHierarchy) { boss.SetActive(true); }

        GameEvents.EnemySpawned(boss);

        DebugLog($"[BossSpawner] Spawned boss '{boss.name}' for floor {context.Floor}");
    }

    public void RefreshSpawnPointReference()
    {
        if (floorManager.BossSpawnPoint != null)
            return;

        base.RefreshSpawnPointReference(
            "BossSpawnPoint",
            transform => floorManager.BossSpawnPoint = transform);
    }

    protected override IReadOnlyList<string> CombatantTags => CombatTags;

    protected override void DisableCombatants()
    {
        int bossesDisabled = ApplyCombatAction(CombatantTags, (boss, combat) =>
        {
            combat.SendMessage("DisableCombat", SendMessageOptions.DontRequireReceiver);
            DebugLog($"[BossSpawner] Disabled combat for boss: {boss.name}");
        });

        DebugLog($"[BossSpawner] Disabled combat for {bossesDisabled} bosses");
    }

    protected override void EnableCombatImmediate()
    {
        int bossesEnabled = ApplyCombatAction(CombatantTags, (boss, combat) =>
        {
            combat.ForceEnableCombat();
            DebugLog($"[BossSpawner] Enabled combat for boss: {boss.name}");
        });

        DebugLog($"[BossSpawner] Enabled combat for {bossesEnabled} bosses");
    }
}