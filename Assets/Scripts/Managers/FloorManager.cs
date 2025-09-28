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

    // floor number
    public static event Action<int> OnFloorChanged; // notify UI 

    public int CurrentFloor { get; private set; } = 1;

    // event to tell others when an enemy appears
    public static event Action<GameObject> OnEnemySpawned;

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    void Start()
    {
        // start in gameplay (for now)
        //GameState.Instance.ChangeState(GameState.GameStates.Playing);

        StartCoroutine(SpawnPlayerCoroutine());
        SpawnEnemies();
    }

    public void LoadNextFloor()
    {
        CurrentFloor = 1;
        OnFloorChanged?.Invoke(CurrentFloor);

        // reset player using singleton
        StartCoroutine(SpawnPlayerCoroutine());

        // spawn enemies for the floor
        SpawnEnemies();
    }

    public void ResetToFloor1()
    {
        CurrentFloor = 1;
        Debug.Log("Resetting to Floor 1");

        // respawn player & enemies
        StartCoroutine(SpawnPlayerCoroutine());

        // spawn enemies
        SpawnEnemies();

        OnFloorChanged?.Invoke(CurrentFloor);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // reset player position whenever a new floor scene loads
        StartCoroutine(SpawnPlayerCoroutine());
        SpawnEnemies();
    }

    public IEnumerator SpawnPlayerCoroutine()
    {
        
        // wait until PlayerInstance exists
        while (PlayerInstance.Instance == null)
            yield return null;

        GameObject playerGO = PlayerInstance.Instance?.gameObject;
        
        if (playerGO == null)
        {
            if (playerPrefab != null)
            {
                GameObject newPlayer = Instantiate(playerPrefab, playerSpawnPoint.position, playerSpawnPoint.rotation);
                playerGO = newPlayer;
            }
            else
            {
                Debug.LogError("No Player prefab assigned to FloorManager!");
                yield break;
            }
        }

        playerGO.SetActive(true);
        
        // Reset position
        var movement = playerGO.GetComponent<PlayerMovement>();
        if (movement != null)
            movement.ResetToSpawn(playerSpawnPoint);

        // Reset health
        var health = playerGO.GetComponent<HealthSystem>();
        if (health != null)
            health.ResetHealth();

        playerGO.SetActive(true);
    }

    void SpawnEnemies()
    {
        for (int i = 0; i < enemySpawnPoints.Count; i++)
        {
            if (i < enemyPrefabsForThisFloor.Count)
            {
                GameObject prefab = enemyPrefabsForThisFloor[i];
                Transform point = enemySpawnPoints[i];

                var enemy = Instantiate(prefab, point.position, point.rotation);
                
                // fire event for each enemy
                OnEnemySpawned?.Invoke(enemy);
            }
        }
    }
}
