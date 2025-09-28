using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;

public class FloorManager : SingletonBase<FloorManager>
{
    [Header("Player")]
    public Transform playerSpawnPoint;

    [Header("Enemies")]
    public List<GameObject> enemyPrefabsForThisFloor; 
    public List<Transform> enemySpawnPoints;

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
        UIManager.Instance.ShowScreen(UIManager.MenuScreen.Gameplay);

        SpawnPlayer();
        SpawnEnemies();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // reset player position whenever a new floor scene loads
        SpawnPlayer();
        SpawnEnemies();
    }

    void SpawnPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        // player persists across floors, check if it already exists 
        if (player != null)
        {
            var movement = player.GetComponent<PlayerMovement>();
            if (movement != null)
                movement.ResetToSpawn(playerSpawnPoint);
        }
        else
            Debug.LogError("Player Instance not found.");
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
