using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpawnManager : SingletonBase<SpawnManager>
{
    #region Variables

    [Header("Player")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform playerSpawnPoint;

    [Header("Enemies")]
    public Dictionary<int, List<string>> enemiesForThisFloor = new Dictionary<int, List<string>>();
    [SerializeField] private List<Transform> enemySpawnPoints;
    
    // Cache for loaded prefabs to avoid repeated Resources.Load calls
    private Dictionary<string, GameObject> prefabCache = new Dictionary<string, GameObject>();

    [Header("Boss Settings")]
    [SerializeField] private GameObject bossPrefab;
    [SerializeField] private Transform bossSpawnPoint;
    public int bossFloorInterval = 5; // Boss every 5 floors (5, 10, 15, 20, etc.)
    public bool isFinalFloor = false;
    public bool isRunning; // track if there's a current run

    // CurrentFloor is now managed by FloorManager - access via FloorManager.Instance.CurrentFloor
    private int CurrentFloor => FloorManager.Instance != null ? FloorManager.Instance.CurrentFloor : 1;

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

    #endregion

    public GameObject GetPrefabFromName(string name)
    {
        // Check cache first
        if (prefabCache.ContainsKey(name))
        {
            return prefabCache[name];
        }

        // Try to load from Resources folder
        GameObject prefab = Resources.Load<GameObject>($"Prefabs/{name}");
        
        if (prefab == null)
        {
            // Try without Prefabs folder path
            prefab = Resources.Load<GameObject>(name);
        }

        if (prefab != null)
        {
            prefabCache[name] = prefab;
            return prefab;
        }

        if (debug)
        {
            Debug.LogWarning($"[SpawnManager] Could not find prefab with name: {name}");
        }

        return null;
    }

    public void LoadEnemySpawnData()
    {
        // tutorial floors
        // * would need a time stop system for prompting tutorial UI
        //enemiesForThisFloor.Add(1, new List<string> { "Enemy_Basic" }); // first enemy
        //enemiesForThisFloor.Add(1, new List<string> { "Enemy_Elite" }); // harder enemy
        //enemiesForThisFloor.Add(1, new List<string> { "Enemy_Boss" }); // test first easy boss

        // gameplay floors
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

    protected override void Awake()
    {
        base.Awake();
        enemySpawnPoints ??= new List<Transform>();
        LoadEnemySpawnData();
    }

    // Note: SpawnManager no longer handles scene loading or floor progression
    // FloorManager is responsible for calling SpawnEnemies() when needed

    // Refresh spawn point references after scene load (called by FloorManager)
    public void RefreshSpawnPointReferences()
    {
        if (debug) Debug.Log("[SpawnManager] Refreshing spawn point references");
        
        // Refresh enemy spawn points
        for (int i = 0; i < enemySpawnPoints.Count; i++)
        {
            if (enemySpawnPoints[i] == null)
            {
                int index = i;
                GameObject spawnPoint = GameObject.Find($"EnemySpawnPoint{index}");
                if (spawnPoint != null)
                {
                    enemySpawnPoints[index] = spawnPoint.transform;
                    if (debug) Debug.Log($"[SpawnManager] Found EnemySpawnPoint{index}");
                }
            }
        }
        
        // Refresh boss spawn point
        if (bossSpawnPoint == null)
        {
            GameObject bossSpawn = GameObject.Find("BossSpawnPoint");
            if (bossSpawn != null)
            {
                bossSpawnPoint = bossSpawn.transform;
                if (debug) Debug.Log("[SpawnManager] Found BossSpawnPoint");
            }
        }
    }

    // Public method for FloorManager to call when enemies need to be spawned
    public void SpawnEnemies()
    {
        ClearExistingEnemies();

        if (IsBossFloor())
        {
            if (bossPrefab != null)
            {
                SpawnBoss();
            }
            else
            {
                // Fallback to dictionary-based spawning if bossPrefab is null
                SpawnEnemy();
            }
        }
        else
        {
            SpawnEnemy();
        }
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
    }

    public void SetBossFloorInterval(int interval) { bossFloorInterval = interval; }

    // check if current floor is boss floor
    public bool IsBossFloor() 
    { 
        if (FloorManager.Instance != null)
        {
            return FloorManager.Instance.IsBossFloor();
        }
        return CurrentFloor % bossFloorInterval == 0; 
    }
    
    public bool IsFinalFloor() 
    { 
        if (FloorManager.Instance != null)
        {
            return FloorManager.Instance.IsFinalFloor();
        }
        return CurrentFloor == 15; 
    }

    public void SpawnEnemy()
    {
        // Get the list of enemy names for the current floor
        if (!enemiesForThisFloor.ContainsKey(CurrentFloor))
        {
            if (debug)
            {
                Debug.LogWarning($"[SpawnManager] No enemy data found for floor {CurrentFloor}");
            }
            return;
        }

        List<string> enemyNamesForFloor = enemiesForThisFloor[CurrentFloor];
        int spawnedCount = 0;

        // Spawn enemies based on the dictionary data
        for (int i = 0; i < enemyNamesForFloor.Count && i < EnemySpawnPoints.Count; i++)
        {
            string enemyName = enemyNamesForFloor[i];
            Transform point = EnemySpawnPoints[i];

            if (point == null)
            {
                if (debug)
                {
                    Debug.LogWarning($"[SpawnManager] Enemy spawn point {i} is null - skipping");
                }
                continue;
            }

            // Get prefab from name
            GameObject prefab = GetPrefabFromName(enemyName);
            if (prefab == null)
            {
                if (debug)
                {
                    Debug.LogWarning($"[SpawnManager] Could not load prefab: {enemyName} - skipping spawn");
                }
                continue;
            }

            GameObject enemy = Object.Instantiate(prefab, point.position, point.rotation);

            EnemyStatScaler scaler = enemy.GetComponent<EnemyStatScaler>();
            if (scaler != null)
            {
                scaler.ScaleToFloor(CurrentFloor);
            }

            EnemyTierVisual visual = enemy.GetComponent<EnemyTierVisual>();
            if (visual != null)
            {
                visual.RefreshVisual();
            }

            if (!enemy.activeInHierarchy)
            {
                enemy.SetActive(true);
            }

            GameEvents.EnemySpawned(enemy);
            spawnedCount++;
        }

        if (debug)
        {
            Debug.Log($"[SpawnManager] Spawned {spawnedCount} enemies for floor {CurrentFloor}");
        }
    }

    public void SpawnBoss()
    {
        GameObject bossPrefabToUse = bossPrefab;
        
        // If bossPrefab is null, try to get it from the dictionary
        if (bossPrefabToUse == null && enemiesForThisFloor.ContainsKey(CurrentFloor))
        {
            List<string> enemyNames = enemiesForThisFloor[CurrentFloor];
            if (enemyNames != null && enemyNames.Count > 0)
            {
                // Try to find "Enemy_Boss" in the list
                string bossName = enemyNames.Find(name => name.Contains("Boss"));
                if (!string.IsNullOrEmpty(bossName))
                {
                    bossPrefabToUse = GetPrefabFromName(bossName);
                }
            }
        }

        if (bossPrefabToUse == null)
        {
            if (debug)
            {
                Debug.LogWarning($"[SpawnManager] No boss prefab available for floor {CurrentFloor}");
            }
            return;
        }

        if (BossSpawnPoint == null)
        {
            if (debug)
            {
                Debug.LogWarning("[SpawnManager] BossSpawnPoint is null - cannot spawn boss");
            }
            return;
        }

        GameObject boss = Object.Instantiate(bossPrefabToUse, BossSpawnPoint.position, BossSpawnPoint.rotation);

        EnemyStatScaler scaler = boss.GetComponent<EnemyStatScaler>();
        if (scaler != null) { scaler.ScaleToFloor(CurrentFloor); }

        EnemyTierVisual visual = boss.GetComponent<EnemyTierVisual>();
        if (visual != null) { visual.RefreshVisual(); }

        if (!boss.activeInHierarchy) { boss.SetActive(true); }

        GameEvents.EnemySpawned(boss);
        
        if (debug)
        {
            Debug.Log($"[SpawnManager] Spawned boss for floor {CurrentFloor}");
        }
    }
}
