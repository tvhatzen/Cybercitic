using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;
using System.Collections;

// each floor should have its own assigned enemies
// (when splitting into spawner classes, set enemy spawns per floor)
public class FloorManager : SingletonBase<FloorManager>
{
    [Header("Player")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform playerSpawnPoint;

    [Header("Enemies")]
    // change this to use the specified enemies per floor
    [SerializeField] private List<GameObject> enemyPrefabsForThisFloor;
    [SerializeField] private List<Transform> enemySpawnPoints;

    [Header("Boss Settings")]
    [SerializeField] private GameObject bossPrefab;
    [SerializeField] private Transform bossSpawnPoint;
    public int bossFloorInterval = 5; // Boss every 5 floors (5, 10, 15, 20, etc.)
    public bool isFinalFloor = false;
    public bool isRunning; // track if there's a current run

    public int CurrentFloor { get; private set; } = 1;

    private bool hasSpawnedOnLoad = false;

    public bool debug = false;

    public FloorProgressBar _floorProgressBar;

    public Transform PlayerSpawnPoint
    {
        get => playerSpawnPoint;
        internal set => playerSpawnPoint = value;
    }

    public IReadOnlyList<Transform> EnemySpawnPoints => enemySpawnPoints;

    public Transform BossSpawnPoint
    {
        get => bossSpawnPoint;
        internal set => bossSpawnPoint = value;
    }

    internal List<Transform> EnemySpawnPointsInternal => enemySpawnPoints;

    private PlayerSpawner playerSpawner;
    private EnemySpawner enemySpawner;
    private BossSpawner bossSpawner;
    private FloorMusicManager floorMusicManager;

    protected override void Awake()
    {
        base.Awake(); 
        InitializeSystems();

        enemyPrefabsForThisFloor ??= new List<GameObject>();
        enemySpawnPoints ??= new List<Transform>();
        
        if (Instance == this)
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void InitializeSystems()
    {
        playerSpawner = new PlayerSpawner(this);
        enemySpawner = new EnemySpawner(this);
        bossSpawner = new BossSpawner(this);
        floorMusicManager = new FloorMusicManager(this);
    }

    private FloorSpawnContext BuildSpawnContext()
    {
        return new FloorSpawnContext(
            this,
            playerPrefab,
            playerSpawnPoint,
            enemyPrefabsForThisFloor,
            enemySpawnPoints,
            bossPrefab,
            bossSpawnPoint,
            CurrentFloor,
            IsBossFloor());
    }

    void Start()
    {
        if (!hasSpawnedOnLoad)
        {
            FloorSpawnContext context = BuildSpawnContext();
            playerSpawner.Spawn(context);
            SpawnEnemies(context);
            
            // Enable combat after enemies are spawned (ensures enemies exist when combat is enabled)
            StartCoroutine(EnableCombatAfterEnemySpawn());
        }
    }

    // called when progressing floors WITHOUT a scene load (used for same-scene floor progression)
    public void LoadNextFloor()
    {
        IncrementFloor();

        FloorSpawnContext context = BuildSpawnContext();
        playerSpawner.Spawn(context);

        SpawnEnemies(context);
        
        // Enable combat after enemies are spawned (ensures enemies exist when combat is enabled)
        StartCoroutine(EnableCombatAfterEnemySpawn());

        // ensure background music updates when progressing floors within the same scene
        floorMusicManager.PlayForFloor(CurrentFloor);
    }

    // increment floor counter and notify listeners (called before scene loads)
    public void IncrementFloor()
    {
        int oldFloor = CurrentFloor;
        CurrentFloor = CurrentFloor + 1;
        
        if (debug) Debug.Log($"[FloorManager] IncrementFloor - Floor changed from {oldFloor} to {CurrentFloor}");
        
        GameEvents.FloorChanged(CurrentFloor);

        _floorProgressBar.IncreaseProgressAmount(1);

        GameEvents.FloorCleared(CurrentFloor - 1); // previous floor was cleared
    }

    // reset to floor 1 for retry. keeps player upgrades but resets floor progression
    public void ResetToFloor1() 
    {
        if (debug) Debug.Log("[FloorManager] Resetting to Floor 1 for retry");

        // Reset floor counter first
        CurrentFloor = 1;
        
        GameEvents.FloorChanged(CurrentFloor);

        _floorProgressBar.ResetProgress(); 

        if (debug) Debug.Log($"[FloorManager] Floor changed event triggered: Floor {CurrentFloor}");

        UpdateFloorUIBackup();

        SceneManager.LoadScene("Gameplay");
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

        playerSpawner.RefreshSpawnPointReference();
        
        // Refresh SpawnManager's spawn point references
        if (SpawnManager.Instance != null)
        {
            SpawnManager.Instance.RefreshSpawnPointReferences();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (debug) Debug.Log($"[FloorManager] OnSceneLoaded - Scene: {scene.name}, Floor: {CurrentFloor}");
        
        // Check for NextLevelTrigger before doing anything else
        trigger[] triggers = FindObjectsByType<trigger>(FindObjectsSortMode.None);
        if (debug) Debug.Log($"[FloorManager] Found {triggers.Length} trigger(s) in scene {scene.name}");
        foreach (trigger trigger in triggers)
        {
            if (debug) Debug.Log($"[FloorManager] Trigger found: {trigger.name} (Active: {trigger.gameObject.activeInHierarchy})");
        }
        
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

        // play music AFTER game state has been set so the correct track is chosen
        floorMusicManager.PlayForFloor(CurrentFloor);

        // reset player position whenever a new floor scene loads
        FloorSpawnContext context = BuildSpawnContext();

        hasSpawnedOnLoad = true;
        playerSpawner.Spawn(context);
        SpawnEnemies(context);
        
        // Enable combat after enemies are spawned (ensures enemies exist when combat is enabled)
        // Wait a frame to ensure all enemies are fully instantiated
        StartCoroutine(EnableCombatAfterEnemySpawn());
        
        // Check for NextLevelTrigger after spawning
        triggers = FindObjectsByType<trigger>(FindObjectsSortMode.None);
        if (debug) Debug.Log($"[FloorManager] After spawning - Found {triggers.Length} trigger(s) in scene {scene.name}");
        foreach (trigger trigger in triggers)
        {
            if (debug) Debug.Log($"[FloorManager] Trigger after spawning: {trigger.name} (Active: {trigger.gameObject.activeInHierarchy})");
        }
        
        // Check trigger status after a delay to see if it gets destroyed
        StartCoroutine(CheckTriggerStatusAfterDelay(1f));
    }

    void SpawnEnemies(FloorSpawnContext context)
    {
        // Use SpawnManager for enemy/boss spawning instead of old spawner classes
        if (SpawnManager.Instance != null)
        {
            if (debug) Debug.Log($"[FloorManager] Using SpawnManager to spawn enemies for floor {context.Floor}");
            SpawnManager.Instance.SpawnEnemies();
        }
        else
        {
            Debug.LogWarning("[FloorManager] SpawnManager.Instance is null! Cannot spawn enemies.");
        }
    }
    
    private IEnumerator EnableCombatAfterEnemySpawn()
    {
        // Wait a frame to ensure all enemies are fully instantiated and active
        yield return null;
        yield return null;
        
        Debug.Log("[FloorManager] Enabling combat after enemy spawn");
        
        // Count enemies before enabling combat
        GameObject[] enemiesBefore = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject[] bossesBefore = GameObject.FindGameObjectsWithTag("Boss");
        Debug.Log($"[FloorManager] Found {enemiesBefore.Length} enemies and {bossesBefore.Length} bosses before enabling combat");
        
        playerSpawner.ResetCombat();
        
        // Verify combat was enabled
        yield return new WaitForSeconds(0.6f); // Wait for the delay + a bit more
        GameObject[] enemiesAfter = GameObject.FindGameObjectsWithTag("Enemy");
        Debug.Log($"[FloorManager] After ResetCombat: Found {enemiesAfter.Length} enemies in scene");
        
        // Check if enemies have EnemyCombat component
        foreach (GameObject enemy in enemiesAfter)
        {
            var combat = enemy.GetComponent<EnemyCombat>();
            if (combat != null)
            {
                Debug.Log($"[FloorManager] Enemy {enemy.name} has EnemyCombat component");
            }
            else
            {
                Debug.LogWarning($"[FloorManager] Enemy {enemy.name} does NOT have EnemyCombat component!");
            }
        }
        
        // Check again after a longer delay to see if they're still enabled
        yield return new WaitForSeconds(1f);
        Debug.Log($"[FloorManager] Checking enemy combat state again after additional delay...");
        GameObject[] enemiesLater = GameObject.FindGameObjectsWithTag("Enemy");
        Debug.Log($"[FloorManager] Found {enemiesLater.Length} enemies after additional delay");
        
        // Try to enable combat one more time to ensure it's enabled
        Debug.Log("[FloorManager] Force enabling combat one more time to ensure it's active...");
        playerSpawner.ResetCombat(0f); // Enable immediately without delay
    }

    public void SetBossFloorInterval(int interval) { bossFloorInterval = interval; }

    // check if current floor is boss floor
    public bool IsBossFloor() { return CurrentFloor % bossFloorInterval == 0; }
    public bool IsFinalFloor() { return CurrentFloor == 15; }

    private IEnumerator CheckTriggerStatusAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        trigger[] triggers = FindObjectsByType<trigger>(FindObjectsSortMode.None);
        if (debug) Debug.Log($"[FloorManager] After {delay}s delay - Found {triggers.Length} trigger(s) in scene");
        foreach (trigger trigger in triggers)
        {
            if (debug) Debug.Log($"[FloorManager] Trigger after delay: {trigger.name} (Active: {trigger.gameObject.activeInHierarchy}, Destroyed: {trigger == null})");
        }
        
        // Also check by name
        GameObject triggerByName = GameObject.Find("NextLevelTrigger (1)");
        if (debug) Debug.Log($"[FloorManager] Trigger by name: {(triggerByName != null ? triggerByName.name + " (Active: " + triggerByName.activeInHierarchy + ")" : "NOT FOUND")}");
    }

}
