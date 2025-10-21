using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;
using System.Collections;

public class FloorManager : SingletonBase<FloorManager>
{
    [Header("Player")]
    public GameObject playerPrefab;
    public Transform playerSpawnPoint;


    [Header("Enemies")]
    public List<GameObject> enemyPrefabsForThisFloor;
    public List<Transform> enemySpawnPoints;

    [Header("Boss Settings")]
    public GameObject bossPrefab;
    public Transform bossSpawnPoint;
    public int bossFloorInterval = 5; // Boss every 5 floors (5, 10, 15, 20, etc.)

    public static event Action<int> OnFloorChanged; // notify UI 
    public static event Action<GameObject> OnEnemySpawned;

    public int CurrentFloor { get; private set; } = 1;

    private bool hasSpawnedOnLoad = false;

    public bool debug = false;

    public FloorProgressBar _floorProgressBar;

    protected override void Awake()
    {
        base.Awake(); 
        
        // Only subscribe to events if this is the valid instance
        if (Instance == this)
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
    }

    void OnDestroy()
    {
        // Only unsubscribe if this was the valid instance
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    void Start()
    {
        if (!hasSpawnedOnLoad)
        {
            StartCoroutine(SpawnPlayerCoroutine());
            SpawnEnemies();
        }
    }

    // called when progressing floors WITHOUT a scene load (used for same-scene floor progression)
    public void LoadNextFloor()
    {
        IncrementFloor();

        // reset player using singleton
        StartCoroutine(SpawnPlayerCoroutine());

        // spawn enemies for the floor
        SpawnEnemies();
    }

    // increment floor counter and notify listeners (called before scene loads)
    public void IncrementFloor()
    {
        int oldFloor = CurrentFloor;
        CurrentFloor = CurrentFloor + 1;
        
        if (debug) Debug.Log($"[FloorManager] IncrementFloor - Floor changed from {oldFloor} to {CurrentFloor}");
        
        OnFloorChanged?.Invoke(CurrentFloor);

        _floorProgressBar.IncreaseProgressAmount(1);

        // tell run stats tracker
        if (RunStatsTracker.Instance != null)
        {
            RunStatsTracker.Instance.FloorCleared(CurrentFloor - 1); // previous floor was cleared
        }
    }

    // reset to floor 1 for retry. keeps player upgrades but resets floor progression
    public void ResetToFloor1() // right now after death/win, not resetting to floor 1 properly
    {
        if (debug) Debug.Log("[FloorManager] Resetting to Floor 1 for retry");

        // Reset floor counter first
        CurrentFloor = 1;
        OnFloorChanged?.Invoke(CurrentFloor);

        _floorProgressBar.ResetProgress(); 

        if (debug) Debug.Log($"[FloorManager] Floor changed event triggered: Floor {CurrentFloor}");

        UpdateFloorUIBackup();

        // Load the first scene (Gameplay) to ensure we start from the beginning
        SceneManager.LoadScene("Gameplay");
    }

    // restore players health to full (for retry after death)
    private void RestorePlayerHealth()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            HealthSystem playerHealth = player.GetComponent<HealthSystem>();
            if (playerHealth != null)
            {
                playerHealth.ResetHealth();
                if (debug) Debug.Log("[FloorManager] Player health restored to full");
            }
        }
    }

    // backup method to directly update FloorUI if event system has timing issues
    private void UpdateFloorUIBackup()
    {
        FloorUI floorUI = FindFirstObjectByType<FloorUI>();
        if (floorUI != null)
        {
            floorUI.UpdateFloorText(CurrentFloor);
            if (debug) Debug.Log($"[FloorManager] Backup FloorUI update called for Floor {CurrentFloor}");
        }
    }

    // refresh Transform references after scene load since they get lost when transitioning scenes
    private void RefreshSpawnPointReferences()
    {
        if (debug) Debug.Log("[FloorManager] Refreshing spawn point references after scene load");
        
        // Refresh player spawn point
        if (playerSpawnPoint == null)
        {
            if (debug) Debug.Log("[FloorManager] PlayerSpawnPoint reference is null, searching for GameObject...");
            
            // Try multiple search methods
            GameObject playerSpawnGO = GameObject.Find("PlayerSpawnPoint");
            
            if (playerSpawnGO == null)
            {
                // Try finding by tag if it has one
                playerSpawnGO = GameObject.FindGameObjectWithTag("PlayerSpawn");
                if (debug) Debug.Log($"[FloorManager] Found by tag 'PlayerSpawn': {playerSpawnGO != null}");
            }
            
            if (playerSpawnGO == null)
            {
                // List all GameObjects to see what's available
                GameObject[] allObjects = FindObjectsOfType<GameObject>();
                if (debug) Debug.Log($"[FloorManager] Total GameObjects in scene: {allObjects.Length}");
                foreach (GameObject obj in allObjects)
                {
                    if (obj.name.ToLower().Contains("spawn") || obj.name.ToLower().Contains("player"))
                    {
                        if (debug) Debug.Log($"[FloorManager] Found related GameObject: '{obj.name}' (Active: {obj.activeInHierarchy})");
                    }
                }
            }
            
            if (playerSpawnGO != null)
            {
                playerSpawnPoint = playerSpawnGO.transform;
                if (debug) Debug.Log($"[FloorManager] Found and refreshed PlayerSpawnPoint reference: {playerSpawnPoint.position}");
            }
            else
            {
                Debug.LogError("[FloorManager] Could not find PlayerSpawnPoint in the new scene!");
            }
        }

        // Refresh enemy spawn points
        for (int i = 0; i < enemySpawnPoints.Count; i++)
        {
            if (enemySpawnPoints[i] == null)
            {
                GameObject enemySpawnGO = GameObject.Find($"EnemySpawnPoint{i}");
                if (enemySpawnGO != null)
                {
                    enemySpawnPoints[i] = enemySpawnGO.transform;
                    if (debug) Debug.Log($"[FloorManager] Found and refreshed EnemySpawnPoint{i} reference");
                }
                else
                {
                    Debug.LogError($"[FloorManager] Could not find EnemySpawnPoint{i} in the new scene!");
                }
            }
        }

        // Refresh boss spawn point
        if (bossSpawnPoint == null)
        {
            GameObject bossSpawnGO = GameObject.Find("BossSpawnPoint");
            if (bossSpawnGO != null)
            {
                bossSpawnPoint = bossSpawnGO.transform;
                if (debug) Debug.Log("[FloorManager] Found and refreshed BossSpawnPoint reference");
            }
            else
            {
                Debug.LogError("[FloorManager] Could not find BossSpawnPoint in the new scene!");
            }
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (debug) Debug.Log($"[FloorManager] OnSceneLoaded - Scene: {scene.name}, Floor: {CurrentFloor}");
        
        // refresh Transform references since they get lost when scene changes
        RefreshSpawnPointReferences();

        // Ensure time is running when loading a new gameplay floor
        if (debug) Debug.Log($"[FloorManager] OnSceneLoaded - Setting Time.timeScale to 1f for gameplay");
        Time.timeScale = 1f;

        // Set game state to Playing when loading a new gameplay floor
        if (GameState.Instance != null)
        {
            if (debug) Debug.Log($"[FloorManager] Current GameState: {GameState.Instance.CurrentState}, changing to Playing");
            GameState.Instance.ChangeState(GameState.GameStates.Playing);
        }

        // reset player position whenever a new floor scene loads
        hasSpawnedOnLoad = true;
        StartCoroutine(SpawnPlayerCoroutine());
        SpawnEnemies();
    }

    public IEnumerator SpawnPlayerCoroutine()
    {
        if (debug) Debug.Log($"[FloorManager] SpawnPlayerCoroutine started - Floor: {CurrentFloor}");
        
        // wait until PlayerInstance exists
        while (PlayerInstance.Instance == null)
            yield return null;

        GameObject playerGO = PlayerInstance.Instance?.gameObject;

        if (playerGO == null)
        {
            if (playerPrefab != null && playerSpawnPoint != null)
            {
                if (debug) Debug.Log($"[FloorManager] Creating new player at spawn point: {playerSpawnPoint.position}");
                GameObject newPlayer = Instantiate(playerPrefab, playerSpawnPoint.position, playerSpawnPoint.rotation);
                playerGO = newPlayer;
            }
            else
            {
                if (debug) Debug.LogError($"No Player prefab ({playerPrefab != null}) or spawn point ({playerSpawnPoint != null}) assigned to FloorManager!");
                yield break;
            }
        }

        playerGO.SetActive(true);

        // reset position
        var movement = playerGO.GetComponent<PlayerMovement>();
        if (movement != null && playerSpawnPoint != null)
        {
            if (debug) Debug.Log($"[FloorManager] Resetting player to spawn point: {playerSpawnPoint.position}");
            movement.ResetToSpawn(playerSpawnPoint);
        }
        else
        {
            if (debug) Debug.LogError($"[FloorManager] Cannot reset player - movement: {movement != null}, spawnPoint: {playerSpawnPoint != null}");
        }

        // reset health
        var health = playerGO.GetComponent<HealthSystem>();
        if (health != null && health.CurrentHealth <= 0)
            health.ResetHealth(); // only reset if dead

        // reset skill cooldowns when respawning
        if (PlayerSkills.Instance != null)
        {
            //PlayerSkills.Instance.ResetAllSkillCooldowns(); // TESTING: dont reset skill cooldown when respawning
            if (debug) Debug.Log("[FloorManager] Reset all skill cooldowns on player spawn");
        }

        yield return null;
        
        // make sure EntityData component exists 
        var entityData = playerGO.GetComponent<EntityData>();
        if (entityData == null)
        {
            Debug.LogError("[FloorManager] Player spawned but EntityData component is missing!");
        }
        else
        {
            if (debug) Debug.Log($"[FloorManager] Player spawned successfully with EntityData. Speed: {entityData.currentSpeed}");
        }

        // reset health bar foreground


        playerGO.SetActive(true);
    }

    void SpawnEnemies()
    {
        // clear existing enemies
        GameObject[] existingEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in existingEnemies)
        {
            Destroy(enemy);
        }

        // check for boss floor (every 5 floors: 5, 10, 15, 20, etc.)
        bool isBossFloor = CurrentFloor % bossFloorInterval == 0;
        if (debug) Debug.Log($"[FloorManager] SpawnEnemies - CurrentFloor: {CurrentFloor}, bossFloorInterval: {bossFloorInterval}, isBossFloor: {isBossFloor}, bossPrefab: {(bossPrefab != null ? bossPrefab.name : "null")}");

        if (isBossFloor && bossPrefab != null)
        {
            if (debug) Debug.Log($"[FloorManager] Spawning BOSS for floor {CurrentFloor} (boss floor!)");
            SpawnBoss();
        }
        else
        {
            if (debug) Debug.Log($"[FloorManager] Spawning regular enemies for floor {CurrentFloor}");
            SpawnRegularEnemies();
        }

        foreach (GameObject enemy in existingEnemies)
        {
            enemy.GetComponent<EnemyVisualFeedback>()?.PlaySpawnEffect(); // not triggering ??
        }
        
    }

    private void SpawnBoss()
    {
        if (bossSpawnPoint == null)
        {
            if (debug) Debug.LogError("[FloorManager] Boss prefab assigned but no boss spawn point set!");
            return;
        }

        var boss = Instantiate(bossPrefab, bossSpawnPoint.position, bossSpawnPoint.rotation);

        // scale boss stats based on current floor
        EnemyStatScaler scaler = boss.GetComponent<EnemyStatScaler>();
        if (scaler != null)
        {
            scaler.ScaleToFloor(CurrentFloor);
        }

        // update tier indicator
        EnemyTierVisual visual = boss.GetComponent<EnemyTierVisual>();
        if (visual != null)
            visual.RefreshVisual();

        // ensure boss is active
        if (!boss.activeInHierarchy)
            boss.SetActive(true);

        // fire event for boss
        OnEnemySpawned?.Invoke(boss);

        if (debug) Debug.Log($"Spawned BOSS for floor {CurrentFloor}");
    }

    private void SpawnRegularEnemies()
    {
        for (int i = 0; i < enemySpawnPoints.Count; i++)
        {
            if (i < enemyPrefabsForThisFloor.Count)
            {
                GameObject prefab = enemyPrefabsForThisFloor[i];
                Transform point = enemySpawnPoints[i];

                // Check if spawn point exists before trying to use it
                if (point == null)
                {
                    if (debug) Debug.LogError($"[FloorManager] EnemySpawnPoint{i} is null - cannot spawn enemy!");
                    continue;
                }

                var enemy = Instantiate(prefab, point.position, point.rotation);

                // scale stats based on current floor
                EnemyStatScaler scaler = enemy.GetComponent<EnemyStatScaler>();
                if (scaler != null)
                    scaler.ScaleToFloor(CurrentFloor);

                // update tier indicator
                EnemyTierVisual visual = enemy.GetComponent<EnemyTierVisual>();
                if (visual != null)
                    visual.RefreshVisual();

                // ensure enemy is active
                if (!enemy.activeInHierarchy)
                {
                    if (debug) Debug.LogWarning($"[FloorManager] Enemy {enemy.name} is not active! Activating...");
                    enemy.SetActive(true);
                }

                // fire event for each enemy
                OnEnemySpawned?.Invoke(enemy);
            }
        }

        if (debug) Debug.Log($"Spawned {Mathf.Min(enemySpawnPoints.Count, enemyPrefabsForThisFloor.Count)} regular enemies for floor {CurrentFloor}");
    }

    public void SetBossFloorInterval(int interval)
    {
        bossFloorInterval = interval;
        if (debug) Debug.Log($"Boss floor interval set to: {bossFloorInterval} (boss every {interval} floors)");
    }

    // check if current floor is boss floor
    public bool IsBossFloor() { return CurrentFloor % bossFloorInterval == 0; }
}
//  create more general spawner 
// list of spawners
// fires off signal to player spawner to run that logic separately