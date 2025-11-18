using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpawnManager : MonoBehaviour
{
    #region Variables

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

    #endregion

    void Awake()
    {
        enemyPrefabsForThisFloor ??= new List<GameObject>();
        enemySpawnPoints ??= new List<Transform>();
    }

    public void Start()
    {
        LoadFloor();
    }

    public void LoadFloor()
    {
        SpawnEnemies();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Check for NextLevelTrigger before doing anything else
        trigger[] triggers = FindObjectsByType<trigger>(FindObjectsSortMode.None);

        // Ensure time is running when loading a new gameplay floor
        Time.timeScale = 1f;

        // Set game state to Playing when loading a new gameplay floor
        if (GameState.Instance != null)
        {
            GameState.Instance.ChangeState(GameState.GameStates.Playing);
        }

        // play music AFTER game state has been set so the correct track is chosen
        floorMusicManager.PlayForFloor(CurrentFloor);

        // Check for NextLevelTrigger after spawning
        triggers = FindObjectsByType<trigger>(FindObjectsSortMode.None);
    }

    void SpawnEnemies()
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
    public bool IsBossFloor() { return CurrentFloor % bossFloorInterval == 0; }
    public bool IsFinalFloor() { return CurrentFloor == 15; }

    public void SpawnEnemy()
    {
        if (IsBossFloor())
        {
            return;
        }

        int spawnedCount = 0;

        for (int i = 0; i < EnemySpawnPoints.Count; i++)
        {
            if (i >= enemyPrefabsForThisFloor.Count)
                break;

            GameObject prefab = enemyPrefabsForThisFloor[i]; // needs change
            Transform point = EnemySpawnPoints[i];

            if (prefab == null)
            {
                continue;
            }

            if (point == null)
            {
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
    }

    public void SpawnBoss()
    {
        if (bossPrefab == null)
        {
            return;
        }

        if (BossSpawnPoint == null)
        {
            return;
        }

        GameObject boss = Object.Instantiate(bossPrefab, BossSpawnPoint.position, BossSpawnPoint.rotation);

        EnemyStatScaler scaler = boss.GetComponent<EnemyStatScaler>();
        if (scaler != null) { scaler.ScaleToFloor(CurrentFloor); }

        EnemyTierVisual visual = boss.GetComponent<EnemyTierVisual>();
        if (visual != null) { visual.RefreshVisual(); }

        if (!boss.activeInHierarchy) { boss.SetActive(true); }

        GameEvents.EnemySpawned(boss);
    }
}
