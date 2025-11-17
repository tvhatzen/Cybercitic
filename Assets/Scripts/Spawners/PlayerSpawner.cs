using System.Collections;
using UnityEngine;

public class PlayerSpawner : SpawnerBase
{
    public PlayerSpawner(FloorManager floorManager) : base(floorManager)
    {
    }

    public override void Spawn(FloorSpawnContext context)
    {
        floorManager.StartCoroutine(SpawnRoutine(context));
    }

    public void RefreshSpawnPointReference()
    {
        if (floorManager.PlayerSpawnPoint != null)
            return;

        base.RefreshSpawnPointReference(
            "PlayerSpawnPoint",
            transform => floorManager.PlayerSpawnPoint = transform,
            "PlayerSpawn",
            logAllCandidates: true);
    }

    private IEnumerator SpawnRoutine(FloorSpawnContext context)
    {
        DebugLog($"[PlayerSpawner] Spawn routine started - Floor: {context.Floor}");

        while (PlayerInstance.Instance == null)
            yield return null;

        GameObject playerGO = PlayerInstance.Instance?.gameObject;

        if (playerGO == null)
        {
            if (context.PlayerPrefab != null && context.PlayerSpawnPoint != null)
            {
                DebugLog($"[PlayerSpawner] Instantiating player at spawn point: {context.PlayerSpawnPoint.position}");
                playerGO = Object.Instantiate(context.PlayerPrefab, context.PlayerSpawnPoint.position, context.PlayerSpawnPoint.rotation);
            }
            else
            {
                Debug.LogError($"[PlayerSpawner] Missing player prefab ({context.PlayerPrefab != null}) or spawn point ({context.PlayerSpawnPoint != null}).");
                yield break;
            }
        }

        playerGO.SetActive(true);

        ResetPlayerState(playerGO, context);
        ResetCombat();

        yield return null;

        ValidatePlayerComponents(playerGO);
    }

    private void ResetPlayerState(GameObject playerGO, FloorSpawnContext context)
    {
        var movement = playerGO.GetComponent<PlayerMovement>();
        if (movement != null && context.PlayerSpawnPoint != null)
        {
            DebugLog($"[PlayerSpawner] Resetting player position to spawn point: {context.PlayerSpawnPoint.position}");
            movement.ResetToSpawn(context.PlayerSpawnPoint);
        }
        else if (DebugEnabled)
        {
            Debug.LogError($"[PlayerSpawner] Cannot reset player position - movement component present: {movement != null}, spawn point assigned: {context.PlayerSpawnPoint != null}");
        }

        var health = playerGO.GetComponent<HealthSystem>();
        if (health != null && health.CurrentHealth <= 0)
        {
            health.ResetHealth();
            DebugLog("[PlayerSpawner] Reset player health after death");

            if (PlayerSkills.Instance != null)
            {
                PlayerSkills.Instance.ResetAllSkillCooldowns();
                DebugLog("[PlayerSpawner] Reset all player skill cooldowns");
            }
        }

        var combat = playerGO.GetComponent<PlayerCombat>();
        if (combat != null)
        {
            combat.ForceResetCombatState();
            DebugLog("[PlayerSpawner] Reset player combat state");
        }

        var healthBar = playerGO.GetComponent<HealthBar>();
        if (healthBar != null)
        {
            healthBar.ResetForeground();
        }
    }

    private void ValidatePlayerComponents(GameObject playerGO)
    {
        var entityData = playerGO.GetComponent<EntityData>();
        if (entityData == null)
        {
            Debug.LogError("[PlayerSpawner] Player spawned without EntityData component!");
        }
        else
        {
            DebugLog($"[PlayerSpawner] Player spawned with EntityData. Speed: {entityData.currentSpeed}");
        }
    }

    protected override float DefaultCombatEnableDelay => 0.5f;

    protected override void DisableCombatants()
    {
        int enemiesDisabled = ApplyCombatAction("Enemy", (enemy, combat) =>
        {
            combat.SendMessage("DisableCombat", SendMessageOptions.DontRequireReceiver);
            DebugLog($"[PlayerSpawner] Reset combat state for enemy: {enemy.name}");
        });

        int bossesDisabled = ApplyCombatAction("Boss", (boss, combat) =>
        {
            combat.SendMessage("DisableCombat", SendMessageOptions.DontRequireReceiver);
            DebugLog($"[PlayerSpawner] Reset combat state for boss: {boss.name}");
        });

        DebugLog($"[PlayerSpawner] Reset combat states for {enemiesDisabled} enemies and {bossesDisabled} bosses");
    }

    protected override void EnableCombatImmediate()
    {
        int enemiesEnabled = ApplyCombatAction("Enemy", (enemy, combat) =>
        {
            combat.ForceEnableCombat();
            DebugLog($"[PlayerSpawner] Force enabled combat for enemy: {enemy.name}");
        });

        int bossesEnabled = ApplyCombatAction("Boss", (boss, combat) =>
        {
            combat.ForceEnableCombat();
            DebugLog($"[PlayerSpawner] Force enabled combat for boss: {boss.name}");
        });

        DebugLog($"[PlayerSpawner] Force enabled combat for {enemiesEnabled} enemies and {bossesEnabled} bosses");
    }
}