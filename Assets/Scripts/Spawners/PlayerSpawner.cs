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
        // Note: ResetCombat() is now called by FloorManager after enemies are spawned
        // to ensure enemies exist when combat is enabled

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
        if (health != null)
        {
            // Always reset health system when spawning on floor 1 (new game) or when health is 0
            // This ensures shield immunity and other states are cleared
            // For floor 1, ALWAYS reset to ensure fresh start, regardless of current health
            if (context.Floor == 1)
            {
                // Floor 1 = new game, always reset everything
                health.ResetHealth();
                // Explicitly clear shield immunity as a safety measure
                health.SetShieldImmunity(false);
                DebugLog($"[PlayerSpawner] Reset player health for new game (Floor: {context.Floor}, Health was: {health.CurrentHealth})");

                if (PlayerSkills.Instance != null)
                {
                    PlayerSkills.Instance.ResetAllSkillCooldowns();
                    DebugLog("[PlayerSpawner] Reset all player skill cooldowns");
                }
            }
            else if (health.CurrentHealth <= 0)
            {
                // Player died, reset health
                health.ResetHealth();
                health.SetShieldImmunity(false);
                DebugLog($"[PlayerSpawner] Reset player health after death (Floor: {context.Floor})");
            }
            else
            {
                // Even if not on floor 1 and health > 0, ensure shield immunity is cleared
                // This prevents shield immunity from persisting between floors
                health.SetShieldImmunity(false);
                DebugLog($"[PlayerSpawner] Cleared shield immunity (Floor: {context.Floor})");
            }
        }

        var combat = playerGO.GetComponent<PlayerCombat>();
        if (combat != null)
        {
            combat.ForceResetCombatState();
            // Ensure combat component is enabled (may have been disabled during death effect)
            if (!combat.enabled)
            {
                combat.enabled = true;
                DebugLog("[PlayerSpawner] Re-enabled PlayerCombat component");
            }
            DebugLog("[PlayerSpawner] Reset player combat state");
        }
        
        // Ensure PlayerAttack is enabled (may have been disabled during death effect)
        var attack = playerGO.GetComponent<PlayerAttack>();
        if (attack != null && !attack.enabled)
        {
            attack.enabled = true;
            DebugLog("[PlayerSpawner] Re-enabled PlayerAttack component");
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

    protected override IEnumerator ForceEnableCombatAfterDelay(float delay)
    {
        DebugLog($"[PlayerSpawner] Starting combat enable delay: {delay} seconds");
        yield return new WaitForSeconds(delay);
        DebugLog("[PlayerSpawner] Delay complete, enabling combat...");
        EnableCombatImmediate();
    }

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
        Debug.Log("[PlayerSpawner] EnableCombatImmediate called");
        
        // Count enemies before trying to enable
        GameObject[] allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject[] allBosses = GameObject.FindGameObjectsWithTag("Boss");
        Debug.Log($"[PlayerSpawner] Found {allEnemies.Length} enemies and {allBosses.Length} bosses with tags");
        
        int enemiesEnabled = ApplyCombatAction("Enemy", (enemy, combat) =>
        {
            Debug.Log($"[PlayerSpawner] Attempting to enable combat for enemy: {enemy.name}");
            try
            {
                combat.ForceEnableCombat();
                Debug.Log($"[PlayerSpawner] Successfully called ForceEnableCombat for enemy: {enemy.name}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[PlayerSpawner] ForceEnableCombat failed for {enemy.name}: {e.Message}");
            }
            DebugLog($"[PlayerSpawner] Force enabled combat for enemy: {enemy.name}");
        });

        int bossesEnabled = ApplyCombatAction("Boss", (boss, combat) =>
        {
            Debug.Log($"[PlayerSpawner] Attempting to enable combat for boss: {boss.name}");
            try
            {
                combat.ForceEnableCombat();
                Debug.Log($"[PlayerSpawner] Successfully called ForceEnableCombat for boss: {boss.name}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[PlayerSpawner] ForceEnableCombat failed for {boss.name}: {e.Message}");
            }
            DebugLog($"[PlayerSpawner] Force enabled combat for boss: {boss.name}");
        });

        Debug.Log($"[PlayerSpawner] Force enabled combat for {enemiesEnabled} enemies and {bossesEnabled} bosses");
        
        // If no enemies were found, retry after a short delay (enemies might not be loaded yet)
        if (enemiesEnabled == 0 && bossesEnabled == 0 && floorManager != null)
        {
            Debug.Log("[PlayerSpawner] No enemies found, retrying combat enable after delay...");
            floorManager.StartCoroutine(RetryEnableCombat());
        }
    }
    
    private IEnumerator RetryEnableCombat()
    {
        const int maxRetries = 10;
        const float retryDelay = 0.2f;
        
        for (int i = 0; i < maxRetries; i++)
        {
            yield return new WaitForSeconds(retryDelay);
            
            int enemiesEnabled = ApplyCombatAction("Enemy", (enemy, combat) =>
            {
                try
                {
                    combat.ForceEnableCombat();
                    Debug.Log($"[PlayerSpawner] Retry {i + 1}: Successfully called ForceEnableCombat for enemy: {enemy.name}");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[PlayerSpawner] Retry {i + 1}: ForceEnableCombat failed for {enemy.name}: {e.Message}");
                }
                DebugLog($"[PlayerSpawner] Retry {i + 1}: Force enabled combat for enemy: {enemy.name}");
            });

            int bossesEnabled = ApplyCombatAction("Boss", (boss, combat) =>
            {
                try
                {
                    combat.ForceEnableCombat();
                    Debug.Log($"[PlayerSpawner] Retry {i + 1}: Successfully called ForceEnableCombat for boss: {boss.name}");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[PlayerSpawner] Retry {i + 1}: ForceEnableCombat failed for {boss.name}: {e.Message}");
                }
                DebugLog($"[PlayerSpawner] Retry {i + 1}: Force enabled combat for boss: {boss.name}");
            });
            
            if (enemiesEnabled > 0 || bossesEnabled > 0)
            {
                DebugLog($"[PlayerSpawner] Successfully enabled combat on retry {i + 1}: {enemiesEnabled} enemies, {bossesEnabled} bosses");
                yield break;
            }
        }
        
        DebugLog($"[PlayerSpawner] Failed to enable combat after {maxRetries} retries");
    }
}